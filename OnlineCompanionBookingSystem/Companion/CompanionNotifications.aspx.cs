using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OnlineCompanionBookingSystem.Companion
{
    // Companion > Notifications: in-app messages for the companion (new booking requests, new ratings,
    // verification results). Notifications belong to the companion's UserID, not CompanionID.
    public partial class CompanionNotifications : Page
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
                BindNotifications(userId);

                // The companion has now seen the notifications, so the sidebar's "new" counter goes away
                // (the notifications themselves stay "unread" until marked as read)
                NavCounts.MarkSeen(NavCounts.Companion, userId, "notifications");
            }
        }

        // Fills the top bar: first name, avatar initials, or the profile picture if one was uploaded.
        private void LoadCompanionProfile(int userId)
        {
            const string query = @"
                SELECT TOP 1 u.FullName, u.ProfilePicture
                FROM Users u
                WHERE u.UserID = @UserID";

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

        // Shows this user's notifications, newest first (higher ID = newer).
        private void BindNotifications(int userId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                const string query = @"
                    SELECT NotificationID, Message, IsRead, DateCreated
                    FROM Notifications
                    WHERE UserID = @UserID
                    ORDER BY NotificationID DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        rptNotifications.Visible = dt.Rows.Count > 0;
                        lblNoNotifications.Visible = dt.Rows.Count == 0;
                        rptNotifications.DataSource = dt;
                        rptNotifications.DataBind();
                    }
                }
            }
        }

        // "Mark as read" on a single notification (CommandArgument = NotificationID). The UserID filter
        // makes sure a companion can only change their own notifications.
        protected void rptNotifications_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "MarkAsRead")
            {
                int notificationId = Convert.ToInt32(e.CommandArgument);
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();
                        const string query = @"
                            UPDATE Notifications
                            SET IsRead = 1
                            WHERE NotificationID = @NotificationID
                              AND UserID = @UserID";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@NotificationID", notificationId);
                            command.Parameters.AddWithValue("@UserID", Convert.ToInt32(Session["UserID"]));
                            if (command.ExecuteNonQuery() != 1)
                            {
                                ShowMessage("This notification is no longer available.", "error");
                                return;
                            }
                        }
                    }

                    BindNotifications(Convert.ToInt32(Session["UserID"]));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Mark notification as read error: " + ex);
                    ShowMessage("We could not update the notification. Please try again.", "error");
                }
            }
        }

        // Marks every unread notification of the logged-in companion as read.
        protected void btnMarkAllRead_Click(object sender, EventArgs e)
        {
            try
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    const string query = "UPDATE Notifications SET IsRead = 1 WHERE UserID = @UserID AND IsRead = 0";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userId);
                        command.ExecuteNonQuery();
                    }
                }

                ShowMessage("All notifications marked as read.", "success");
                BindNotifications(userId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Mark all notifications as read error: " + ex);
                ShowMessage("We could not update your notifications. Please try again.", "error");
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