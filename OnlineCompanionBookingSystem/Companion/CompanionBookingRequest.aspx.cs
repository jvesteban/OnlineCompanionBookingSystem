using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OnlineCompanionBookingSystem.Companion
{
    // Companion > Booking Requests: shows every booking made with this companion and lets them
    // Accept (-> Confirmed) or Decline (-> Declined) the ones that are still Pending.
    public partial class BookingRequests : Page
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
                BindBookingRequests(userId);

                // The companion has now seen the requests, so the sidebar's "new" counter goes away
                NavCounts.MarkSeen(NavCounts.Companion, userId, "requests");
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

        // Lists all bookings for this companion, newest first (any status).
        private void BindBookingRequests(int userId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                BookingActivities.EnsureSchema(connection);   // the query below reads the BookingActivities table
                int companionId = GetCompanionId(connection, userId);
                if (companionId == 0)
                {
                    rptBookingRequests.Visible = false;
                    lblNoRequests.Visible = true;
                    return;
                }

                const string query = @"
                    SELECT b.BookingID, u.FullName AS CustomerName, u.ProfilePicture AS CustomerProfilePicture, p.PackageName, p.Duration, b.BookingDate, b.BookingTime, b.Status, " + BookingActivities.JoinedActivitiesSql + @" AS Activities
                    FROM Bookings b
                    INNER JOIN Users u ON u.UserID = b.CustomerID
                    INNER JOIN Packages p ON p.PackageID = b.PackageID
                    WHERE b.CompanionID = @CompanionID
                    ORDER BY b.BookingID DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CompanionID", companionId);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        BookingStatus.Apply(dt);   // adds the display status (On-going, ...) worked out from the date and time
                        rptBookingRequests.Visible = dt.Rows.Count > 0;
                        lblNoRequests.Visible = dt.Rows.Count == 0;
                        rptBookingRequests.DataSource = dt;
                        rptBookingRequests.DataBind();
                    }
                }
            }
        }

        // Accept / Decline button on a booking row (CommandArgument = BookingID).
        // The status change and the customer's notification are saved in one transaction, so either
        // both happen or neither does. The "Status = 'Pending'" checks stop a request from being
        // answered twice (for example from two browser tabs).
        protected void rptBookingRequests_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int bookingId = Convert.ToInt32(e.CommandArgument);
            string newStatus = string.Empty;

            if (e.CommandName == "AcceptBooking")
            {
                newStatus = "Confirmed";
            }
            else if (e.CommandName == "DeclineBooking")
            {
                newStatus = "Declined";
            }

            if (!string.IsNullOrEmpty(newStatus))
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();
                        int companionId = GetCompanionId(connection, Convert.ToInt32(Session["UserID"]));
                        using (SqlTransaction transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                // Step 1: confirm the booking belongs to this companion and is still pending,
                                // and get the customer's ID (to notify them) and the companion's name (for the message).
                                int customerId;
                                string companionName;
                                const string customerQuery = @"
                                    SELECT b.CustomerID, u.FullName
                                    FROM Bookings b
                                    INNER JOIN CompanionProfiles cp ON cp.CompanionID = b.CompanionID
                                    INNER JOIN Users u ON u.UserID = cp.UserID
                                    WHERE b.BookingID = @BookingID
                                      AND b.CompanionID = @CompanionID
                                      AND b.Status = 'Pending'";
                                using (SqlCommand customerCommand = new SqlCommand(customerQuery, connection, transaction))
                                {
                                    customerCommand.Parameters.AddWithValue("@BookingID", bookingId);
                                    customerCommand.Parameters.AddWithValue("@CompanionID", companionId);
                                    using (SqlDataReader customerReader = customerCommand.ExecuteReader())
                                    {
                                        if (!customerReader.Read())
                                        {
                                            transaction.Rollback();
                                            ShowMessage("This booking request is no longer pending.", "error");
                                            return;
                                        }

                                        customerId = Convert.ToInt32(customerReader["CustomerID"]);
                                        companionName = customerReader["FullName"] == DBNull.Value
                                            ? "your companion"
                                            : customerReader["FullName"].ToString().Trim();
                                    }
                                }

                                // Step 2: change the booking status (Confirmed or Declined).
                                const string updateQuery = @"
                                    UPDATE Bookings
                                    SET Status = @Status
                                    WHERE BookingID = @BookingID
                                      AND CompanionID = @CompanionID
                                      AND Status = 'Pending'";
                                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection, transaction))
                                {
                                    updateCommand.Parameters.AddWithValue("@Status", newStatus);
                                    updateCommand.Parameters.AddWithValue("@BookingID", bookingId);
                                    updateCommand.Parameters.AddWithValue("@CompanionID", companionId);
                                    if (updateCommand.ExecuteNonQuery() != 1)
                                    {
                                        transaction.Rollback();
                                        ShowMessage("This booking request is no longer pending.", "error");
                                        return;
                                    }
                                }

                                // Step 3: tell the customer the result (shows on their Notifications page).
                                string notificationMessage = newStatus == "Confirmed"
                                    ? "Your booking with " + companionName + " has been confirmed."
                                    : "Your companion booking request was declined.";
                                const string notificationQuery = @"
                                    INSERT INTO Notifications (UserID, Message, IsRead, DateCreated, RelatedBookingID)
                                    VALUES (@UserID, @Message, 0, GETDATE(), @RelatedBookingID)";
                                using (SqlCommand notificationCommand = new SqlCommand(notificationQuery, connection, transaction))
                                {
                                    notificationCommand.Parameters.AddWithValue("@UserID", customerId);
                                    notificationCommand.Parameters.AddWithValue("@Message", notificationMessage);
                                    notificationCommand.Parameters.AddWithValue("@RelatedBookingID", bookingId);
                                    notificationCommand.ExecuteNonQuery();
                                }

                                transaction.Commit();
                            }
                            catch
                            {
                                transaction.Rollback();
                                throw;
                            }
                        }
                    }

                    ShowMessage($"Booking request has been successfully {newStatus.ToLowerInvariant()}.", "success");
                    BindBookingRequests(Convert.ToInt32(Session["UserID"]));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Booking request update error: " + ex);
                    ShowMessage("We could not update the booking request. Please try again.", "error");
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
                : status == "cancelled" || status == "declined" ? "status-cancelled"
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