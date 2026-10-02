using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OnlineCompanionBookingManagementSystem.Customer
{
    // Customer > Rate Companion: lets a customer give a 1-5 star rating and an optional comment for a
    // Completed booking. One rating per booking. The companion is notified when a rating is submitted.
    public partial class RateCompanion : Page
    {
        // Connection string from Web.config
        private string ConnectionString => ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;
        // The booking ID is passed by CustomerMyBookings.aspx through the query string.
        private int BookingId => int.TryParse(Request.QueryString["BookingID"], out int id) ? id : 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Not logged in as a customer: go to Login and come back to this exact page afterwards (ReturnUrl).
            string role = Session["Role"] == null ? string.Empty : Session["Role"].ToString().Trim();
            if (Session["UserID"] == null || !string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                string returnUrl = Request.RawUrl;
                Response.Redirect("~/Account/Login.aspx?ReturnUrl=" + Server.UrlEncode(returnUrl), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                int customerId = Convert.ToInt32(Session["UserID"]);
                LoadUserData(customerId);
                LoadBooking(customerId);
            }
        }

        // Fills the top bar and profile dropdown (name, email, picture or initials).
        private void LoadUserData(int userId)
        {
            const string query = "SELECT FullName, Email, ProfilePicture FROM Users WHERE UserID = @UserID";
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserID", userId);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return;

                    string fullName = reader["FullName"] == DBNull.Value ? "User" : reader["FullName"].ToString().Trim();
                    string email = reader["Email"] == DBNull.Value ? "customer@ocbs.com" : reader["Email"].ToString().Trim();
                    string profilePicture = reader["ProfilePicture"] == DBNull.Value ? string.Empty : reader["ProfilePicture"].ToString().Trim();
                    string[] nameParts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string initials = nameParts.Length > 1
                        ? (nameParts[0][0].ToString() + nameParts[nameParts.Length - 1][0]).ToUpperInvariant()
                        : (nameParts.Length == 1 ? nameParts[0].Substring(0, Math.Min(2, nameParts[0].Length)).ToUpperInvariant() : "U");

                    litUserFirstName.Text = Server.HtmlEncode(nameParts.Length > 0 ? nameParts[0] : "User");
                    litDropdownName.Text = Server.HtmlEncode(fullName);
                    litDropdownEmail.Text = Server.HtmlEncode(email);
                    litAvatarInitial.Text = initials;
                    litAvatarInitialLg.Text = initials;

                    if (!string.IsNullOrWhiteSpace(profilePicture))
                    {
                        imgTopAvatar.ImageUrl = profilePicture;
                        imgDropAvatar.ImageUrl = profilePicture;
                        imgTopAvatar.Visible = true;
                        imgDropAvatar.Visible = true;
                        litAvatarInitial.Visible = false;
                        litAvatarInitialLg.Visible = false;
                    }
                }
            }
        }

        // Shows the booking being rated. The form is hidden unless the booking belongs to this customer,
        // is Completed, and has not been rated yet.
        private void LoadBooking(int customerId)
        {
            if (BookingId <= 0)
            {
                ShowMessage("The booking reference is invalid.", false);
                pnlRatingForm.Visible = false;
                return;
            }

            const string query = @"
                SELECT TOP 1
                    b.BookingID,
                    uComp.FullName AS CompanionName,
                    p.PackageName,
                    b.BookingDate,
                    b.Status,
                    CASE WHEN r.BookingID IS NULL THEN 0 ELSE 1 END AS HasRating
                FROM Bookings b
                INNER JOIN CompanionProfiles cp ON b.CompanionID = cp.CompanionID
                INNER JOIN Users uComp ON cp.UserID = uComp.UserID
                INNER JOIN Packages p ON b.PackageID = p.PackageID
                LEFT JOIN Ratings r ON r.BookingID = b.BookingID AND r.CustomerID = b.CustomerID
                WHERE b.BookingID = @BookingID AND b.CustomerID = @CustomerID";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@BookingID", BookingId);
                command.Parameters.AddWithValue("@CustomerID", Convert.ToInt32(Session["UserID"]));
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        ShowMessage("This booking could not be found.", false);
                        pnlRatingForm.Visible = false;
                        return;
                    }

                    string status = reader["Status"].ToString();
                    bool hasRating = Convert.ToInt32(reader["HasRating"]) == 1;
                    litCompanionName.Text = Server.HtmlEncode(reader["CompanionName"].ToString());
                    litPackageName.Text = Server.HtmlEncode(reader["PackageName"].ToString());
                    litBookingDate.Text = Convert.ToDateTime(reader["BookingDate"]).ToString("MMM. dd, yyyy");

                    if (!string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase))
                    {
                        ShowMessage("Only completed bookings can be rated.", false);
                        pnlRatingForm.Visible = false;
                    }
                    else if (hasRating)
                    {
                        ShowMessage("You have already rated this booking.", true);
                        pnlRatingForm.Visible = false;
                    }
                }
            }
        }

        // Server-side check that a star score from 1 to 5 was chosen (browser checks can be bypassed).
        protected void cvScore_ServerValidate(object source, ServerValidateEventArgs args)
        {
            int score;
            args.IsValid = int.TryParse(rblScore.SelectedValue, out score) && score >= 1 && score <= 5;
        }

        // Saves the rating and notifies the companion, all in one transaction.
        // The INSERT re-checks everything in SQL (own booking, Completed, not already rated), so a rating
        // can't be forced through by editing the URL or submitting twice; exactly 1 row must be inserted.
        protected void btnSubmitRating_Click(object sender, EventArgs e)
        {
            Page.Validate();
            if (!Page.IsValid) return;

            int customerId = Convert.ToInt32(Session["UserID"]);
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int companionId;
                        const string companionQuery = @"
                            SELECT CompanionID
                            FROM Bookings
                            WHERE BookingID = @BookingID
                              AND CustomerID = @CustomerID
                              AND Status = 'Completed'";
                        using (SqlCommand companionCommand = new SqlCommand(companionQuery, connection, transaction))
                        {
                            companionCommand.Parameters.AddWithValue("@BookingID", BookingId);
                            companionCommand.Parameters.AddWithValue("@CustomerID", customerId);
                            object companionResult = companionCommand.ExecuteScalar();
                            if (companionResult == null)
                            {
                                transaction.Rollback();
                                ShowMessage("This booking could not be rated. It may already have a rating.", false);
                                return;
                            }
                            companionId = Convert.ToInt32(companionResult);
                        }

                        const string ratingQuery = @"
                            INSERT INTO Ratings (BookingID, CustomerID, CompanionID, Score, Comment, DateCreated)
                            SELECT b.BookingID, b.CustomerID, b.CompanionID, @Score, @Comment, GETDATE()
                            FROM Bookings b
                            WHERE b.BookingID = @BookingID
                              AND b.CustomerID = @CustomerID
                              AND b.Status = 'Completed'
                              AND NOT EXISTS (
                                  SELECT 1 FROM Ratings r
                                  WHERE r.BookingID = b.BookingID AND r.CustomerID = b.CustomerID
                              )";
                        using (SqlCommand ratingCommand = new SqlCommand(ratingQuery, connection, transaction))
                        {
                            ratingCommand.Parameters.AddWithValue("@BookingID", BookingId);
                            ratingCommand.Parameters.AddWithValue("@CustomerID", customerId);
                            ratingCommand.Parameters.AddWithValue("@Score", Convert.ToInt32(rblScore.SelectedValue));
                            ratingCommand.Parameters.AddWithValue("@Comment", string.IsNullOrWhiteSpace(txtComment.Text) ? string.Empty : txtComment.Text.Trim());
                            if (ratingCommand.ExecuteNonQuery() != 1)
                            {
                                transaction.Rollback();
                                ShowMessage("This booking could not be rated. It may already have a rating.", false);
                                return;
                            }
                        }

                        const string notificationQuery = @"
                            INSERT INTO Notifications (UserID, Message, IsRead, DateCreated)
                            SELECT cp.UserID, 'A customer submitted a new rating for your completed booking.', 0, GETDATE()
                            FROM CompanionProfiles cp
                            WHERE cp.CompanionID = @CompanionID";
                        using (SqlCommand notificationCommand = new SqlCommand(notificationQuery, connection, transaction))
                        {
                            notificationCommand.Parameters.AddWithValue("@CompanionID", companionId);
                            notificationCommand.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine("Submit rating error: " + ex);
                        ShowMessage("The rating could not be submitted right now. Please try again.", false);
                        return;
                    }
                }
            }

            ShowMessage("Thank you. Your rating has been submitted successfully.", true);
            pnlRatingForm.Visible = false;
        }

        // Shows the green/red message box. The text is HTML-encoded before it is rendered.
        private void ShowMessage(string message, bool success)
        {
            pnlMessage.CssClass = success ? "rate-message success" : "rate-message error";
            litMessage.Text = Server.HtmlEncode(message);
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
