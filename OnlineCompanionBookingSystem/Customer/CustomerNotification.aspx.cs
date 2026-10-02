using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using OnlineCompanionBookingSystem;

namespace OnlineCompanionBookingManagementSystem.Customer
{
    // Customer > Notifications: the customer's in-app messages (booking confirmed/declined, etc.).
    // When a notification is linked to a booking, the page also shows that booking's details and
    // the companion's contact information. Most helper methods below format values for the .aspx
    // markup and HTML-encode them so they are safe to render.
    public partial class Notifications : System.Web.UI.Page
    {
        // Connection string from Web.config
        private string ConnStr => ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Only logged-in customers may open this page; everyone else goes back to Login.
            if (Session["UserID"] == null || Session["Role"]?.ToString() != "Customer")
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                int customerUserId = Convert.ToInt32(Session["UserID"]);

                LoadUserProfile(customerUserId);
                BindNotifications(customerUserId);

                // The customer has now seen the notifications, so the sidebar's "new" counter goes away
                // (the notifications themselves stay "unread" until marked as read)
                NavCounts.MarkSeen(NavCounts.Customer, customerUserId, "notifications");
            }
        }

        // Fills the top bar and profile dropdown (name, email, picture or initials) and the unread-notification badge.
        private void LoadUserProfile(int userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    // Isinama na natin ang ProfilePicture sa pag-query mula sa Users table
                    string query = "SELECT FullName, Email, ProfilePicture FROM Users WHERE UserID = @UserID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string fullName = reader["FullName"] != DBNull.Value ? reader["FullName"].ToString().Trim() : "User";
                                string email = reader["Email"] != DBNull.Value ? reader["Email"].ToString().Trim() : "customer@ocbs.com";
                                string profilePic = reader["ProfilePicture"] != DBNull.Value ? reader["ProfilePicture"].ToString().Trim() : string.Empty;
                                string firstName = fullName.Split(' ')[0];

                                litUserFirstName.Text = Server.HtmlEncode(firstName);
                                litDropdownName.Text = Server.HtmlEncode(fullName);
                                litDropdownEmail.Text = Server.HtmlEncode(email);

                                string initials = "U";
                                if (!string.IsNullOrEmpty(fullName))
                                {
                                    string[] parts = fullName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (parts.Length >= 2)
                                    {
                                        initials = (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
                                    }
                                    else if (parts.Length == 1 && parts[0].Length > 0)
                                    {
                                        initials = parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
                                    }
                                }

                                litAvatarInitial.Text = initials;
                                litAvatarInitialLg.Text = initials;

                                // Pamamahala sa Profile Picture kung may naka-upload na
                                // Tandaan: Siguraduhing may ID o Control din ang imgTopAvatar at imgDropAvatar sa Notifications.aspx kung gagamitin ito
                                if (!string.IsNullOrEmpty(profilePic))
                                {
                                    if (imgTopAvatar != null) { imgTopAvatar.ImageUrl = profilePic; imgTopAvatar.Visible = true; }
                                    if (imgDropAvatar != null) { imgDropAvatar.ImageUrl = profilePic; imgDropAvatar.Visible = true; }

                                    // Itago ang text initials kung may picture na
                                    if (litAvatarInitial != null) litAvatarInitial.Visible = false;
                                    if (litAvatarInitialLg != null) litAvatarInitialLg.Visible = false;
                                }
                                else
                                {
                                    if (imgTopAvatar != null) imgTopAvatar.Visible = false;
                                    if (imgDropAvatar != null) imgDropAvatar.Visible = false;

                                    if (litAvatarInitial != null) litAvatarInitial.Visible = true;
                                    if (litAvatarInitialLg != null) litAvatarInitialLg.Visible = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadUserProfile Error: " + ex.Message);
            }
        }

        // Loads the customer's notifications, newest first. OUTER APPLY pulls in the linked booking (if any) and,
        // through it, the package and the companion's details. The booking is only joined when it belongs to this
        // customer (b1.CustomerID = n.UserID), so another customer's booking can never be exposed.
        private void BindNotifications(int userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    string query = @"
                        SELECT n.NotificationID, n.Message, n.IsRead, n.DateCreated,
                               b.BookingID AS RelatedBookingID,
                               b.Status AS BookingStatus,
                               b.BookingDate,
                               b.BookingTime,
                               p.PackageName,
                               customer.FullName AS CustomerName,
                               customer.Email AS CustomerEmail,
                               customer.ContactNumber AS CustomerContact,
                               comp.FullName AS CompanionName,
                               comp.Email AS CompanionEmail,
                               comp.ContactNumber AS CompanionContact,
                               comp.ProfilePicture AS CompanionProfilePicture
                        FROM Notifications n
                        OUTER APPLY (
                            SELECT TOP 1 b1.*
                            FROM Bookings b1
                            WHERE b1.BookingID = n.RelatedBookingID
                              AND b1.CustomerID = n.UserID
                        ) b
                        LEFT JOIN Packages p ON p.PackageID = b.PackageID
                        LEFT JOIN Users customer ON customer.UserID = b.CustomerID
                        LEFT JOIN CompanionProfiles cp ON cp.CompanionID = b.CompanionID
                        LEFT JOIN Users comp ON comp.UserID = cp.UserID
                        WHERE n.UserID = @UserID
                        ORDER BY n.DateCreated DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                rptNotifications.DataSource = dt;
                                rptNotifications.DataBind();
                                rptNotifications.Visible = true;
                                pnlNoNotifs.Visible = false;
                            }

                            else
                            {
                                rptNotifications.Visible = false;
                                pnlNoNotifs.Visible = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("BindNotifications Error: " + ex.Message);
                rptNotifications.Visible = false;
                pnlNoNotifs.Visible = true;
            }
        }

        // The notification text, HTML-encoded.
        protected string GetNotificationMessage(object messageObj, object relatedBookingIdObj)
        {
            string message = messageObj == null || messageObj == DBNull.Value
                ? string.Empty
                : messageObj.ToString();

            return HttpUtility.HtmlEncode(message);
        }

        // True when the notification is linked to a booking (so the page can show booking/companion details).
        protected bool HasConfirmedBooking(object relatedBookingIdObj)
        {
            return GetBookingId(relatedBookingIdObj).HasValue;
        }

        // Link to the companion's profile, but only for the customer's own Confirmed bookings.
        // Returns "" (no link) in every other case.
        protected string GetCompanionProfileUrl(object relatedBookingIdObj)
        {
            int? bookingId = GetBookingId(relatedBookingIdObj);
            if (!bookingId.HasValue)
            {
                return string.Empty;
            }

            try
            {
                int customerId = Convert.ToInt32(Session["UserID"]);
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT CompanionID
                    FROM Bookings
                    WHERE BookingID = @BookingID
                      AND CustomerID = @CustomerID
                      AND Status = 'Confirmed'", conn))
                {
                    cmd.Parameters.AddWithValue("@BookingID", bookingId.Value);
                    cmd.Parameters.AddWithValue("@CustomerID", customerId);
                    conn.Open();
                    object companionId = cmd.ExecuteScalar();
                    if (companionId != null && companionId != DBNull.Value)
                    {
                        return ResolveUrl("~/Customer/CompanionProfile.aspx?id=" + Convert.ToInt32(companionId));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetCompanionProfileUrl Error: " + ex.Message);
            }

            return string.Empty;
        }

        // HTML of the companion's avatar: picture if there is one, otherwise a circle with initials.
        protected string GetCompanionAvatar(object pathObj, object nameObj)
        {
            string path = pathObj == null || pathObj == DBNull.Value ? string.Empty : pathObj.ToString().Trim();
            string name = nameObj == null || nameObj == DBNull.Value ? "Companion" : nameObj.ToString().Trim();

            if (!string.IsNullOrWhiteSpace(path))
            {
                return "<img class=\"notification-companion-avatar\" src=\"" +
                    HttpUtility.HtmlAttributeEncode(ResolveUrl(path)) + "\" alt=\"" +
                    HttpUtility.HtmlAttributeEncode(name) + "\" />";
            }

            string[] parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string initials = parts.Length > 1
                ? (parts[0][0].ToString() + parts[parts.Length - 1][0]).ToUpperInvariant()
                : (parts.Length == 1 ? parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant() : "CP");

            return "<div class=\"notification-companion-avatar notification-companion-initials\">" +
                HttpUtility.HtmlEncode(initials) + "</div>";
        }

        // HTML block with the booking's package, schedule ("Oct. 05, 2026 • 02:00 PM"), and status.
        protected string GetBookingDetails(object packageObj, object dateObj, object timeObj, object statusObj)
        {
            string package = packageObj == null || packageObj == DBNull.Value ? "Companion service" : packageObj.ToString();
            string date = dateObj == null || dateObj == DBNull.Value
                ? "Date to be confirmed"
                : Convert.ToDateTime(dateObj).ToString("MMM. dd, yyyy");
            string time = timeObj == null || timeObj == DBNull.Value
                ? "Time to be confirmed"
                : TimeSpan.TryParse(timeObj.ToString(), out TimeSpan parsedTime)
                    ? DateTime.Today.Add(parsedTime).ToString("hh:mm tt")
                    : timeObj.ToString();
            string status = statusObj == null || statusObj == DBNull.Value ? "Confirmed" : statusObj.ToString();

            return "<div class=\"notification-booking-details\">" +
                   "<span><strong>Package:</strong> " + HttpUtility.HtmlEncode(package) + "</span>" +
                   "<span><strong>Schedule:</strong> " + HttpUtility.HtmlEncode(date + " • " + time) + "</span>" +
                   "<span class=\"notification-status\"><strong>Status:</strong> " + HttpUtility.HtmlEncode(status) + "</span>" +
                   "</div>";
        }

        // HTML block with the companion's email and contact number ("Not provided" when missing).
        protected string GetContactDetails(object emailObj, object contactObj)
        {
            string email = emailObj == null || emailObj == DBNull.Value || string.IsNullOrWhiteSpace(emailObj.ToString())
                ? "Not provided"
                : emailObj.ToString().Trim();
            string contact = contactObj == null || contactObj == DBNull.Value || string.IsNullOrWhiteSpace(contactObj.ToString())
                ? "Not provided"
                : contactObj.ToString().Trim();

            return "<div class=\"notification-contact-details\">" +
                   "<span><strong>Email</strong><br />" + HttpUtility.HtmlEncode(email) + "</span>" +
                   "<span><strong>Contact number</strong><br />" + HttpUtility.HtmlEncode(contact) + "</span>" +
                   "</div>";
        }

        // Small helpers used by the .aspx: each returns a safely encoded value, or the fallback when it is empty.
        // EncodeValue -> for text between tags.
        protected string EncodeValue(object valueObj, string fallback)
        {
            string value = valueObj == null || valueObj == DBNull.Value
                ? fallback
                : valueObj.ToString().Trim();
            return HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(value) ? fallback : value);
        }

        // GetAttributeValue -> for values placed inside an HTML attribute (e.g. data-* attributes).
        protected string GetAttributeValue(object valueObj, string fallback)
        {
            return HttpUtility.HtmlAttributeEncode(
                valueObj == null || valueObj == DBNull.Value || string.IsNullOrWhiteSpace(valueObj.ToString())
                    ? fallback
                    : valueObj.ToString().Trim());
        }

        // GetImageUrl -> a resolved, attribute-safe image URL ("" if there is no picture).
        protected string GetImageUrl(object pathObj)
        {
            if (pathObj == null || pathObj == DBNull.Value || string.IsNullOrWhiteSpace(pathObj.ToString()))
            {
                return string.Empty;
            }

            return HttpUtility.HtmlAttributeEncode(ResolveUrl(pathObj.ToString().Trim()));
        }

        // GetInitialsValue -> initials of a name (e.g. "Maria Ipia" -> "MI"), attribute-safe.
        protected string GetInitialsValue(object nameObj)
        {
            string name = nameObj == null || nameObj == DBNull.Value ? "Companion" : nameObj.ToString().Trim();
            string[] parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string initials = parts.Length > 1
                ? (parts[0][0].ToString() + parts[parts.Length - 1][0]).ToUpperInvariant()
                : (parts.Length == 1 ? parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant() : "CP");
            return HttpUtility.HtmlAttributeEncode(initials);
        }

        // GetScheduleValue -> "Oct. 05, 2026 • 02:00 PM" as an attribute-safe string.
        protected string GetScheduleValue(object dateObj, object timeObj)
        {
            string date = dateObj == null || dateObj == DBNull.Value
                ? "Date to be confirmed"
                : Convert.ToDateTime(dateObj).ToString("MMM. dd, yyyy");
            string time = timeObj == null || timeObj == DBNull.Value
                ? "Time to be confirmed"
                : TimeSpan.TryParse(timeObj.ToString(), out TimeSpan parsedTime)
                    ? DateTime.Today.Add(parsedTime).ToString("hh:mm tt")
                    : timeObj.ToString();
            return HttpUtility.HtmlAttributeEncode(date + " • " + time);
        }

        // Converts the RelatedBookingID value from the data source to an int; null when the notification has no booking.
        // (The parameter is named "messageObj" but it actually receives the related booking id.)
        private int? GetBookingId(object messageObj)
        {
            if (messageObj == null || messageObj == DBNull.Value)
            {
                return null;
            }

            return int.TryParse(messageObj.ToString(), out int bookingId)
                ? (int?)bookingId
                : null;
        }

        // Marks all of the customer's notifications as read, then refreshes the list and the unread badge.
        protected void btnMarkAllRead_Click(object sender, EventArgs e)
        {
            try
            {
                int userId = Convert.ToInt32(Session["UserID"]);

                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    string query = "UPDATE Notifications SET IsRead = 1 WHERE UserID = @UserID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                BindNotifications(userId);
                LoadUserProfile(userId); // Para ma-update ang unread badge sa sidebar/topbar
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("btnMarkAllRead_Click Error: " + ex.Message);
            }
        }

        // Clears the session and returns to the landing page.
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Default.aspx");
        }
    }
}