using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OnlineCompanionBookingSystem.Admin
{
    // Admin > Dashboard: buod ng system (mga bilang), 5 pinakabagong pending na verification
    // (puwedeng i-approve/i-reject agad), at 5 pinakabagong booking.
    public partial class Dashboard : System.Web.UI.Page
    {
        // Connection string mula sa Web.config
        private string ConnStr => ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;

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
                LoadStats();
                LoadPendingVerifications();
                LoadRecentBookings();
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

        // Mga bilang sa stat cards: customers, companions, pending verifications, at lahat ng bookings
        private void LoadStats()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Role = 'Customer'", conn))
                        litTotalCustomers.Text = cmd.ExecuteScalar().ToString();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Role = 'Companion'", conn))
                        litTotalCompanions.Text = cmd.ExecuteScalar().ToString();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM CompanionProfiles WHERE VerificationStatus = 'Pending'", conn))
                        litPendingVerifications.Text = cmd.ExecuteScalar().ToString();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Bookings", conn))
                        litTotalBookings.Text = cmd.ExecuteScalar().ToString();
                }
            }
            catch (Exception ex)
            {
                litTotalCustomers.Text = "0";
                litTotalCompanions.Text = "0";
                litPendingVerifications.Text = "0";
                litTotalBookings.Text = "0";
                System.Diagnostics.Debug.WriteLine("LoadStats error: " + ex.Message);
            }
        }

        // 5 pinakabagong companion application na Pending pa. Ang "Activities" ay pinagsasama sa isang text (FOR XML PATH trick).
        private void LoadPendingVerifications()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    string query = @"
                        SELECT TOP 5
                            cp.CompanionID,
                            u.FullName,
                            u.Email,
                            u.ContactNumber,
                            u.DateCreated,
                            cp.VerificationDocPath,
                            STUFF((
                                SELECT ', ' + a.ActivityName
                                FROM Activities a
                                WHERE a.CompanionID = cp.CompanionID
                                FOR XML PATH('')
                            ), 1, 2, '') AS Activities
                        FROM CompanionProfiles cp
                        INNER JOIN Users u ON cp.UserID = u.UserID
                        WHERE cp.VerificationStatus = 'Pending'
                        ORDER BY u.DateCreated DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            System.Data.DataTable dt = new System.Data.DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count == 0)
                            {
                                lblNoPending.Visible = true;
                            }
                            else
                            {
                                rptPendingVerifications.DataSource = dt;
                                rptPendingVerifications.DataBind();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblNoPending.Visible = true;
                System.Diagnostics.Debug.WriteLine("LoadPendingVerifications error: " + ex.Message);
            }
        }

        // 5 pinakabagong booking, kasama ang pangalan ng customer, companion, at package
        private void LoadRecentBookings()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    string query = @"
                        SELECT TOP 5
                            b.BookingID,
                            cust.FullName AS CustomerName,
                            comp.FullName AS CompanionName,
                            p.PackageName,
                            b.BookingDate,
                            b.Status
                        FROM Bookings b
                        INNER JOIN Users cust ON b.CustomerID = cust.UserID
                        INNER JOIN CompanionProfiles cp ON b.CompanionID = cp.CompanionID
                        INNER JOIN Users comp ON cp.UserID = comp.UserID
                        INNER JOIN Packages p ON b.PackageID = p.PackageID
                        ORDER BY b.DateCreated DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            System.Data.DataTable dt = new System.Data.DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count == 0)
                            {
                                lblNoBookings.Visible = true;
                            }
                            else
                            {
                                rptRecentBookings.DataSource = dt;
                                rptRecentBookings.DataBind();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblNoBookings.Visible = true;
                System.Diagnostics.Debug.WriteLine("LoadRecentBookings error: " + ex.Message);
            }
        }

        // CSS class ng status pill (kulay) ayon sa status ng booking
        protected string GetStatusClass(string status)
        {
            switch (status)
            {
                case "Confirmed":
                    return "confirmed";
                case "Pending":
                    return "pending";
                case "Declined":
                case "Cancelled":
                    return "rejected";
                case "Completed":
                    return "confirmed";
                default:
                    return "pending";
            }
        }

        // Approve/Reject button sa listahan ng pending (CommandArgument = CompanionID).
        // Pagkatapos, nire-redirect ang page para mag-refresh ang mga bilang at listahan.
        protected void rptPendingVerifications_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int companionId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "Approve")
            {
                UpdateVerificationStatus(companionId, "Verified");
            }
            else if (e.CommandName == "Reject")
            {
                UpdateVerificationStatus(companionId, "Rejected");
            }

            Response.Redirect("~/Admin/AdminDashboard.aspx");
        }

        // Approve/Reject mula sa detail modal; ang napiling CompanionID ay inilalagay ng JavaScript sa hidden field (hfSelectedCompanionId)
        protected void btnModalApprove_Click(object sender, EventArgs e)
        {
            if (int.TryParse(hfSelectedCompanionId.Value, out int companionId))
            {
                UpdateVerificationStatus(companionId, "Verified");
            }
            Response.Redirect("~/Admin/AdminDashboard.aspx");
        }

        protected void btnModalReject_Click(object sender, EventArgs e)
        {
            if (int.TryParse(hfSelectedCompanionId.Value, out int companionId))
            {
                UpdateVerificationStatus(companionId, "Rejected");
            }
            Response.Redirect("~/Admin/AdminDashboard.aspx");
        }

        // Binabago ang VerificationStatus ("Verified" o "Rejected"). Kapag may na-update na row, nagpapadala ng email sa companion.
        // DateVerified ay nalalagyan lang kapag Verified. (Hindi nagpapadala ng in-app notification dito, di tulad ng Verification page.)
        private void UpdateVerificationStatus(int companionId, string status)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    string query = @"UPDATE CompanionProfiles 
                                     SET VerificationStatus = @Status, 
                                         DateVerified = CASE WHEN @Status = 'Verified' THEN GETDATE() ELSE DateVerified END
                                     WHERE CompanionID = @CompanionID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@CompanionID", companionId);
                        conn.Open();
                        int affected = cmd.ExecuteNonQuery();

                        // Abisuhan ang companion sa email tungkol sa desisyon ng admin.
                        // Kapag Rejected, ibinabalik muna ang registration fee at kasama sa email ang detalye ng refund.
                        if (affected > 0)
                        {
                            RefundResult refund = status == "Rejected" ? PaymentService.RefundCompanionRegistration(companionId) : null;
                            if (status == "Verified")
                            {
                                PaymentService.RestoreCompanionRegistrationPayment(companionId);
                            }
                            EmailHelper.SendCompanionVerificationResultById(companionId, status == "Verified", refund);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateVerificationStatus error: " + ex.Message);
            }
        }

        // Binubura ang Session at bumabalik sa landing page
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("~/Default.aspx");
        }
    }
}