using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace OnlineCompanionBookingSystem.Admin
{
    // Admin > Verification: dito nire-review ng admin ang mga companion application (ID document).
    // Approve -> puwede nang mag-login ang companion; Reject -> hindi; Revoke -> ibinabalik sa Pending.
    // Bawat aksyon ay nagpapadala ng in-app notification at email sa companion.
    public partial class Verification : System.Web.UI.Page
    {
        // Connection string mula sa Web.config
        string connString = ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;

        // ViewState para maalala kung anong tab ang kasalukuyang active
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
                LoadVerifications(CurrentFilter);

                // The admin has now seen the applications, so the sidebar's "new" counter goes away
                NavCounts.MarkSeen(NavCounts.Admin, Convert.ToInt32(Session["UserID"]), "verification");
            }
        }

        // Kinukuha ang detalye ng naka-login na admin para sa profile dropdown/modal sa taas ng page
        private void LoadAdminProfile()
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
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

        // Binubuo ang listahan ng mga application. Parameterized ang @Status at @Search para hindi ma-SQL-inject.
        // Pagkatapos kunin ang data, nagdaragdag ng tatlong dagdag na column na gagamitin ng .aspx para sa display:
        //   StatusLabel ("Date Verified" / "Date Applied"), DateFormatted, at StatusClass (kulay ng pill).
        private void LoadVerifications(string filterStatus, string searchQuery = "")
        {
            // The query below reads the Payments table, so make sure it exists first
            // (it is normally created by the first registration payment).
            try
            {
                using (SqlConnection schemaConn = new SqlConnection(connString))
                {
                    schemaConn.Open();
                    PaymentService.EnsureSchema(schemaConn);
                    CompanionGender.EnsureColumn(schemaConn);   // the query below reads CompanionProfiles.Gender
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EnsureSchema error: " + ex.Message);
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"SELECT c.CompanionID, u.FullName, u.Email, 
                                        u.ContactNumber AS ContactNo, 
                                        c.VerificationStatus, 
                                        u.DateCreated AS DateApplied, 
                                        c.DateVerified, 
                                        c.VerificationDocPath AS DocumentPath,
                                        c.Gender,
                                        u.ProfilePicture,
                                        pay.Amount AS PayAmount, pay.Method AS PayMethod, pay.Status AS PayStatus,
                                        (SELECT STUFF((SELECT ', ' + a.ActivityName
                                                       FROM Activities a
                                                       WHERE a.CompanionID = c.CompanionID
                                                       FOR XML PATH('')), 1, 2, '')) AS Activities
                                 FROM CompanionProfiles c
                                 JOIN Users u ON c.UserID = u.UserID
                                 OUTER APPLY (SELECT TOP 1 Amount, Method, Status FROM Payments p
                                              WHERE p.UserID = u.UserID ORDER BY p.PaymentID DESC) pay
                                 WHERE 1=1";

                if (filterStatus != "All")
                {
                    query += " AND c.VerificationStatus = @Status";
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

                        dt.Columns.Add("StatusLabel", typeof(string));
                        dt.Columns.Add("DateFormatted", typeof(string));
                        dt.Columns.Add("StatusClass", typeof(string));
                        dt.Columns.Add("PaymentInfo", typeof(string));   // e.g. "Registration fee: PHP 50.00 paid via GCash"
                        dt.Columns.Add("PaymentClass", typeof(string));  // paid / refunded / none (color of the line)

                        // Verified: ipakita ang petsa ng pag-verify; Rejected/Pending: ang petsa ng pag-apply
                        foreach (DataRow row in dt.Rows)
                        {
                            // Registration fee line: what the applicant paid, and whether it was refunded after a rejection
                            string payStatus = row["PayStatus"] == DBNull.Value ? "" : row["PayStatus"].ToString();
                            if (payStatus == "")
                            {
                                row["PaymentInfo"] = "Registration fee: no payment record";
                                row["PaymentClass"] = "none";
                            }
                            else
                            {
                                string fee = "PHP " + Convert.ToDecimal(row["PayAmount"]).ToString("N2") + " via " + row["PayMethod"];
                                row["PaymentInfo"] = payStatus == "Refunded" ? "Fee refunded: " + fee : "Fee paid: " + fee;
                                row["PaymentClass"] = payStatus == "Refunded" ? "refunded" : "paid";
                            }

                            string status = row["VerificationStatus"].ToString();
                            if (status == "Verified")
                            {
                                row["StatusLabel"] = "Date Verified";
                                row["DateFormatted"] = row["DateVerified"] != DBNull.Value ? Convert.ToDateTime(row["DateVerified"]).ToString("MMM. dd, yyyy") : "";
                                row["StatusClass"] = "confirmed";
                            }
                            else if (status == "Rejected")
                            {
                                row["StatusLabel"] = "Date Applied";
                                row["DateFormatted"] = row["DateApplied"] != DBNull.Value ? Convert.ToDateTime(row["DateApplied"]).ToString("MMM. dd, yyyy") : "";
                                row["StatusClass"] = "rejected";
                            }
                            else
                            {
                                row["StatusLabel"] = "Date Applied";
                                row["DateFormatted"] = row["DateApplied"] != DBNull.Value ? Convert.ToDateTime(row["DateApplied"]).ToString("MMM. dd, yyyy") : "";
                                row["StatusClass"] = "pending";
                            }

                            if (row["Activities"] == DBNull.Value || string.IsNullOrEmpty(row["Activities"].ToString()))
                            {
                                row["Activities"] = "None specified";
                            }
                        }

                        rptVerifications.DataSource = dt;
                        rptVerifications.DataBind();

                        lblEmpty.Visible = (dt.Rows.Count == 0);
                    }
                }
            }
        }

        // Ipinapakita ang profile picture ng companion; initials lang kung wala pa siyang na-upload
        protected string GetAvatarHtml(object fullNameObj, object profilePictureObj)
        {
            string fullName = fullNameObj == DBNull.Value ? "" : Convert.ToString(fullNameObj);
            string picture = profilePictureObj == null || profilePictureObj == DBNull.Value ? "" : Convert.ToString(profilePictureObj);

            if (!string.IsNullOrWhiteSpace(picture))
            {
                return $"<img style='width:100%;height:100%;object-fit:cover;border-radius:50%' src='{System.Web.HttpUtility.HtmlAttributeEncode(ResolveUrl(picture))}' alt='{System.Web.HttpUtility.HtmlAttributeEncode(fullName)}' />";
            }
            return System.Web.HttpUtility.HtmlEncode(GetInitials(fullName));
        }

        // Initials para sa avatar (hal. "Juan Dela Cruz" -> "JC"); "OC" kung walang pangalan
        protected string GetInitials(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return "OC";
            string[] parts = fullName.Trim().Split(' ');
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[parts.Length - 1][0]}".ToUpper();
            return fullName.Substring(0, Math.Min(2, fullName.Length)).ToUpper();
        }

        // Binubura ang Session at bumabalik sa landing page
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("~/Default.aspx");
        }

        // Mga filter tab: itinatala ang napili, ina-highlight ang tab, at nire-reload ang listahan (kasama ang search text)
        protected void tabAll_Click(object sender, EventArgs e)
        {
            CurrentFilter = "All";
            SetActiveTab(tabAll);
            LoadVerifications(CurrentFilter, txtSearch.Text);
        }

        protected void tabPending_Click(object sender, EventArgs e)
        {
            CurrentFilter = "Pending";
            SetActiveTab(tabPending);
            LoadVerifications(CurrentFilter, txtSearch.Text);
        }

        protected void tabVerified_Click(object sender, EventArgs e)
        {
            CurrentFilter = "Verified";
            SetActiveTab(tabVerified);
            LoadVerifications(CurrentFilter, txtSearch.Text);
        }

        protected void tabRejected_Click(object sender, EventArgs e)
        {
            CurrentFilter = "Rejected";
            SetActiveTab(tabRejected);
            LoadVerifications(CurrentFilter, txtSearch.Text);
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadVerifications(CurrentFilter, txtSearch.Text);
        }

        // Ang napiling tab lang ang may "active" na class
        private void SetActiveTab(LinkButton activeTab)
        {
            tabAll.CssClass = "filter-tab";
            tabPending.CssClass = "filter-tab";
            tabVerified.CssClass = "filter-tab";
            tabRejected.CssClass = "filter-tab";
            activeTab.CssClass = "filter-tab active";
        }

        // Tinatawag kapag pinindot ang Approve/Reject/Revoke sa isang application (CommandArgument = CompanionID).
        // Una, binabago ang status sa database; pagkatapos, ipinapadala ang notification at email.
        protected void rptVerifications_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int companionId = Convert.ToInt32(e.CommandArgument);
            string actionText = "";

            // The reason the admin gave in the dialog (shown to the companion). A rejection or revocation must have one.
            string reason = DecisionReason.Read(Request);
            if (DecisionReason.IsRequired(e.CommandName) && reason.Length == 0)
            {
                ShowSweetAlert("Reason Required", "Please choose a reason so the companion knows why.", "warning");
                LoadVerifications(CurrentFilter, txtSearch.Text);
                return;
            }


            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "";
                if (e.CommandName == "Approve")
                {
                    query = "UPDATE CompanionProfiles SET VerificationStatus = 'Verified', DateVerified = GETDATE() WHERE CompanionID = @CompanionID";
                    actionText = "approved";
                }
                else if (e.CommandName == "Reject")
                {
                    query = "UPDATE CompanionProfiles SET VerificationStatus = 'Rejected' WHERE CompanionID = @CompanionID";
                    actionText = "rejected";
                }
                else if (e.CommandName == "Revoke")
                {
                    query = "UPDATE CompanionProfiles SET VerificationStatus = 'Pending', DateVerified = NULL WHERE CompanionID = @CompanionID";
                    actionText = "revoked";
                }

                if (!string.IsNullOrEmpty(query))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CompanionID", companionId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }

                    SendNotification(companionId, e.CommandName, reason);

                    // Email sa companion para sa approve, reject, at revoke.
                    // Reject: ibinabalik muna ang registration fee (parehong paraan ng pagbabayad) at kasama sa email ang refund.
                    // Approve at Revoke: walang refund (ang fee ay para sa buong proseso ng application).
                    RefundResult refund = null;
                    if (e.CommandName == "Reject")
                    {
                        refund = PaymentService.RefundCompanionRegistration(companionId);
                        EmailHelper.SendCompanionVerificationResultById(companionId, false, refund, reason);
                    }
                    else if (e.CommandName == "Approve")
                    {
                        PaymentService.RestoreCompanionRegistrationPayment(companionId);
                        EmailHelper.SendCompanionVerificationResultById(companionId, true, null, reason);
                    }
                    else if (e.CommandName == "Revoke")
                    {
                        EmailHelper.SendCompanionVerificationRevokedById(companionId, reason);
                    }

                    string alertText = $"Companion verification has been {actionText} successfully.";
                    if (refund != null && refund.Refunded)
                    {
                        alertText += " The PHP " + refund.Amount.ToString("N2") + " registration fee was refunded to " +
                                     System.Web.HttpUtility.JavaScriptStringEncode(refund.Method) + " (ref. " + refund.RefundReference + ").";
                    }
                    ShowSweetAlert("Status Updated", alertText, "success");
                }
            }

            // I-reload ang kasalukuyang tab at search query para hindi mawala ang filter state
            LoadVerifications(CurrentFilter, txtSearch.Text);
        }

        // Nagpapakita ng SweetAlert popup (galing sa CDN sa .aspx) sa pamamagitan ng startup script
        private void ShowSweetAlert(string title, string message, string icon)
        {
            string script = $"Swal.fire({{ title: '{title}', text: '{message}', icon: '{icon}', confirmButtonColor: '#007bff' }});";
            ClientScript.RegisterStartupScript(this.GetType(), "SweetAlertNotif", script, true);
        }

        // Gumagawa ng in-app notification para sa companion (lalabas sa Companion > Notifications).
        // Ang Notifications table ay nakabatay sa UserID, kaya hinahanap muna ang UserID mula sa CompanionID.
        private void SendNotification(int companionId, string action, string reason)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string getUserIdQuery = "SELECT UserID FROM CompanionProfiles WHERE CompanionID = @CompanionID";
                int userId = 0;

                using (SqlCommand cmd = new SqlCommand(getUserIdQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanionID", companionId);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null) userId = Convert.ToInt32(result);
                }

                if (userId > 0)
                {
                    string message = "";
                    if (action == "Approve") message = "Your companion account has been verified by the administrator!";
                    else if (action == "Reject") message = "Your companion verification request was rejected.";
                    else if (action == "Revoke") message = "Your verification status has been reverted to pending.";

                    // Add the admin's reason, and keep the whole text inside the 255-character Message column
                    if (!string.IsNullOrWhiteSpace(reason)) message += " Reason: " + reason;
                    if (message.Length > 255) message = message.Substring(0, 252) + "...";

                    string insertNotif = "INSERT INTO Notifications (UserID, Message, DateCreated, IsRead) VALUES (@UserID, @Message, GETDATE(), 0)";
                    using (SqlCommand cmd = new SqlCommand(insertNotif, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@Message", message);
                        if (conn.State == ConnectionState.Closed) conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}