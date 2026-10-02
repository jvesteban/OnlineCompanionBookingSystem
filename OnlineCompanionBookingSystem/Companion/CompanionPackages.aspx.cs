using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OnlineCompanionBookingSystem.Companion
{
    // Companion > Packages: the service packages (name, duration, rate) that customers can book.
    // Companions can add, edit, and delete their own packages; a package already used by a booking cannot be deleted.
    public partial class CompanionPackages : Page
    {
        // Connection string from Web.config
        private string ConnectionString => ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Only the owner of the companion profile may manage its packages.
            if (Session["UserID"] == null || !string.Equals(Session["Role"]?.ToString(), "Companion", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // Load identity and package data only during the initial page request.
                LoadUserIdentity();
                // Fill the shared profile dropdown in the top bar (name, email, photo)
                CompanionUi.FillAccountMenu(this, Convert.ToInt32(Session["UserID"]));
                LoadPackages();
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
                string fullName = result == null || result == DBNull.Value ? "Companion" : result.ToString().Trim();
                string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                litUserFirstName.Text = HttpUtility.HtmlEncode(parts.Length == 0 ? "Companion" : parts[0]);
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
            string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "C";
            return parts.Length > 1
                ? (parts[0][0].ToString() + parts[parts.Length - 1][0]).ToUpperInvariant()
                : parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();
        }

        private int GetCompanionId(SqlConnection connection)
        {
            // Resolve the companion profile from the authenticated UserID.
            const string query = "SELECT TOP 1 CompanionID FROM CompanionProfiles WHERE UserID = @UserID";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserID", Convert.ToInt32(Session["UserID"]));
                object result = command.ExecuteScalar();
                return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
        }

        // Loads this companion's packages into the repeater and updates the count badge.
        private void LoadPackages()
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

                // Filter by CompanionID so one companion cannot see another companion's packages.
                const string query = @"
                    SELECT PackageID, PackageName, Description, Duration, Rate
                    FROM Packages
                    WHERE CompanionID = @CompanionID
                    ORDER BY PackageName";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CompanionID", companionId);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable packages = new DataTable();
                        adapter.Fill(packages);
                        rptPackages.Visible = packages.Rows.Count > 0;
                        lblNoPackages.Visible = packages.Rows.Count == 0;
                        litPackageCount.Text = packages.Rows.Count.ToString();
                        rptPackages.DataSource = packages;
                        rptPackages.DataBind();
                    }
                }
            }
        }

        // Adds a new package. Duration is limited to 1-30 (hours or days) and the rate must not be negative.
        protected void btnAddPackage_Click(object sender, EventArgs e)
        {
            Page.Validate();
            if (!Page.IsValid) return;

            // Validate numeric values server-side because browser validation can be bypassed.
            int duration;
            decimal rate;
            if (!int.TryParse(txtDuration.Text, out duration) || duration < 1 || duration > 30 ||
                !decimal.TryParse(txtRate.Text, out rate) || rate < 0)
            {
                ShowMessage("Enter a valid duration and Philippine peso rate.", false);
                return;
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                int companionId = GetCompanionId(connection);
                if (companionId == 0)
                {
                    ShowMessage("Your companion profile could not be found.", false);
                    return;
                }

                const string insertQuery = @"
                    INSERT INTO Packages (CompanionID, PackageName, Description, Duration, Rate)
                    VALUES (@CompanionID, @PackageName, @Description, @Duration, @Rate)";
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@CompanionID", companionId);
                    command.Parameters.AddWithValue("@PackageName", txtPackageName.Text.Trim());
                    command.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(txtDescription.Text) ? (object)DBNull.Value : txtDescription.Text.Trim());
                    command.Parameters.AddWithValue("@Duration", FormatDuration(duration, ddlDurationUnit.SelectedValue));
                    command.Parameters.AddWithValue("@Rate", rate);
                    command.ExecuteNonQuery();
                }
            }

            ClearPackageForm();
            ShowMessage("Package added successfully.", true);
            LoadPackages();
        }

        // Save (edit) and Delete buttons on a package row (CommandArgument = PackageID).
        protected void rptPackages_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            // The package ID comes from the row, but ownership is checked again in every query.
            if (e.CommandName != "SavePackage" && e.CommandName != "DeletePackage") return;

            int packageId;
            if (!int.TryParse(e.CommandArgument.ToString(), out packageId)) return;

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                int companionId = GetCompanionId(connection);
                if (companionId == 0) return;

                if (e.CommandName == "DeletePackage")
                {
                    // Preserve referential integrity by blocking deletion of packages used by bookings.
                    const string bookingCheckQuery = "SELECT COUNT(*) FROM Bookings WHERE PackageID = @PackageID AND CompanionID = @CompanionID";
                    using (SqlCommand bookingCheck = new SqlCommand(bookingCheckQuery, connection))
                    {
                        bookingCheck.Parameters.AddWithValue("@PackageID", packageId);
                        bookingCheck.Parameters.AddWithValue("@CompanionID", companionId);
                        if (Convert.ToInt32(bookingCheck.ExecuteScalar()) > 0)
                        {
                            ShowMessage("This package cannot be removed because it is linked to an existing booking.", false);
                            return;
                        }
                    }

                    const string deleteQuery = "DELETE FROM Packages WHERE PackageID = @PackageID AND CompanionID = @CompanionID";
                    ExecutePackageCommand(connection, deleteQuery, packageId, companionId, null, null, null);
                    ShowMessage("Package removed successfully.", true);
                }
                else
                {
                    // Read the edited values from the current Repeater row.
                    TextBox nameBox = e.Item.FindControl("txtEditPackageName") as TextBox;
                    TextBox descriptionBox = e.Item.FindControl("txtEditDescription") as TextBox;
                    TextBox durationBox = e.Item.FindControl("txtEditDuration") as TextBox;
                    DropDownList durationUnitBox = e.Item.FindControl("ddlEditDurationUnit") as DropDownList;
                    TextBox rateBox = e.Item.FindControl("txtEditRate") as TextBox;
                    int duration;
                    decimal rate;

                    if (nameBox == null || string.IsNullOrWhiteSpace(nameBox.Text) ||
                        !int.TryParse(durationBox == null ? string.Empty : durationBox.Text, out duration) ||
                        duration < 1 || duration > 30 ||
                        !decimal.TryParse(rateBox == null ? string.Empty : rateBox.Text, out rate) || rate < 0)
                    {
                        ShowMessage("Enter a valid package name, duration, and rate.", false);
                        return;
                    }

                    const string updateQuery = @"
                        UPDATE Packages
                        SET PackageName = @PackageName,
                            Description = @Description,
                            Duration = @Duration,
                            Rate = @Rate
                        WHERE PackageID = @PackageID AND CompanionID = @CompanionID";
                    ExecutePackageCommand(connection, updateQuery, packageId, companionId, nameBox.Text.Trim(),
                        descriptionBox == null || string.IsNullOrWhiteSpace(descriptionBox.Text) ? null : descriptionBox.Text.Trim(),
                        FormatDuration(duration, durationUnitBox == null ? "Hour" : durationUnitBox.SelectedValue), rate);
                    ShowMessage("Package updated successfully.", true);
                }
            }

            LoadPackages();
        }

        private void ExecutePackageCommand(SqlConnection connection, string query, int packageId, int companionId, string packageName, string description, string duration = null, decimal? rate = null)
        {
            // This helper keeps update/delete parameter binding consistent.
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@PackageID", packageId);
                command.Parameters.AddWithValue("@CompanionID", companionId);
                if (packageName != null) command.Parameters.AddWithValue("@PackageName", packageName);
                if (description != null) command.Parameters.AddWithValue("@Description", description);
                else if (query.Contains("@Description")) command.Parameters.AddWithValue("@Description", DBNull.Value);
                if (duration != null) command.Parameters.AddWithValue("@Duration", duration);
                if (rate.HasValue) command.Parameters.AddWithValue("@Rate", rate.Value);
                command.ExecuteNonQuery();
            }
        }

        // Duration is stored as text like "2 Hours" or "1 Day". These helpers split it back into the number and the unit.
        // Reads the number part: "2 Hours" -> 2.
        private bool TryParseDuration(string value, out int duration)
        {
            string[] parts = (value ?? string.Empty).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string firstPart = parts.Length == 0 ? string.Empty : parts[0];
            return int.TryParse(firstPart, out duration);
        }

        // Reads the unit part: "2 Days" -> "Day" (anything else, including missing, -> "Hour").
        private string GetDurationUnit(string value)
        {
            string[] parts = (value ?? string.Empty).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return "Hour";

            string unit = parts[1].ToLowerInvariant();
            return unit.StartsWith("day") ? "Day" : "Hour";
        }

        // Used by the .aspx to fill the duration box when editing a package.
        protected string GetDurationNumber(object value)
        {
            int duration;
            return TryParseDuration(value == null ? string.Empty : value.ToString(), out duration)
                ? duration.ToString()
                : string.Empty;
        }

        // As each row is built, select the matching unit (Hour/Day) in its edit dropdown.
        protected void rptPackages_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            DropDownList durationUnitBox = e.Item.FindControl("ddlEditDurationUnit") as DropDownList;
            if (durationUnitBox == null) return;

            DataRowView package = e.Item.DataItem as DataRowView;
            string duration = package == null || package["Duration"] == DBNull.Value
                ? string.Empty
                : package["Duration"].ToString();
            durationUnitBox.SelectedValue = GetDurationUnit(duration);
        }

        // Builds the stored text, e.g. (1, "Hour") -> "1 Hour", (3, "Day") -> "3 Days".
        private string FormatDuration(int duration, string unit)
        {
            string normalizedUnit = string.Equals(unit, "Day", StringComparison.OrdinalIgnoreCase) ? "Day" : "Hour";
            return duration + " " + normalizedUnit + (duration == 1 ? string.Empty : "s");
        }

        private void ClearPackageForm()
        {
            txtPackageName.Text = string.Empty;
            txtDuration.Text = string.Empty;
            txtRate.Text = string.Empty;
            txtDescription.Text = string.Empty;
        }

        private void ShowMessage(string message, bool success)
        {
            // Encode feedback before rendering it in the page.
            pnlMessage.CssClass = success ? "package-message success" : "package-message error";
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
