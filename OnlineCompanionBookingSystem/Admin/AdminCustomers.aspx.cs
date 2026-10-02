using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace OnlineCompanionBookingSystem.Admin
{
    // Admin > Customers: listahan ng lahat ng customer, may stats, chart ng top customers,
    // filter (All/Active/Inactive), search, at option na i-activate o i-deactivate ang account.
    public partial class AdminCustomers : System.Web.UI.Page
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
                LoadCustomerStats();
                LoadTopCustomersChart();
                LoadCustomers(CurrentFilter);
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

        // Bilang ng customers sa stat cards. Ang ISNULL(IsActive, 1) ay nagtuturing na Active ang account kung walang value.
        private void LoadCustomerStats()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Role = 'Customer'", conn))
                        litTotalCustomers.Text = cmd.ExecuteScalar().ToString();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Role = 'Customer' AND ISNULL(IsActive, 1) = 1", conn))
                        litActiveCustomers.Text = cmd.ExecuteScalar().ToString();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Role = 'Customer' AND ISNULL(IsActive, 1) = 0", conn))
                        litInactiveCustomers.Text = cmd.ExecuteScalar().ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadCustomerStats error: " + ex.Message);
            }
        }

        // Top 5 customer na pinakamaraming booking para sa chart.
        // Inilalagay ang mga pangalan at bilang sa hidden fields (comma-separated) na binabasa ng JavaScript ng chart.
        // LEFT JOIN para kasama pati ang customer na wala pang booking.
        private void LoadTopCustomersChart()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    string query = @"
                        SELECT TOP 5 u.FullName, COUNT(b.BookingID) AS TotalBookings
                        FROM Users u
                        LEFT JOIN Bookings b ON u.UserID = b.CustomerID
                        WHERE u.Role = 'Customer'
                        GROUP BY u.FullName
                        ORDER BY TotalBookings DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<string> names = new List<string>();
                            List<string> bookings = new List<string>();

                            while (reader.Read())
                            {
                                names.Add(reader["FullName"].ToString());
                                bookings.Add(reader["TotalBookings"].ToString());
                            }

                            hfTopCustomerNames.Value = string.Join(",", names);
                            hfTopCustomerBookings.Value = string.Join(",", bookings);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadTopCustomersChart error: " + ex.Message);
            }
        }

        // Binubuo ang listahan ng customers. Dinadagdagan ang query ayon sa filter at search text;
        // parameterized ang @Search para hindi ma-SQL-inject.
        private void LoadCustomers(string filterStatus, string searchQuery = "")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    string query = @"
                        SELECT 
                            u.UserID, 
                            u.FullName, 
                            u.Email, 
                            u.ContactNumber, 
                            u.ProfilePicture,
                            u.DateCreated, 
                            ISNULL(u.IsActive, 1) AS IsActive,
                            (SELECT COUNT(*) FROM Bookings b WHERE b.CustomerID = u.UserID) AS TotalBookings
                        FROM Users u
                        WHERE u.Role = 'Customer'";

                    if (filterStatus == "Active")
                    {
                        query += " AND ISNULL(u.IsActive, 1) = 1";
                    }
                    else if (filterStatus == "Inactive")
                    {
                        query += " AND ISNULL(u.IsActive, 1) = 0";
                    }

                    if (!string.IsNullOrWhiteSpace(searchQuery))
                    {
                        query += " AND (u.FullName LIKE @Search OR u.Email LIKE @Search)";
                    }

                    query += " ORDER BY u.DateCreated DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (!string.IsNullOrWhiteSpace(searchQuery))
                        {
                            cmd.Parameters.AddWithValue("@Search", "%" + searchQuery.Trim() + "%");
                        }

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            rptCustomers.DataSource = dt;
                            rptCustomers.DataBind();

                            lblEmpty.Visible = (dt.Rows.Count == 0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblEmpty.Visible = true;
                System.Diagnostics.Debug.WriteLine("LoadCustomers error: " + ex.Message);
            }
        }

        // Avatar HTML: ang profile photo ng customer kung meron (galing sa Customer > Profile), kung wala ay initials.
        // Naka-encode ang lahat ng value; ang photo path ay tinatanggap lang kung nasa ~/Uploads/ para hindi makapasok ang kakaibang URL.
        protected string GetCustomerAvatar(object profilePicture, object fullName)
        {
            string url = GetPhotoUrl(profilePicture);
            string name = fullName == null || fullName == DBNull.Value ? string.Empty : fullName.ToString();
            if (url.Length > 0)
                return "<img class=\"customer-avatar-img\" src=\"" + System.Web.HttpUtility.HtmlAttributeEncode(url) + "\" alt=\"" + System.Web.HttpUtility.HtmlAttributeEncode(name) + "\" />";
            return System.Web.HttpUtility.HtmlEncode(GetInitials(name));
        }

        // URL ng profile photo na magagamit ng browser, o "" kung wala / hindi valid ang path
        protected string GetPhotoUrl(object profilePicture)
        {
            string path = profilePicture == null || profilePicture == DBNull.Value ? string.Empty : profilePicture.ToString().Trim();
            if (!path.StartsWith("~/Uploads/", StringComparison.OrdinalIgnoreCase) || path.Contains("..")) return string.Empty;
            return ResolveUrl(path);
        }

        // Initials para sa avatar (hal. "Juan Dela Cruz" -> "JC"); "CU" kung walang pangalan
        protected string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "CU";
            string[] parts = fullName.Trim().Split(' ');
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[parts.Length - 1][0]}".ToUpper();
            return fullName.Substring(0, Math.Min(2, fullName.Length)).ToUpper();
        }

        // Mga filter tab: itinatala ang napili, ina-highlight ang tab, at nire-reload ang listahan (kasama ang search text)
        protected void tabAll_Click(object sender, EventArgs e)
        {
            CurrentFilter = "All";
            SetActiveTab(tabAll);
            LoadCustomers(CurrentFilter, txtSearch.Text);
        }

        protected void tabActive_Click(object sender, EventArgs e)
        {
            CurrentFilter = "Active";
            SetActiveTab(tabActive);
            LoadCustomers(CurrentFilter, txtSearch.Text);
        }

        protected void tabInactive_Click(object sender, EventArgs e)
        {
            CurrentFilter = "Inactive";
            SetActiveTab(tabInactive);
            LoadCustomers(CurrentFilter, txtSearch.Text);
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadCustomers(CurrentFilter, txtSearch.Text);
        }

        // Ang napiling tab lang ang may "active" na class
        private void SetActiveTab(LinkButton activeTab)
        {
            tabAll.CssClass = "filter-tab";
            tabActive.CssClass = "filter-tab";
            tabInactive.CssClass = "filter-tab";
            activeTab.CssClass = "filter-tab active";
        }

        // Tinatawag kapag pinindot ang Activate/Deactivate. Ang CommandArgument ay "UserID|IsActive" (hal. "5|True");
        // binabaligtad ang kasalukuyang status. Hindi binubura ang account; IsActive lang ang nagbabago.
        protected void rptCustomers_ItemCommand(object source, RepeaterCommandEventArgs e)
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

                    string actionText = newStatus ? "activated" : "deactivated";
                    ShowSweetAlert("Account Updated", $"Customer account has been successfully {actionText}.", "success");

                    LoadCustomerStats();
                    LoadTopCustomersChart();
                    LoadCustomers(CurrentFilter, txtSearch.Text);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Update customer status error: " + ex);
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