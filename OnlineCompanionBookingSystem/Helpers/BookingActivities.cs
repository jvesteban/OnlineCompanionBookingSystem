using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace OnlineCompanionBookingSystem
{
    /// <summary>
    /// The activities a customer picked when booking a companion (one booking can have several).
    /// They are stored in the BookingActivities table as a COPY of the activity name at the time of booking,
    /// so the booking still shows what was booked even if the companion later renames or removes the activity.
    /// </summary>
    public static class BookingActivities
    {
        private static readonly object SchemaLock = new object();
        private static bool schemaReady;

        // Same script as in the "database changes" note: creates the table on first use, so no manual SQL is needed.
        private const string CreateTableSql = @"
            IF OBJECT_ID(N'dbo.BookingActivities', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.BookingActivities (
                    BookingActivityID INT IDENTITY(1,1) PRIMARY KEY,
                    BookingID         INT NOT NULL CONSTRAINT FK_BookingActivities_Bookings REFERENCES dbo.Bookings(BookingID) ON DELETE CASCADE,
                    ActivityName      NVARCHAR(150) NOT NULL
                );
                CREATE INDEX IX_BookingActivities_BookingID ON dbo.BookingActivities (BookingID);
            END";

        // The SQL that joins a booking's activities into one text, e.g. "Jogging, Walking".
        // (".value" is used because plain FOR XML PATH would turn "&" into "&amp;".)
        public const string JoinedActivitiesSql =
            "STUFF((SELECT ', ' + ba.ActivityName FROM BookingActivities ba WHERE ba.BookingID = b.BookingID " +
            "ORDER BY ba.BookingActivityID FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '')";

        // Creates the table if it does not exist (only checked once while the site is running).
        public static void EnsureSchema(SqlConnection conn, SqlTransaction tx = null)
        {
            if (schemaReady && tx == null) return;
            lock (SchemaLock)
            {
                using (var cmd = new SqlCommand(CreateTableSql, conn, tx)) cmd.ExecuteNonQuery();
                if (tx == null) schemaReady = true;
            }
        }

        // Saves the chosen activities of a new booking and returns their names.
        // Each activity must really belong to that companion; otherwise nothing is saved and an
        // InvalidOperationException is thrown (so a tampered form cannot attach someone else's activity).
        public static List<string> Save(SqlConnection conn, SqlTransaction tx, int bookingId, int companionId, IEnumerable<int> activityIds)
        {
            const string sql = @"
                INSERT INTO BookingActivities (BookingID, ActivityName)
                OUTPUT INSERTED.ActivityName
                SELECT @BookingID, ActivityName FROM Activities WHERE ActivityID = @ActivityID AND CompanionID = @CompanionID";

            var names = new List<string>();
            foreach (int activityId in activityIds)
            {
                using (var cmd = new SqlCommand(sql, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@BookingID", bookingId);
                    cmd.Parameters.AddWithValue("@ActivityID", activityId);
                    cmd.Parameters.AddWithValue("@CompanionID", companionId);
                    object name = cmd.ExecuteScalar();
                    if (name == null) throw new InvalidOperationException("Activity " + activityId + " does not belong to companion " + companionId);
                    names.Add(Convert.ToString(name));
                }
            }
            return names;
        }
    }
}
