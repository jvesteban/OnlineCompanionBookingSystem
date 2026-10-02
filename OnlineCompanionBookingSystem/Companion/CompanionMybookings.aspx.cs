using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OnlineCompanionBookingSystem.Companion
{
    // Companion > My Bookings: bookings the companion already accepted (Confirmed) or finished (Completed).
    // A confirmed booking can be marked as Completed after the activity; this is what lets the customer rate it.
    public partial class CompanionMybookings : Page
    {
        // Connection string from Web.config
        private string ConnectionString => ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Only logged-in companions may open this page; everyone else goes back to Login.
            if (Session["UserID"] == null || !string.Equals(Session["Role"]?.ToString(), "Companion", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                LoadCompanionProfile(userId);
                // Fill the shared profile dropdown in the top bar (name, email, photo)
                CompanionUi.FillAccountMenu(this, userId);
                BindMyBookings(userId);

                // The companion has now seen these bookings, so the sidebar's "new" counter goes away
                NavCounts.MarkSeen(NavCounts.Companion, userId, "bookings");
            }
        }

        // Fills the top bar: first name, avatar initials, or the profile picture if one was uploaded.
        private void LoadCompanionProfile(int userId)
        {
            const string query = @"
                SELECT TOP 1 cp.CompanionID, u.FullName, u.ProfilePicture
                FROM CompanionProfiles cp
                INNER JOIN Users u ON u.UserID = cp.UserID
                WHERE cp.UserID = @UserID";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read()) return;

                        string fullName = reader["FullName"] == DBNull.Value ? "Companion" : reader["FullName"].ToString().Trim();
                        string firstName = GetFirstName(fullName);
                        litUserFirstName.Text = HttpUtility.HtmlEncode(firstName);
                        litAvatarInitial.Text = GetInitials(fullName);

                        string profilePicture = reader["ProfilePicture"] == DBNull.Value
                            ? string.Empty
                            : reader["ProfilePicture"].ToString().Trim();
                        imgAvatar.Visible = !string.IsNullOrWhiteSpace(profilePicture);
                        litAvatarInitial.Visible = string.IsNullOrWhiteSpace(profilePicture);
                        if (imgAvatar.Visible)
                        {
                            imgAvatar.ImageUrl = profilePicture;
                        }
                    }
                }
            }
        }

        // Session holds the UserID; bookings use CompanionID, so convert first (0 = no profile found).
        private int GetCompanionId(SqlConnection connection, int userId)
        {
            const string query = "SELECT TOP 1 CompanionID FROM CompanionProfiles WHERE UserID = @UserID";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserID", userId);
                object result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        // Lists only Confirmed and Completed bookings (pending/declined ones are on the Booking Requests page).
        private void BindMyBookings(int userId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                BookingActivities.EnsureSchema(connection);   // the query below reads the BookingActivities table
                int companionId = GetCompanionId(connection, userId);
                if (companionId == 0)
                {
                    rptMyBookings.Visible = false;
                    lblNoBookings.Visible = true;
                    return;
                }

                const string query = @"
                    SELECT b.BookingID, u.FullName AS CustomerName, u.ProfilePicture AS CustomerProfilePicture, p.PackageName, p.Duration, b.BookingDate, b.BookingTime, b.Status, " + BookingActivities.JoinedActivitiesSql + @" AS Activities
                    FROM Bookings b
                    INNER JOIN Users u ON u.UserID = b.CustomerID
                    INNER JOIN Packages p ON p.PackageID = b.PackageID
                    WHERE b.CompanionID = @CompanionID AND b.Status IN ('Confirmed', 'Completed')
                    ORDER BY b.BookingID DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CompanionID", companionId);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        BookingStatus.Apply(dt);   // adds the display status (On-going, ...) worked out from the date and time
                        rptMyBookings.Visible = dt.Rows.Count > 0;
                        lblNoBookings.Visible = dt.Rows.Count == 0;
                        rptMyBookings.DataSource = dt;
                        rptMyBookings.DataBind();
                    }
                }
            }
        }

        // "Mark as completed" button (CommandArgument = BookingID). The WHERE clause only matches this companion's
        // own Confirmed bookings, so exactly 1 row must change; anything else means it is no longer available.
        protected void rptMyBookings_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "CompleteBooking")
            {
                int bookingId = Convert.ToInt32(e.CommandArgument);
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();
                        const string query = @"
                            UPDATE Bookings
                            SET Status = 'Completed'
                            WHERE BookingID = @BookingID
                              AND CompanionID = @CompanionID
                              AND Status = 'Confirmed'";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@BookingID", bookingId);
                            command.Parameters.AddWithValue("@CompanionID", GetCompanionId(connection, Convert.ToInt32(Session["UserID"])));
                            if (command.ExecuteNonQuery() != 1)
                            {
                                ShowMessage("This booking is no longer available to complete.", "error");
                                return;
                            }
                        }
                    }

                    ShowMessage("Booking has been successfully marked as completed.", "success");
                    BindMyBookings(Convert.ToInt32(Session["UserID"]));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Complete booking error: " + ex);
                    ShowMessage("We could not update the booking status. Please try again.", "error");
                }
            }
        }

        // Shows the green/red banner. The text is HTML-encoded before it is rendered.
        private void ShowMessage(string message, string type)
        {
            lblMessage.Text = HttpUtility.HtmlEncode(message);
            lblMessage.CssClass = type == "success" ? "message-banner message-success" : "message-banner message-error";
            lblMessage.Visible = true;
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

        // CSS class (color) of the status badge for a booking.
        protected string GetStatusClass(object value)
        {
            string status = value == null ? string.Empty : value.ToString().ToLowerInvariant();
            return status == "completed" ? "status-completed"
                : status == "confirmed" ? "status-confirmed"
                : status == "ongoing" ? "status-ongoing"
                : status == "awaiting" ? "status-awaiting"
                : "status-pending";
        }

        // First word of the full name, used for the greeting in the top bar.
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