using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OnlineCompanionBookingSystem.Companion
{
    // Companion > Availability: the weekly schedule (day + start/end time) a companion is open for bookings.
    // Slots are added/removed here; each query is scoped to the logged-in companion's CompanionID.
    public partial class CompanionAvailability : Page
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
                BindAvailability(userId);
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

        // Session holds the UserID; the schedule tables use CompanionID, so convert first (0 = no profile found).
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

        // Lists the companion's slots, sorted Monday -> Sunday, then by start time.
        private void BindAvailability(int userId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                int companionId = GetCompanionId(connection, userId);
                if (companionId == 0)
                {
                    rptAvailability.Visible = false;
                    lblNoAvailability.Visible = true;
                    return;
                }

                const string query = @"
                    SELECT AvailabilityID, DayOfWeek, StartTime, EndTime 
                    FROM Availability 
                    WHERE CompanionID = @CompanionID 
                    ORDER BY 
                        CASE DayOfWeek 
                            WHEN 'Monday' THEN 1 
                            WHEN 'Tuesday' THEN 2 
                            WHEN 'Wednesday' THEN 3 
                            WHEN 'Thursday' THEN 4 
                            WHEN 'Friday' THEN 5 
                            WHEN 'Saturday' THEN 6 
                            WHEN 'Sunday' THEN 7 
                            ELSE 8 
                        END, StartTime";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CompanionID", companionId);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        rptAvailability.Visible = dt.Rows.Count > 0;
                        lblNoAvailability.Visible = dt.Rows.Count == 0;
                        rptAvailability.DataSource = dt;
                        rptAvailability.DataBind();
                    }
                }
            }
        }

        // Adds a slot after checking the times are valid ("HH:mm", end later than start) and that
        // the exact same slot doesn't already exist for that day.
        protected void btnAddAvailability_Click(object sender, EventArgs e)
        {
            TimeSpan startTime;
            TimeSpan endTime;
            if (!TimeSpan.TryParseExact(txtStartTime.Text.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out startTime) ||
                !TimeSpan.TryParseExact(txtEndTime.Text.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out endTime))
            {
                ShowMessage("Please provide a valid start and end time.", "error");
                return;
            }

            if (startTime >= endTime)
            {
                ShowMessage("End time must be later than start time.", "error");
                return;
            }

            try
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    int companionId = GetCompanionId(connection, userId);
                    if (companionId == 0)
                    {
                        ShowMessage("Companion profile not found.", "error");
                        return;
                    }

                    const string duplicateQuery = @"
                        SELECT COUNT(*)
                        FROM Availability
                        WHERE CompanionID = @CompanionID
                          AND DayOfWeek = @DayOfWeek
                          AND StartTime = @StartTime
                          AND EndTime = @EndTime";
                    using (SqlCommand duplicateCommand = new SqlCommand(duplicateQuery, connection))
                    {
                        duplicateCommand.Parameters.AddWithValue("@CompanionID", companionId);
                        duplicateCommand.Parameters.AddWithValue("@DayOfWeek", ddlDayOfWeek.SelectedValue);
                        duplicateCommand.Parameters.AddWithValue("@StartTime", startTime);
                        duplicateCommand.Parameters.AddWithValue("@EndTime", endTime);
                        if (Convert.ToInt32(duplicateCommand.ExecuteScalar()) > 0)
                        {
                            ShowMessage("This schedule slot already exists.", "error");
                            return;
                        }
                    }

                    const string query = "INSERT INTO Availability (CompanionID, DayOfWeek, StartTime, EndTime) VALUES (@CompanionID, @DayOfWeek, @StartTime, @EndTime)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CompanionID", companionId);
                        command.Parameters.AddWithValue("@DayOfWeek", ddlDayOfWeek.SelectedValue);
                        command.Parameters.AddWithValue("@StartTime", startTime);
                        command.Parameters.AddWithValue("@EndTime", endTime);
                        command.ExecuteNonQuery();
                    }
                }

                ShowMessage("Availability slot successfully added.", "success");
                txtStartTime.Text = string.Empty;
                txtEndTime.Text = string.Empty;
                BindAvailability(userId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Add availability error: " + ex);
                ShowMessage("We could not add the schedule slot. Please try again.", "error");
            }
        }

        // Delete button on a slot (CommandArgument = AvailabilityID). The CompanionID filter in the DELETE
        // prevents removing another companion's slot.
        protected void rptAvailability_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "DeleteSlot")
            {
                int availabilityId = Convert.ToInt32(e.CommandArgument);
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();
                        int companionId = GetCompanionId(connection, Convert.ToInt32(Session["UserID"]));
                        const string query = "DELETE FROM Availability WHERE AvailabilityID = @AvailabilityID AND CompanionID = @CompanionID";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@AvailabilityID", availabilityId);
                            command.Parameters.AddWithValue("@CompanionID", companionId);
                            if (command.ExecuteNonQuery() != 1)
                            {
                                ShowMessage("This schedule slot could not be removed.", "error");
                                return;
                            }
                        }
                    }

                    ShowMessage("Availability slot successfully removed.", "success");
                    BindAvailability(Convert.ToInt32(Session["UserID"]));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Remove availability error: " + ex);
                    ShowMessage("We could not remove the schedule slot. Please try again.", "error");
                }
            }
        }

        // Shows a stored time as 12-hour text, e.g. 14:30 -> "2:30 PM".
        protected string FormatTime(object value)
        {
            if (value == null || value == DBNull.Value) return string.Empty;

            TimeSpan time;
            if (value is TimeSpan)
            {
                time = (TimeSpan)value;
            }
            else if (!TimeSpan.TryParse(value.ToString(), out time))
            {
                return HttpUtility.HtmlEncode(value.ToString());
            }

            return DateTime.Today.Add(time).ToString("h:mm tt", CultureInfo.InvariantCulture);
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