using System;
using System.Configuration;
using System.Data.SqlClient;

namespace OnlineCompanionBookingSystem.Account
{
    // Login page para sa lahat ng role (Customer, Companion, Admin).
    // Flow: hanapin ang user gamit ang email -> i-check ang IsActive -> i-verify ang password ->
    // para sa Companion, i-check na Verified na ng admin -> mag-set ng Session -> i-redirect ayon sa role.
    public partial class Login : System.Web.UI.Page
    {
        // Pinalitan natin para direktang bumabase sa OCBSConnectionString mula sa web.config
        private string connStr => ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // Kunin muna ang record base sa Email lang para makuha ang stored hash
                    string query = "SELECT UserID, FullName, PasswordHash, Role, IsActive FROM Users WHERE Email = @Email";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bool isActive = (bool)reader["IsActive"];

                                if (!isActive)
                                {
                                    lblMessage.Text = "Your account has been deactivated. Please contact support.";
                                    lblMessage.ForeColor = System.Drawing.Color.Red;
                                    return;
                                }

                                string storedHash = reader["PasswordHash"].ToString();

                                // I-hash ang ininput na password at i-compare sa naka-save na hash (VerifyPassword)
                                bool isPasswordValid = PasswordHelper.VerifyPassword(password, storedHash);

                                if (isPasswordValid)
                                {
                                    string userId = reader["UserID"].ToString();
                                    string fullName = reader["FullName"].ToString();
                                    string role = reader["Role"].ToString();

                                    reader.Close();

                                    // Ang Companion ay makaka-login lang kapag Verified na ng admin
                                    if (role.Trim() == "Companion")
                                    {
                                        string verificationStatus;
                                        using (SqlCommand statusCmd = new SqlCommand(
                                            "SELECT VerificationStatus FROM CompanionProfiles WHERE UserID = @UserID", conn))
                                        {
                                            statusCmd.Parameters.AddWithValue("@UserID", userId);
                                            verificationStatus = statusCmd.ExecuteScalar()?.ToString();
                                        }

                                        if (verificationStatus != "Verified")
                                        {
                                            lblMessage.Text = verificationStatus == "Rejected"
                                                ? "Your companion application was not approved. Please contact support for assistance."
                                                : "Your companion account is still pending admin verification. You will receive an email once it has been approved.";
                                            lblMessage.ForeColor = System.Drawing.Color.Red;
                                            return;
                                        }
                                    }

                                    // I-set ang Session; ito ang ginagamit ng ibang page para malaman kung sino ang naka-login at anong role niya
                                    Session["UserID"] = userId;
                                    Session["FullName"] = fullName;
                                    Session["Role"] = role;

                                    // Kung may ReturnUrl, doon ibalik ang user. Local path lang ang tinatanggap
                                    // (nagsisimula sa "/" pero hindi "//") para hindi magamit sa open-redirect papunta sa ibang website.
                                    string returnUrl = Request.QueryString["ReturnUrl"];
                                    if (!string.IsNullOrWhiteSpace(returnUrl) &&
                                        returnUrl.StartsWith("/", StringComparison.Ordinal) &&
                                        !returnUrl.StartsWith("//", StringComparison.Ordinal))
                                    {
                                        Response.Redirect(returnUrl, false);
                                        Context.ApplicationInstance.CompleteRequest();
                                        return;
                                    }

                                    // Mag-redirect base sa role at tamang folder path
                                    switch (role.Trim())
                                    {
                                        case "Customer":
                                            Response.Redirect("~/Customer/Dashboard.aspx", false);
                                            break;
                                        case "Companion":
                                            Response.Redirect("~/Companion/CompanionDashboard.aspx", false);
                                            break;
                                        case "Admin":
                                            Response.Redirect("~/Admin/AdminDashboard.aspx", false);
                                            break;
                                        default:
                                            Response.Redirect("~/Default.aspx", false);
                                            break;
                                    }
                                    Context.ApplicationInstance.CompleteRequest();
                                }
                                else
                                {
                                    lblMessage.Text = "Invalid email or password.";
                                    lblMessage.ForeColor = System.Drawing.Color.Red;
                                }
                            }
                            else
                            {
                                // Pareho ang mensahe kapag walang ganoong email, para hindi malaman ng iba kung alin ang rehistrado
                                lblMessage.Text = "Invalid email or password.";
                                lblMessage.ForeColor = System.Drawing.Color.Red;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Login error: " + ex);
                lblMessage.Text = "We could not complete the login request. Please try again.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }
    }
}