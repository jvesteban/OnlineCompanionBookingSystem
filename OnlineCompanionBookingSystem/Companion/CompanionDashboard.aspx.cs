using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;

namespace OnlineCompanionBookingSystem.Companion
{
    // Companion > Dashboard: overview of the companion's account: verification status, booking totals,
    // average rating, unread notifications, the 5 latest bookings, and the 5 latest customer reviews.
    public partial class CompanionDashboard : Page
    {
        // Connection string from Web.config
        private string ConnectionString => ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Only authenticated Companion accounts may access this dashboard.
            if (Session["UserID"] == null || !string.Equals(Session["Role"]?.ToString(), "Companion", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // Load dashboard data once on the initial request.
                LoadCompanionDashboard(Convert.ToInt32(Session["UserID"]));
            }
        }

        // Runs every dashboard query on one shared connection. The profile is loaded first because it returns the
        // CompanionID that the other queries need (they all use CompanionID, except notifications which use UserID).
        private void LoadCompanionDashboard(int userId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                int companionId = LoadProfile(connection, userId);
                if (companionId == 0)
                {
                    ShowEmptyDashboard();
                    return;
                }

                LoadStatistics(connection, companionId);
                LoadRecentBookings(connection, companionId);
                LoadRecentRatings(connection, companionId);
                LoadUnreadNotifications(connection, userId);
            }
        }

        // Fills the greeting, avatar, and verification status. Returns the CompanionID, or 0 if no profile exists.
        private int LoadProfile(SqlConnection connection, int userId)
        {
            // Resolve the companion profile before loading companion-specific statistics.
            const string query = @"
                SELECT TOP 1 cp.CompanionID, cp.VerificationStatus, u.FullName, u.Email, u.ProfilePicture
                FROM CompanionProfiles cp
                INNER JOIN Users u ON u.UserID = cp.UserID
                WHERE cp.UserID = @UserID";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserID", userId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return 0;

                    string fullName = reader["FullName"] == DBNull.Value ? "Companion" : reader["FullName"].ToString().Trim();
                    string firstName = GetFirstName(fullName);
                    litUserFirstName.Text = HttpUtility.HtmlEncode(firstName);
                    litAccountName.Text = HttpUtility.HtmlEncode(fullName);
                    litAccountEmail.Text = HttpUtility.HtmlEncode(reader["Email"] == DBNull.Value ? "No email available" : reader["Email"].ToString());
                    litWelcomeName.Text = HttpUtility.HtmlEncode(firstName);
                    litAvatarInitial.Text = GetInitials(fullName);
                    litAvatarInitialLg.Text = GetInitials(fullName);
                    string profilePicture = reader["ProfilePicture"] == DBNull.Value
                        ? string.Empty
                        : reader["ProfilePicture"].ToString().Trim();
                    imgAvatar.Visible = !string.IsNullOrWhiteSpace(profilePicture);
                    litAvatarInitial.Visible = string.IsNullOrWhiteSpace(profilePicture);
                    if (imgAvatar.Visible)
                    {
                        imgAvatar.ImageUrl = profilePicture;
                        imgDropAvatar.ImageUrl = profilePicture;
                    }
                    imgDropAvatar.Visible = imgAvatar.Visible;
                    litAvatarInitialLg.Visible = !imgDropAvatar.Visible;
                    litVerificationStatus.Text = HttpUtility.HtmlEncode(reader["VerificationStatus"].ToString());
                    return Convert.ToInt32(reader["CompanionID"]);
                }
            }
        }

        private void LoadStatistics(SqlConnection connection, int companionId)
        {
            // Aggregate booking totals and the current average rating for the profile.
            const string query = @"
                SELECT COUNT(*) AS TotalBookings,
                       ISNULL(SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END), 0) AS PendingBookings,
                       ISNULL(SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END), 0) AS CompletedBookings,
                       ISNULL((SELECT AVG(CAST(r.Score AS DECIMAL(10, 2))) FROM Ratings r WHERE r.CompanionID = @CompanionID), 0) AS AverageRating
                FROM Bookings
                WHERE CompanionID = @CompanionID";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CompanionID", companionId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return;
                    litTotalBookings.Text = reader["TotalBookings"].ToString();
                    litPendingBookings.Text = reader["PendingBookings"].ToString();
                    litCompletedBookings.Text = reader["CompletedBookings"].ToString();
                    litAverageRating.Text = Convert.ToDecimal(reader["AverageRating"]).ToString("0.0");
                }
            }
        }

        private void LoadRecentBookings(SqlConnection connection, int companionId)
        {
            // Show only the five most recently created bookings to keep the dashboard focused.
            const string query = @"
                SELECT TOP 5 u.FullName AS CustomerName, u.ProfilePicture AS CustomerProfilePicture, p.PackageName, p.Duration, b.BookingDate, b.BookingTime, b.Status
                FROM Bookings b
                INNER JOIN Users u ON u.UserID = b.CustomerID
                INNER JOIN Packages p ON p.PackageID = b.PackageID
                WHERE b.CompanionID = @CompanionID
                ORDER BY b.DateCreated DESC";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CompanionID", companionId);
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataTable bookings = new DataTable();
                    adapter.Fill(bookings);
                    BookingStatus.Apply(bookings);   // display status (On-going, ...) worked out from the date and time
                    // Display an empty-state message when the companion has no bookings yet.
                    rptRecentBookings.Visible = bookings.Rows.Count > 0;
                    lblNoBookings.Visible = bookings.Rows.Count == 0;
                    rptRecentBookings.DataSource = bookings;
                    rptRecentBookings.DataBind();
                }
            }
        }

        private void LoadUnreadNotifications(SqlConnection connection, int userId)
        {
            // Notifications belong to the companion's UserID, not CompanionID.
            const string query = "SELECT COUNT(*) FROM Notifications WHERE UserID = @UserID AND IsRead = 0";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserID", userId);
                litUnreadNotifications.Text = Convert.ToInt32(command.ExecuteScalar()).ToString();
            }
        }

        // The 5 latest customer reviews (score, comment, date, customer name) for the "Customer Reviews" section.
        private void LoadRecentRatings(SqlConnection connection, int companionId)
        {
            const string query = @"
                SELECT TOP 5 r.Score, r.Comment, r.DateCreated, u.FullName AS CustomerName
                FROM Ratings r
                INNER JOIN Users u ON u.UserID = r.CustomerID
                WHERE r.CompanionID = @CompanionID
                ORDER BY r.DateCreated DESC";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CompanionID", companionId);
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataTable ratings = new DataTable();
                    adapter.Fill(ratings);
                    rptRecentRatings.Visible = ratings.Rows.Count > 0;
                    lblNoRatings.Visible = ratings.Rows.Count == 0;
                    rptRecentRatings.DataSource = ratings;
                    rptRecentRatings.DataBind();
                }
            }
        }

        // Used when the account has no CompanionProfiles row (should not normally happen).
        private void ShowEmptyDashboard()
        {
            litVerificationStatus.Text = "Profile unavailable";
            rptRecentBookings.Visible = false;
            lblNoBookings.Visible = true;
        }

        // 5-star display: ginto ang puno, abo ang wala (HTML entity para hindi masira ng encoding)
        protected string GetStarsHtml(object scoreObj)
        {
            int score = 0;
            if (scoreObj != null && scoreObj != DBNull.Value)
            {
                score = (int)Math.Round(Convert.ToDecimal(scoreObj));
            }
            score = Math.Max(0, Math.Min(5, score));
            return "<span class=\"stars\" aria-hidden=\"true\"><span class=\"star-on\">" + string.Concat(System.Linq.Enumerable.Repeat("&#9733;", score)) +
                   "</span><span class=\"star-off\">" + string.Concat(System.Linq.Enumerable.Repeat("&#9733;", 5 - score)) + "</span></span>";
        }

        // Initials for the avatar, e.g. "Juan Dela Cruz" -> "JC"; "C" if the name is empty.
        protected string GetInitials(object value)
        {
            string name = value == null ? "C" : value.ToString().Trim();
            if (name.Length == 0) return "C";
            string[] parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1
                ? (parts[0][0].ToString() + parts[parts.Length - 1][0]).ToUpperInvariant()
                : name.Substring(0, Math.Min(2, name.Length)).ToUpperInvariant();
        }

        protected string GetStatusClass(object value)
        {
            // Map database status values to the CSS badge classes used by the dashboard.
            string status = value == null ? string.Empty : value.ToString().ToLowerInvariant();
            return status == "completed" ? "status-completed"
                : status == "confirmed" ? "status-confirmed"
                : status == "ongoing" ? "status-ongoing"
                : status == "awaiting" ? "status-awaiting"
                : status == "cancelled" || status == "declined" ? "status-cancelled"
                : "status-pending";
        }

        // First word of the full name, used for the greeting.
        private string GetFirstName(string fullName)
        {
            string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 0 ? "Companion" : parts[0];
        }

        // Clears the session and returns to the login page.
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Account/Login.aspx");
        }
    }
}