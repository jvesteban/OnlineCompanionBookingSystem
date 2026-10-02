using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

namespace OnlineCompanionBookingSystem.Admin
{
    // Admin > Companions: listahan ng lahat ng companion (anumang verification status), may stats,
    // chart ng top-rated, filter, search, at option na i-enable o i-disable ang account.
    public partial class AdminCompanions : System.Web.UI.Page
    {
        // Connection string mula sa Web.config
        private string ConnStr => ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;

        // Naka-save sa ViewState ang napiling tab para hindi mawala pagkatapos ng postback
        private string CurrentFilter
        {
            get => ViewState["CurrentFilter"]?.ToString() ?? "All";
            set => ViewState["CurrentFilter"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Admin lang ang puwedeng pumasok dito; ang iba ay ibabalik sa Login
            if (Session["UserID"] == null || Session["Role"]?.ToString() != "Admin")
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadAdminProfile();
                LoadCompanionStats();
                LoadTopCompanionsChart();
                LoadCompanions(CurrentFilter);
            }
        }

        // Kinukuha ang detalye ng naka-login na admin para sa profile dropdown/modal sa taas ng page
        private void LoadAdminProfile()
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    string query = "SELECT FullName, Email, ContactNumber, DateCreated FROM Users WHERE UserID = @UserID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                litAdminName.Text = Server.HtmlEncode(reader["FullName"].ToString());
                                litAdminEmail.Text = Server.HtmlEncode(reader["Email"].ToString());
                                litAdminContact.Text = Server.HtmlEncode(reader["ContactNumber"] != DBNull.Value ? reader["ContactNumber"].ToString() : "N/A");
                                litAdminDateCreated.Text = Convert.ToDateTime(reader["DateCreated"]).ToString("MMMM dd, yyyy");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                litAdminName.Text = "Administrator";
                System.Diagnostics.Debug.WriteLine("LoadAdminProfile error: " + ex.Message);
            }
        }

        // Mga bilang sa stat cards: lahat ng companion, Verified, at Pending
        private void LoadCompanionStats()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Role = 'Companion'", conn))
                        litTotalCompanions.Text = cmd.ExecuteScalar().ToString();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM CompanionProfiles WHERE VerificationStatus = 'Verified'", conn))
                        litVerifiedCompanions.Text = cmd.ExecuteScalar().ToString();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM CompanionProfiles WHERE VerificationStatus = 'Pending'", conn))
                        litPendingCompanions.Text = cmd.ExecuteScalar().ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadCompanionStats error: " + ex.Message);
            }
        }

        // Top 5 companion na may pinakamataas na average rating para sa chart.
        // Inilalagay ang mga pangalan at rating sa hidden fields (comma-separated) na binabasa ng JavaScript ng chart.
        // LEFT JOIN + ISNULL para kasama pati ang companion na wala pang rating (0).
        private void LoadTopCompanionsChart()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    string query = @"
                        SELECT TOP 5 
                            u.FullName, 
                            ISNULL(AVG(CAST(r.Score AS FLOAT)), 0) AS AvgRating
                        FROM CompanionProfiles cp
                        INNER JOIN Users u ON cp.UserID = u.UserID
                        LEFT JOIN Ratings r ON cp.CompanionID = r.CompanionID
                        WHERE u.Role = 'Companion'
                        GROUP BY u.FullName
                        ORDER BY AvgRating DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<string> names = new List<string>();
                            List<string> ratings = new List<string>();

                            while (reader.Read())
                            {
                                names.Add(reader["FullName"].ToString());
                                double rating = Convert.ToDouble(reader["AvgRating"]);
                                ratings.Add(rating.ToString("F1"));
                            }

                            hfTopCompanionNames.Value = string.Join(",", names);
                            hfTopCompanionRatings.Value = string.Join(",", ratings);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadTopCompanionsChart error: " + ex.Message);
            }
        }

        // Binubuo ang listahan ng companions. Ang "Activities" ay pinagsasama-sama sa isang text (FOR XML PATH trick)
        // hal. "Walking, Jogging". Parameterized ang @Status at @Search para hindi ma-SQL-inject.
        private void LoadCompanions(string filterStatus, string searchQuery = "")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    string query = @"
                        SELECT 
                            cp.CompanionID,
                            u.UserID,
                            u.FullName,
                            u.Email,
                            u.ContactNumber,
                            u.DateCreated,
                            ISNULL(u.IsActive, 1) AS IsActive,
                            u.ProfilePicture,
                            cp.VerificationStatus,
                            cp.Bio,
                            cp.VerificationDocPath,
                            (SELECT STUFF((SELECT ', ' + a.ActivityName 
                                           FROM Activities a 
                                           WHERE a.CompanionID = cp.CompanionID 
                                           FOR XML PATH('')), 1, 2, '')) AS Activities,
                            (SELECT COUNT(*) FROM Bookings b WHERE b.CompanionID = cp.CompanionID) AS TotalBookings,
                            ISNULL((SELECT AVG(CAST(r.Score AS FLOAT)) FROM Ratings r WHERE r.CompanionID = cp.CompanionID), 0) AS AvgRating
                        FROM CompanionProfiles cp
                        INNER JOIN Users u ON cp.UserID = u.UserID
                        WHERE u.Role = 'Companion'";

                    if (filterStatus != "All")
                    {
                        query += " AND cp.VerificationStatus = @Status";
                    }

                    if (!string.IsNullOrWhiteSpace(searchQuery))
                    {
                        query += " AND (u.FullName LIKE @Search OR u.Email LIKE @Search)";
                    }

                    query += " ORDER BY u.DateCreated DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (filterStatus != "All")
                        {
                            cmd.Parameters.AddWithValue("@Status", filterStatus);
                        }

                        if (!string.IsNullOrWhiteSpace(searchQuery))
                        {
                            cmd.Parameters.AddWithValue("@Search", "%" + searchQuery.Trim() + "%");
                        }

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            // Lagyan ng default na text ang companion na wala pang activity
                            foreach (DataRow row in dt.Rows)
                            {
                                if (row["Activities"] == DBNull.Value || string.IsNullOrEmpty(row["Activities"].ToString()))
                                {
                                    row["Activities"] = "None specified";
                                }
                            }

                            rptCompanions.DataSource = dt;
                            rptCompanions.DataBind();

                            lblEmpty.Visible = (dt.Rows.Count == 0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblEmpty.Visible = true;
                System.Diagnostics.Debug.WriteLine("LoadCompanions error: " + ex.Message);
            }
        }

        // Initials para sa avatar (hal. "Juan Dela Cruz" -> "JC"); "CP" kung walang pangalan
        protected string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "CP";
            string[] parts = fullName.Trim().Split(' ');
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[parts.Length - 1][0]}".ToUpper();
            return fullName.Substring(0, Math.Min(2, fullName.Length)).ToUpper();
        }

        // HTML ng avatar: profile picture kung meron, kung wala ay bilog na may initials. Naka-encode ang mga value laban sa HTML injection.
        protected string GetPhotoOrAvatar(object profilePhotoObj, object fullNameObj)
        {
            string profilePhoto = profilePhotoObj == DBNull.Value ? string.Empty : Convert.ToString(profilePhotoObj);
            string fullName = fullNameObj == DBNull.Value ? "Companion" : Convert.ToString(fullNameObj).Trim();

            if (!string.IsNullOrEmpty(profilePhoto))
            {
                return $"<img class='companion-avatar' src='{HttpUtility.HtmlAttributeEncode(ResolveUrl(profilePhoto))}' alt='{HttpUtility.HtmlAttributeEncode(fullName)}' />";
            }

            string initials = GetInitials(fullName);
            return $"<div class='companion-avatar'>{HttpUtility.HtmlEncode(initials)}</div>";
        }

        // Rating na may isang decimal (hal. 4.5); "0.0" kung wala pang rating
        protected string FormatRating(object rating)
        {
            if (rating == DBNull.Value || rating == null) return "0.0";
            double score = Convert.ToDouble(rating);
            return score.ToString("F1");
        }

        // CSS class ng status pill (kulay) ayon sa verification status
        protected string GetStatusPillClass(string status)
        {
            switch (status)
            {
                case "Verified": return "confirmed";
                case "Pending": return "pending";
                case "Rejected": return "rejected";
                default: return "pending";
            }
        }

        // Mga filter tab: itinatala ang napili, ina-highlight ang tab, at nire-reload ang listahan (kasama ang search text)
        protected void tabAll_Click(object sender, EventArgs e)
        {
            CurrentFilter = "All";
            SetActiveTab(tabAll);
            LoadCompanions(CurrentFilter, txtSearch.Text);
        }

        protected void tabVerified_Click(object sender, EventArgs e)
        {
            CurrentFilter = "Verified";
            SetActiveTab(tabVerified);
            LoadCompanions(CurrentFilter, txtSearch.Text);
        }

        protected void tabPending_Click(object sender, EventArgs e)
        {
            CurrentFilter = "Pending";
            SetActiveTab(tabPending);
            LoadCompanions(CurrentFilter, txtSearch.Text);
        }

        protected void tabRejected_Click(object sender, EventArgs e)
        {
            CurrentFilter = "Rejected";
            SetActiveTab(tabRejected);
            LoadCompanions(CurrentFilter, txtSearch.Text);
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadCompanions(CurrentFilter, txtSearch.Text);
        }

        // Ang napiling tab lang ang may "active" na class
        private void SetActiveTab(LinkButton activeTab)
        {
            tabAll.CssClass = "filter-tab";
            tabVerified.CssClass = "filter-tab";
            tabPending.CssClass = "filter-tab";
            tabRejected.CssClass = "filter-tab";
            activeTab.CssClass = "filter-tab active";
        }

        // Tinatawag kapag pinindot ang Enable/Disable. Ang CommandArgument ay "UserID|IsActive" (hal. "5|True");
        // binabaligtad ang kasalukuyang status. Hindi ito ang Verified/Rejected (iyon ay sa Verification page).
        protected void rptCompanions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "ToggleStatus")
            {
                string[] args = e.CommandArgument.ToString().Split('|');
                int userId = Convert.ToInt32(args[0]);
                bool currentIsActive = Convert.ToBoolean(args[1]);
                bool newStatus = !currentIsActive;

                try
                {
                    using (SqlConnection conn = new SqlConnection(ConnStr))
                    {
                        string query = "UPDATE Users SET IsActive = @IsActive WHERE UserID = @UserID";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@IsActive", newStatus ? 1 : 0);
                            cmd.Parameters.AddWithValue("@UserID", userId);
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }

                    string actionText = newStatus ? "enabled" : "disabled";
                    ShowSweetAlert("Status Updated", $"Companion account has been {actionText}.", "success");

                    LoadCompanionStats();
                    LoadTopCompanionsChart();
                    LoadCompanions(CurrentFilter, txtSearch.Text);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Update companion status error: " + ex);
                    ShowSweetAlert("Error", "Could not update the account status. Please try again.", "error");
                }
            }
        }

        // Nagpapakita ng SweetAlert popup (galing sa CDN sa .aspx) sa pamamagitan ng startup script
        private void ShowSweetAlert(string title, string message, string icon)
        {
            string script = $"Swal.fire({{ title: '{title}', text: '{message}', icon: '{icon}', confirmButtonColor: '#007bff' }});";
            ClientScript.RegisterStartupScript(this.GetType(), "SweetAlertNotif", script, true);
        }

        // Binubura ang Session at bumabalik sa landing page
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("~/Default.aspx");
        }
    }
}