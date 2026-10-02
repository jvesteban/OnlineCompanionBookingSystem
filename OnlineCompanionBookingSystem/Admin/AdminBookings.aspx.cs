using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace OnlineCompanionBookingSystem.Admin
{
    // Admin > Bookings: nagpapakita ng lahat ng booking sa system, may stats, chart ng top packages,
    // filter ayon sa status, search, at option na i-cancel ang booking.
    public partial class Bookings : System.Web.UI.Page
    {
        // Connection string mula sa Web.config
        private string ConnStr => ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;

        // Naka-save sa ViewState ang napiling tab (All/Pending/...) para hindi mawala pagkatapos ng postback
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
                LoadBookingStats();
                LoadTopPackagesChart();
                LoadBookings(CurrentFilter);
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

        // Mga bilang sa stat cards. Ang "Confirmed" ay kasama ang Completed; ang "Cancelled" ay kasama ang Declined.
        private void LoadBookingStats()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Bookings", conn))
                        litTotalBookings.Text = cmd.ExecuteScalar().ToString();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Bookings WHERE Status IN ('Confirmed', 'Completed')", conn))
                        litConfirmedBookings.Text = cmd.ExecuteScalar().ToString();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Bookings WHERE Status = 'Pending'", conn))
                        litPendingBookings.Text = cmd.ExecuteScalar().ToString();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Bookings WHERE Status IN ('Cancelled', 'Declined')", conn))
                        litCancelledBookings.Text = cmd.ExecuteScalar().ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadBookingStats error: " + ex.Message);
            }
        }

        // Top 5 na pinakamadalas na na-book na package para sa chart.
        // Inilalagay ang mga pangalan at bilang sa hidden fields (comma-separated) na binabasa ng JavaScript ng chart.
        private void LoadTopPackagesChart()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    string query = @"
                        SELECT TOP 5 p.PackageName, COUNT(b.BookingID) AS TotalCount
                        FROM Bookings b
                        INNER JOIN Packages p ON b.PackageID = p.PackageID
                        GROUP BY p.PackageName
                        ORDER BY TotalCount DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<string> packages = new List<string>();
                            List<string> counts = new List<string>();

                            while (reader.Read())
                            {
                                packages.Add(reader["PackageName"].ToString());
                                counts.Add(reader["TotalCount"].ToString());
                            }

                            hfTopPackageNames.Value = string.Join(",", packages);
                            hfTopPackageCounts.Value = string.Join(",", counts);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadTopPackagesChart error: " + ex.Message);
            }
        }

        // Binubuo ang listahan ng booking. Dinadagdagan ang query ayon sa napiling filter at search text.
        // Parameterized ang @Status at @Search para hindi ma-SQL-inject.
        private void LoadBookings(string filterStatus, string searchQuery = "")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    string query = @"
                        SELECT 
                            b.BookingID,
                            cust.FullName AS CustomerName,
                            compUser.FullName AS CompanionName,
                            p.PackageName,
                            p.Rate,
                            b.BookingDate,
                            b.BookingTime,
                            b.Status,
                            b.DateCreated
                        FROM Bookings b
                        INNER JOIN Users cust ON b.CustomerID = cust.UserID
                        INNER JOIN CompanionProfiles cp ON b.CompanionID = cp.CompanionID
                        INNER JOIN Users compUser ON cp.UserID = compUser.UserID
                        INNER JOIN Packages p ON b.PackageID = p.PackageID
                        WHERE 1=1";

                    if (filterStatus != "All")
                    {
                        // Ang "Cancelled" tab ay kasama rin ang mga Declined na booking
                        if (filterStatus == "Cancelled")
                        {
                            query += " AND b.Status IN ('Cancelled', 'Declined')";
                        }
                        else
                        {
                            query += " AND b.Status = @Status";
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(searchQuery))
                    {
                        query += " AND (cust.FullName LIKE @Search OR compUser.FullName LIKE @Search OR p.PackageName LIKE @Search)";
                    }

                    query += " ORDER BY b.DateCreated DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (filterStatus != "All" && filterStatus != "Cancelled")
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

                            rptBookings.DataSource = dt;
                            rptBookings.DataBind();

                            lblEmpty.Visible = (dt.Rows.Count == 0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblEmpty.Visible = true;
                System.Diagnostics.Debug.WriteLine("LoadBookings error: " + ex.Message);
            }
        }

        // CSS class ng status pill (kulay) ayon sa status ng booking
        protected string GetStatusPillClass(string status)
        {
            switch (status)
            {
                case "Confirmed":
                case "Completed":
                    return "confirmed";
                case "Pending":
                    return "pending";
                case "Cancelled":
                case "Declined":
                    return "rejected";
                default:
                    return "pending";
            }
        }

        // Mga filter tab: itinatala ang napili, ina-highlight ang tab, at nire-reload ang listahan (kasama ang search text)
        protected void tabAll_Click(object sender, EventArgs e)
        {
            CurrentFilter = "All";
            SetActiveTab(tabAll);
            LoadBookings(CurrentFilter, txtSearch.Text);
        }

        protected void tabPending_Click(object sender, EventArgs e)
        {
            CurrentFilter = "Pending";
            SetActiveTab(tabPending);
            LoadBookings(CurrentFilter, txtSearch.Text);
        }

        protected void tabConfirmed_Click(object sender, EventArgs e)
        {
            CurrentFilter = "Confirmed";
            SetActiveTab(tabConfirmed);
            LoadBookings(CurrentFilter, txtSearch.Text);
        }

        protected void tabCompleted_Click(object sender, EventArgs e)
        {
            CurrentFilter = "Completed";
            SetActiveTab(tabCompleted);
            LoadBookings(CurrentFilter, txtSearch.Text);
        }

        protected void tabCancelled_Click(object sender, EventArgs e)
        {
            CurrentFilter = "Cancelled";
            SetActiveTab(tabCancelled);
            LoadBookings(CurrentFilter, txtSearch.Text);
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadBookings(CurrentFilter, txtSearch.Text);
        }

        // Ang napiling tab lang ang may "active" na class
        private void SetActiveTab(LinkButton activeTab)
        {
            tabAll.CssClass = "filter-tab";
            tabPending.CssClass = "filter-tab";
            tabConfirmed.CssClass = "filter-tab";
            tabCompleted.CssClass = "filter-tab";
            tabCancelled.CssClass = "filter-tab";
            activeTab.CssClass = "filter-tab active";
        }

        // Tinatawag kapag may pinindot na button sa isang booking row (CommandArgument = BookingID)
        protected void rptBookings_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "CancelBooking")
            {
                int bookingId = Convert.ToInt32(e.CommandArgument);

                try
                {
                    using (SqlConnection conn = new SqlConnection(ConnStr))
                    {
                        conn.Open();
                        // One transaction: the cancellation and both notifications are saved together or not at all
                        using (SqlTransaction tx = conn.BeginTransaction())
                        {
                            // 1. Pending o Confirmed lang ang puwedeng i-cancel; hindi na ginagalaw ang Completed/Declined
                            const string query = @"UPDATE Bookings
                                         SET Status = 'Cancelled'
                                         WHERE BookingID = @BookingID
                                           AND Status IN ('Pending', 'Confirmed')";
                            using (SqlCommand cmd = new SqlCommand(query, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@BookingID", bookingId);
                                if (cmd.ExecuteNonQuery() != 1)
                                {
                                    tx.Rollback();
                                    ShowSweetAlert("Booking Not Changed", "Only pending or confirmed bookings can be cancelled.", "warning");
                                    return;
                                }
                            }

                            // 2. Abisuhan ang customer at ang companion na ang admin ang nag-cancel.
                            //    RelatedBookingID ang nag-uugnay ng notification sa booking (para sa "Booking Details" ng customer).
                            const string notifyQuery = @"
                                DECLARE @When VARCHAR(60), @Pkg VARCHAR(200), @CustomerID INT, @CompanionUserID INT, @CustomerName VARCHAR(200);
                                SELECT @When = CONVERT(VARCHAR(12), b.BookingDate, 107) + ' at ' + CONVERT(VARCHAR(5), b.BookingTime, 108),
                                       @Pkg = p.PackageName, @CustomerID = b.CustomerID, @CompanionUserID = cp.UserID, @CustomerName = cu.FullName
                                FROM Bookings b
                                INNER JOIN Packages p ON p.PackageID = b.PackageID
                                INNER JOIN CompanionProfiles cp ON cp.CompanionID = b.CompanionID
                                INNER JOIN Users cu ON cu.UserID = b.CustomerID
                                WHERE b.BookingID = @BookingID;

                                INSERT INTO Notifications (UserID, Message, IsRead, DateCreated, RelatedBookingID)
                                VALUES (@CustomerID, 'Your booking for ' + @Pkg + ' on ' + @When + ' was cancelled by the administrator.', 0, GETDATE(), @BookingID),
                                       (@CompanionUserID, 'The administrator cancelled the booking from ' + @CustomerName + ' for ' + @Pkg + ' on ' + @When + '.', 0, GETDATE(), @BookingID);";
                            using (SqlCommand notify = new SqlCommand(notifyQuery, conn, tx))
                            {
                                notify.Parameters.AddWithValue("@BookingID", bookingId);
                                notify.ExecuteNonQuery();
                            }

                            tx.Commit();
                        }
                    }

                    ShowSweetAlert("Booking Cancelled", $"Booking #{bookingId} has been cancelled.", "success");

                    LoadBookingStats();
                    LoadTopPackagesChart();
                    LoadBookings(CurrentFilter, txtSearch.Text);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Cancel booking error: " + ex);
                    ShowSweetAlert("Error", "Could not cancel the booking. Please try again.", "error");
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