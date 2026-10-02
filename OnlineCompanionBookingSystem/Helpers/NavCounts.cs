using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace OnlineCompanionBookingSystem
{
    /// <summary>
    /// The red "new" counters next to some sidebar items (like Facebook's notification badges).
    ///
    /// A counter only shows what is NEW to the user, and it disappears once they open that page:
    ///   Companion: Booking Requests (new requests), My Bookings (new confirmed/completed), Notifications
    ///   Customer : My Bookings (bookings the companion confirmed/declined/completed), Notifications
    ///   Admin    : Verification Requests (new applications)
    /// Everything else (Dashboard, Activities, Packages, Profile, ...) deliberately has no counter.
    ///
    /// How "new" is decided: every section has a list of "items" (ItemID + Marker). The Marker is the item's state
    /// (for a booking: its Status). When the user opens a page, the current items and their markers are saved in the
    /// NavSeen table. An item counts as new when it is not in NavSeen, or when its Marker has changed since
    /// (for example a booking went from Pending to Confirmed). This needs no extra columns on the existing tables.
    /// </summary>
    public static class NavCounts
    {
        public const string Customer = "Customer";
        public const string Companion = "Companion";
        public const string Admin = "Admin";

        private static readonly object SchemaLock = new object();
        private static bool schemaReady;

        // Creates the NavSeen table the first time it is needed (no manual SQL script required).
        private const string CreateTableSql = @"
            IF OBJECT_ID(N'dbo.NavSeen', N'U') IS NULL
            CREATE TABLE dbo.NavSeen (
                UserID  INT         NOT NULL CONSTRAINT FK_NavSeen_Users REFERENCES dbo.Users(UserID) ON DELETE CASCADE,
                Section VARCHAR(30) NOT NULL,
                ItemID  INT         NOT NULL,
                Marker  VARCHAR(30) NOT NULL,
                CONSTRAINT PK_NavSeen PRIMARY KEY (UserID, Section, ItemID)
            );";

        // Which sections each role has. These names are also the data-nav-count="..." values in the page markup.
        private static string[] SectionsFor(string role)
        {
            if (Same(role, Companion)) return new[] { "requests", "bookings", "notifications" };
            if (Same(role, Customer)) return new[] { "bookings", "notifications" };
            if (Same(role, Admin)) return new[] { "verification" };
            return null;
        }

        // The "items" of a section, as "SELECT ItemID, Marker ...". Only fixed text is used here (the section and role
        // are checked against SectionsFor first), so nothing a user types ever ends up inside this SQL.
        private static string ItemsSql(string role, string section)
        {
            const string unreadNotifications =
                "SELECT NotificationID AS ItemID, CAST('' AS VARCHAR(30)) AS Marker FROM Notifications WHERE UserID = @UserID AND IsRead = 0";

            if (Same(role, Companion))
            {
                switch (section)
                {
                    case "requests":
                        return "SELECT BookingID AS ItemID, CAST(Status AS VARCHAR(30)) AS Marker FROM Bookings WHERE CompanionID = @CompanionID AND Status = 'Pending'";
                    case "bookings":
                        return "SELECT BookingID AS ItemID, CAST(Status AS VARCHAR(30)) AS Marker FROM Bookings WHERE CompanionID = @CompanionID AND Status IN ('Confirmed', 'Completed')";
                    case "notifications":
                        return unreadNotifications;
                }
            }
            else if (Same(role, Customer))
            {
                switch (section)
                {
                    // News from the companion: confirmed, declined, or completed (the customer's own cancel is not news)
                    case "bookings":
                        return "SELECT BookingID AS ItemID, CAST(Status AS VARCHAR(30)) AS Marker FROM Bookings WHERE CustomerID = @UserID AND Status IN ('Confirmed', 'Declined', 'Completed')";
                    case "notifications":
                        return unreadNotifications;
                }
            }
            else if (Same(role, Admin) && section == "verification")
            {
                return "SELECT CompanionID AS ItemID, CAST(VerificationStatus AS VARCHAR(30)) AS Marker FROM CompanionProfiles WHERE VerificationStatus = 'Pending'";
            }
            return null;
        }

        // Companion pages use CompanionID, but the session only has the UserID, so look it up first.
        private static string Prefix(string role)
        {
            return Same(role, Companion)
                ? "DECLARE @CompanionID INT = (SELECT TOP 1 CompanionID FROM CompanionProfiles WHERE UserID = @UserID); "
                : string.Empty;
        }

        // Number of NEW items per section (new = never seen, or its Marker changed since it was seen).
        // Returns null for an unknown role. connectionString is optional (only used by tests).
        public static List<KeyValuePair<string, int>> Compute(string role, int userId, string connectionString = null)
        {
            if (SectionsFor(role) == null) return null;
            using (var conn = OpenConnection(connectionString))
            {
                EnsureSchema(conn, null);
                return ComputeCore(conn, null, role, userId);
            }
        }

        // Same as Compute, but on a connection/transaction that the caller already has (used by tests).
        public static List<KeyValuePair<string, int>> ComputeCore(SqlConnection conn, SqlTransaction tx, string role, int userId)
        {
            string[] sections = SectionsFor(role);
            var sql = new StringBuilder(Prefix(role)).Append("SELECT ");
            for (int i = 0; i < sections.Length; i++)
            {
                if (i > 0) sql.Append(", ");
                sql.Append("(SELECT COUNT(*) FROM (").Append(ItemsSql(role, sections[i])).Append(") i ")
                   .Append("LEFT JOIN NavSeen s ON s.UserID = @UserID AND s.Section = '").Append(sections[i]).Append("' AND s.ItemID = i.ItemID ")
                   .Append("WHERE s.ItemID IS NULL OR s.Marker <> i.Marker) AS [").Append(sections[i]).Append("]");
            }

            var result = new List<KeyValuePair<string, int>>();
            using (var cmd = new SqlCommand(sql.ToString(), conn, tx))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            result.Add(new KeyValuePair<string, int>(reader.GetName(i), reader.IsDBNull(i) ? 0 : Convert.ToInt32(reader.GetValue(i))));
                        }
                    }
                }
            }
            return result;
        }

        // Call this when the user opens the page of a section: everything currently in that section counts as seen,
        // so its counter goes away. Never throws (a counter problem must not break the page).
        public static void MarkSeen(string role, int userId, string section, string connectionString = null)
        {
            try
            {
                using (var conn = OpenConnection(connectionString))
                {
                    EnsureSchema(conn, null);
                    MarkSeenCore(conn, null, role, userId, section);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("NavCounts.MarkSeen error: " + ex);
            }
        }

        // Same as MarkSeen, but on a connection/transaction that the caller already has (used by tests).
        public static void MarkSeenCore(SqlConnection conn, SqlTransaction tx, string role, int userId, string section)
        {
            string items = Array.IndexOf(SectionsFor(role) ?? new string[0], section) >= 0 ? ItemsSql(role, section) : null;
            if (items == null) return;

            // Replace what was saved for this section with the items that exist right now.
            string sql = Prefix(role) +
                "DELETE FROM NavSeen WHERE UserID = @UserID AND Section = @Section; " +
                "INSERT INTO NavSeen (UserID, Section, ItemID, Marker) SELECT @UserID, @Section, ItemID, Marker FROM (" + items + ") i;";
            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Section", section);
                cmd.ExecuteNonQuery();
            }
        }

        // Creates the NavSeen table if it does not exist yet (only checked once while the site is running).
        public static void EnsureSchema(SqlConnection conn, SqlTransaction tx)
        {
            if (schemaReady && tx == null) return;
            lock (SchemaLock)
            {
                using (var cmd = new SqlCommand(CreateTableSql, conn, tx))
                {
                    cmd.ExecuteNonQuery();
                }
                if (tx == null) schemaReady = true;
            }
        }

        // {"requests":{"n":2},"notifications":{"n":1}}  (only our own fixed names and integers, so no escaping is needed)
        public static string ToJson(List<KeyValuePair<string, int>> counts)
        {
            var sb = new StringBuilder("{");
            for (int i = 0; i < counts.Count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append('"').Append(counts[i].Key).Append("\":{\"n\":").Append(counts[i].Value).Append('}');
            }
            return sb.Append('}').ToString();
        }

        private static SqlConnection OpenConnection(string connectionString)
        {
            var conn = new SqlConnection(connectionString ?? ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString);
            conn.Open();
            return conn;
        }

        private static bool Same(string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Handlers/NavCounts.ashx: returns the sidebar counters of the logged-in user as JSON.
    /// Only the session's own user and role are used, so nobody can ask for another user's numbers.
    /// </summary>
    public class NavCountsHandler : IHttpHandler, IReadOnlySessionState
    {
        public bool IsReusable { get { return true; } }

        public void ProcessRequest(HttpContext context)
        {
            var response = context.Response;
            response.ContentType = "application/json";
            // Never cache: the numbers must be fresh every time the page asks.
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();

            var session = context.Session;
            if (session == null || session["UserID"] == null || session["Role"] == null)
            {
                response.StatusCode = 401;
                response.Write("{}");
                return;
            }

            try
            {
                int userId = Convert.ToInt32(session["UserID"]);
                var counts = NavCounts.Compute(session["Role"].ToString().Trim(), userId);
                if (counts == null)
                {
                    response.StatusCode = 403;
                    response.Write("{}");
                    return;
                }
                response.Write(NavCounts.ToJson(counts));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("NavCounts error: " + ex);
                response.StatusCode = 500;
                response.Write("{}");
            }
        }
    }
}
