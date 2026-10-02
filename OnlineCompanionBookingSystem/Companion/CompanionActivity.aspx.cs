using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OnlineCompanionBookingSystem.Companion
{
    // Companion > Activities: lets a verified companion add, rename, and remove the activities
    // (walking, jogging, etc.) shown on their public profile. Every query is scoped to the
    // logged-in companion's CompanionID so one companion can never edit another's list.
    public partial class CompanionActivity : Page
    {
        // Connection string from Web.config
        private string ConnectionString => ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Activities are private management functions for authenticated companions.
            if (Session["UserID"] == null || !string.Equals(Session["Role"]?.ToString(), "Companion", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadUserIdentity();
                // Fill the shared profile dropdown in the top bar (name, email, photo)
                CompanionUi.FillAccountMenu(this, Convert.ToInt32(Session["UserID"]));
                // Bind the current companion's activities only on the initial request.
                LoadActivities();
            }
        }

        // Fills the top bar: first name, avatar initials, or the profile picture if one was uploaded.
        private void LoadUserIdentity()
        {
            const string query = "SELECT FullName, ProfilePicture FROM Users WHERE UserID = @UserID";
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserID", Convert.ToInt32(Session["UserID"]));
                connection.Open();
                object result = command.ExecuteScalar();
                string fullName = result == null || result == DBNull.Value
                    ? "Companion"
                    : result.ToString().Trim();

                string[] nameParts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string firstName = nameParts.Length == 0 ? "Companion" : nameParts[0];
                litUserFirstName.Text = HttpUtility.HtmlEncode(firstName);
                litAvatarInitial.Text = GetInitials(fullName);

                // Second read of the same row to get the profile picture path (the first one only needed the name).
                using (SqlCommand pictureCommand = new SqlCommand(query, connection))
                {
                    pictureCommand.Parameters.AddWithValue("@UserID", Convert.ToInt32(Session["UserID"]));
                    using (SqlDataReader reader = pictureCommand.ExecuteReader())
                    {
                        if (reader.Read() && reader["ProfilePicture"] != DBNull.Value)
                        {
                            string profilePicture = reader["ProfilePicture"].ToString().Trim();
                            imgTopAvatar.Visible = !string.IsNullOrWhiteSpace(profilePicture);
                            litAvatarInitial.Visible = !imgTopAvatar.Visible;
                            if (imgTopAvatar.Visible) imgTopAvatar.ImageUrl = profilePicture;
                        }
                    }
                }
            }
        }

        // Initials for the avatar, e.g. "Juan Dela Cruz" -> "JC"; "C" if the name is empty.
        private string GetInitials(string fullName)
        {
            string[] nameParts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length == 0) return "C";

            return nameParts.Length > 1
                ? (nameParts[0][0].ToString() + nameParts[nameParts.Length - 1][0]).ToUpperInvariant()
                : nameParts[0].Substring(0, Math.Min(2, nameParts[0].Length)).ToUpperInvariant();
        }

        private int GetCompanionId(SqlConnection connection)
        {
            // Resolve the profile ID from the logged-in account before any activity operation.
            const string query = "SELECT TOP 1 CompanionID FROM CompanionProfiles WHERE UserID = @UserID";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserID", Convert.ToInt32(Session["UserID"]));
                object result = command.ExecuteScalar();
                return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
        }

        // Loads this companion's activities into the repeater and updates the count badge.
        private void LoadActivities()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                int companionId = GetCompanionId(connection);
                if (companionId == 0)
                {
                    ShowMessage("Your companion profile could not be found.", false);
                    return;
                }

                // Only this companion's rows are loaded and displayed.
                const string query = @"
                    SELECT ActivityID, ActivityName
                    FROM Activities
                    WHERE CompanionID = @CompanionID
                    ORDER BY ActivityName";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CompanionID", companionId);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable activities = new DataTable();
                        adapter.Fill(activities);
                        rptActivities.Visible = activities.Rows.Count > 0;
                        lblNoActivities.Visible = activities.Rows.Count == 0;
                        litActivityCount.Text = activities.Rows.Count.ToString();
                        rptActivities.DataSource = activities;
                        rptActivities.DataBind();
                    }
                }
            }
        }

        // Adds a new activity after validation and a case-insensitive duplicate check.
        protected void btnAddActivity_Click(object sender, EventArgs e)
        {
            Page.Validate();
            if (!Page.IsValid) return;

            string activityName = txtActivityName.Text.Trim();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                int companionId = GetCompanionId(connection);
                if (companionId == 0)
                {
                    ShowMessage("Your companion profile could not be found.", false);
                    return;
                }

                // Prevent duplicate activity names for the same companion.
                const string duplicateQuery = @"
                    SELECT COUNT(*) FROM Activities
                    WHERE CompanionID = @CompanionID
                      AND LOWER(LTRIM(RTRIM(ActivityName))) = LOWER(@ActivityName)";
                using (SqlCommand duplicateCommand = new SqlCommand(duplicateQuery, connection))
                {
                    duplicateCommand.Parameters.AddWithValue("@CompanionID", companionId);
                    duplicateCommand.Parameters.AddWithValue("@ActivityName", activityName);
                    if (Convert.ToInt32(duplicateCommand.ExecuteScalar()) > 0)
                    {
                        ShowMessage("This activity is already in your list.", false);
                        return;
                    }
                }

                const string insertQuery = "INSERT INTO Activities (CompanionID, ActivityName) VALUES (@CompanionID, @ActivityName)";
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@CompanionID", companionId);
                    command.Parameters.AddWithValue("@ActivityName", activityName);
                    command.ExecuteNonQuery();
                }
            }

            txtActivityName.Text = string.Empty;
            ShowMessage("Activity added successfully.", true);
            LoadActivities();
        }

        // Handles the Save (rename) and Delete buttons on each activity row (CommandArgument = ActivityID).
        protected void rptActivities_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            // Recheck CompanionID in every update/delete request to prevent cross-account edits.
            if (e.CommandName != "SaveActivity" && e.CommandName != "DeleteActivity") return;

            int activityId;
            if (!int.TryParse(e.CommandArgument.ToString(), out activityId)) return;

            TextBox editBox = e.Item.FindControl("txtEditActivity") as TextBox;
            string activityName = editBox == null ? string.Empty : editBox.Text.Trim();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                int companionId = GetCompanionId(connection);
                if (companionId == 0) return;

                if (e.CommandName == "DeleteActivity")
                {
                    const string deleteQuery = "DELETE FROM Activities WHERE ActivityID = @ActivityID AND CompanionID = @CompanionID";
                    ExecuteActivityCommand(connection, deleteQuery, activityId, companionId, null);
                    ShowMessage("Activity removed from your profile.", true);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(activityName))
                    {
                        ShowMessage("Activity name cannot be blank.", false);
                        return;
                    }

                    const string duplicateQuery = @"
                        SELECT COUNT(*) FROM Activities
                        WHERE CompanionID = @CompanionID
                          AND ActivityID <> @ActivityID
                          AND LOWER(LTRIM(RTRIM(ActivityName))) = LOWER(@ActivityName)";
                    using (SqlCommand duplicateCommand = new SqlCommand(duplicateQuery, connection))
                    {
                        duplicateCommand.Parameters.AddWithValue("@CompanionID", companionId);
                        duplicateCommand.Parameters.AddWithValue("@ActivityID", activityId);
                        duplicateCommand.Parameters.AddWithValue("@ActivityName", activityName);
                        if (Convert.ToInt32(duplicateCommand.ExecuteScalar()) > 0)
                        {
                            ShowMessage("This activity is already in your list.", false);
                            return;
                        }
                    }

                    const string updateQuery = @"
                        UPDATE Activities
                        SET ActivityName = @ActivityName
                        WHERE ActivityID = @ActivityID AND CompanionID = @CompanionID";
                    ExecuteActivityCommand(connection, updateQuery, activityId, companionId, activityName);
                    ShowMessage("Activity updated successfully.", true);
                }
            }

            LoadActivities();
        }

        // Runs an UPDATE or DELETE on one activity. The query must filter by both ActivityID and CompanionID;
        // @ActivityName is only added when renaming.
        private void ExecuteActivityCommand(SqlConnection connection, string query, int activityId, int companionId, string activityName)
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ActivityID", activityId);
                command.Parameters.AddWithValue("@CompanionID", companionId);
                if (activityName != null) command.Parameters.AddWithValue("@ActivityName", activityName);
                command.ExecuteNonQuery();
            }
        }

        private void ShowMessage(string message, bool success)
        {
            // Encode feedback before rendering it in the page.
            pnlMessage.CssClass = success ? "activity-message success" : "activity-message error";
            litMessage.Text = HttpUtility.HtmlEncode(message);
            pnlMessage.Visible = true;
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
