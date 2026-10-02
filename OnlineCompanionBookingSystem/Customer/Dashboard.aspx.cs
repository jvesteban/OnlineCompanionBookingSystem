using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

namespace OnlineCompanionBookingSystem.Customer
{
    // Customer > Dashboard: the customer's home page. Shows quick stats (total / upcoming / completed bookings,
    // unread notifications), the next upcoming booking, and 3 top-rated companions with a quick-view popup.
    public partial class Dashboard : System.Web.UI.Page
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
                LoadQuickStats(customerUserId);
                LoadUpcomingBooking(customerUserId);
                LoadRecommendedCompanions();
            }
        }

        // Fills the greeting, top bar, and profile dropdown (name, email, picture or initials).
        private void LoadUserProfile(int userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
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
                                litWelcomeName.Text = Server.HtmlEncode(firstName);
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

                                Image imgTop = FindControl("imgTopAvatar") as Image;
                                Image imgDrop = FindControl("imgDropAvatar") as Image;

                                if (!string.IsNullOrEmpty(profilePic))
                                {
                                    if (imgTop != null) { imgTop.ImageUrl = profilePic; imgTop.Visible = true; }
                                    if (imgDrop != null) { imgDrop.ImageUrl = profilePic; imgDrop.Visible = true; }

                                    if (litAvatarInitial != null) litAvatarInitial.Visible = false;
                                    if (litAvatarInitialLg != null) litAvatarInitialLg.Visible = false;
                                }
                                else
                                {
                                    if (imgTop != null) imgTop.Visible = false;
                                    if (imgDrop != null) imgDrop.Visible = false;

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

        // The stat cards. "Upcoming" = today or later and still Pending/Confirmed. Also updates the
        // unread-notification badge in the sidebar.
        private void LoadQuickStats(int userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Bookings WHERE CustomerID = @UserID", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        litTotalBookings.Text = cmd.ExecuteScalar().ToString();
                    }

                    using (SqlCommand cmd = new SqlCommand(@"
                        SELECT COUNT(*) FROM Bookings 
                        WHERE CustomerID = @UserID 
                          AND BookingDate >= CAST(GETDATE() AS DATE) 
                          AND Status IN ('Confirmed', 'Pending')", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        litUpcomingBookings.Text = cmd.ExecuteScalar().ToString();
                    }

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Bookings WHERE CustomerID = @UserID AND Status = 'Completed'", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        litCompletedBookings.Text = cmd.ExecuteScalar().ToString();
                    }

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Notifications WHERE UserID = @UserID AND IsRead = 0", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        int unread = Convert.ToInt32(cmd.ExecuteScalar());
                        litUnreadNotifs.Text = unread.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadQuickStats Error: " + ex.Message);
            }
        }

        // The single nearest upcoming booking (earliest date, then time) that is Pending or Confirmed.
        // Shows an empty-state panel when there is none.
        private void LoadUpcomingBooking(int userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    string query = @"
                        SELECT TOP 1 
                            b.BookingID,
                            compUser.FullName AS CompanionName,
                            compUser.ProfilePicture AS ProfileImg,
                            p.PackageName,
                            b.BookingDate,
                            b.BookingTime,
                            b.Status
                        FROM Bookings b
                        INNER JOIN CompanionProfiles cp ON b.CompanionID = cp.CompanionID
                        INNER JOIN Users compUser ON cp.UserID = compUser.UserID
                        INNER JOIN Packages p ON b.PackageID = p.PackageID
                        WHERE b.CustomerID = @UserID 
                          AND b.BookingDate >= CAST(GETDATE() AS DATE)
                          AND b.Status IN ('Confirmed', 'Pending')
                        ORDER BY b.BookingDate ASC, b.BookingTime ASC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                pnlHasUpcoming.Visible = true;
                                pnlNoUpcoming.Visible = false;

                                litUpcomingCompanionName.Text = Server.HtmlEncode(reader["CompanionName"].ToString());
                                litUpcomingPackage.Text = Server.HtmlEncode(reader["PackageName"].ToString());

                                DateTime bDate = Convert.ToDateTime(reader["BookingDate"]);
                                string bTime = reader["BookingTime"].ToString();
                                litUpcomingDateTime.Text = $"{bDate:MMM. dd, yyyy} &nbsp; 🕑 {Server.HtmlEncode(bTime)}";

                                string status = reader["Status"].ToString();
                                litUpcomingStatus.Text = Server.HtmlEncode(status);

                                litUpcomingCompanionAvatar.Text = GetCompanionAvatar(
                                    reader["ProfileImg"],
                                    reader["CompanionName"]);
                            }
                            else
                            {
                                pnlHasUpcoming.Visible = false;
                                pnlNoUpcoming.Visible = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                pnlHasUpcoming.Visible = false;
                pnlNoUpcoming.Visible = true;
                System.Diagnostics.Debug.WriteLine("LoadUpcomingBooking Error: " + ex.Message);
            }
        }

        // The 3 highest-rated Verified, active companions (ties broken by number of reviews).
        private void LoadRecommendedCompanions()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    string query = @"
                        SELECT TOP 3
                            cp.CompanionID,
                            u.FullName,
                            u.ProfilePicture AS ProfilePicturePath,
                            ISNULL((SELECT TOP 1 ActivityName FROM Activities WHERE CompanionID = cp.CompanionID), 'Companion') AS MainTag,
                            ISNULL((SELECT MIN(Rate) FROM Packages WHERE CompanionID = cp.CompanionID), 250) AS MinRate,
                            ISNULL((SELECT AVG(CAST(Score AS FLOAT)) FROM Ratings WHERE CompanionID = cp.CompanionID), 5.0) AS AvgRating,
                            (SELECT COUNT(*) FROM Ratings WHERE CompanionID = cp.CompanionID) AS TotalReviews
                        FROM CompanionProfiles cp
                        INNER JOIN Users u ON cp.UserID = u.UserID
                        WHERE cp.VerificationStatus = 'Verified' 
                          AND ISNULL(u.IsActive, 1) = 1
                        ORDER BY AvgRating DESC, TotalReviews DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            rptRecommended.DataSource = dt;
                            rptRecommended.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadRecommendedCompanions Error: " + ex.Message);
            }
        }

        // Avatar helpers used by the .aspx. Both build the same HTML; they only differ in the CSS class
        // (companion cards vs. the "upcoming booking" panel).
        protected string GetCompanionCardAvatar(object pathObj, object fullNameObj)
        {
            return BuildCompanionAvatar(pathObj, fullNameObj, "companion-card-avatar");
        }

        protected string GetCompanionAvatar(object pathObj, object fullNameObj)
        {
            return BuildCompanionAvatar(pathObj, fullNameObj, "upcoming-img");
        }

        // Returns an <img> when the companion has a picture, otherwise a circle with their initials.
        private string BuildCompanionAvatar(object pathObj, object fullNameObj, string cssClass)
        {
            string path = pathObj == null || pathObj == DBNull.Value ? string.Empty : pathObj.ToString().Trim();
            string fullName = fullNameObj == null || fullNameObj == DBNull.Value
                ? "Companion"
                : fullNameObj.ToString().Trim();

            if (!string.IsNullOrWhiteSpace(path))
            {
                return "<img class=\"" + cssClass + "\" src=\"" + HttpUtility.HtmlAttributeEncode(ResolveUrl(path)) + "\" alt=\"" +
                    HttpUtility.HtmlAttributeEncode(fullName) + "\" />";
            }

            return "<div class=\"" + cssClass + " companion-avatar-initials\">" +
                HttpUtility.HtmlEncode(GetInitials(fullName)) + "</div>";
        }

        // Initials for the avatar, e.g. "Juan Dela Cruz" -> "JC"; "C" if the name is empty.
        private string GetInitials(string fullName)
        {
            string[] parts = (fullName ?? string.Empty)
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                return "C";
            }

            return parts.Length > 1
                ? (parts[0][0].ToString() + parts[parts.Length - 1][0]).ToUpperInvariant()
                : parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();
        }

        // Rating as stars out of 5, e.g. 4.2 -> "★★★★☆" (rounded, minimum 1 star).
        protected string FormatStars(object ratingObj)
        {
            double rating = ratingObj != DBNull.Value ? Convert.ToDouble(ratingObj) : 5.0;
            int fullStars = (int)Math.Round(rating);
            fullStars = Math.Max(1, Math.Min(5, fullStars));

            string stars = new string('★', fullStars);
            string empty = new string('☆', 5 - fullStars);
            return stars + empty;
        }

        // "View" on a recommended companion (CommandArgument = CompanionID): opens the quick-view popup.
        protected void rptRecommended_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "ViewProfile")
            {
                int companionId = Convert.ToInt32(e.CommandArgument);
                LoadCompanionModalDetails(companionId);
            }
        }

        // Fills the quick-view popup (name, specialty, bio, starting rate, rating) and points its
        // "Book" button to the full profile page, then shows the popup.
        private void LoadCompanionModalDetails(int companionId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    string query = @"
                        SELECT 
                            cp.CompanionID,
                            u.FullName,
                            u.ProfilePicture AS ProfilePicturePath,
                            cp.Bio,
                            ISNULL((SELECT TOP 1 ActivityName FROM Activities WHERE CompanionID = cp.CompanionID), 'Companion') AS MainTag,
                            ISNULL((SELECT MIN(Rate) FROM Packages WHERE CompanionID = cp.CompanionID), 250) AS MinRate,
                            ISNULL((SELECT AVG(CAST(Score AS FLOAT)) FROM Ratings WHERE CompanionID = cp.CompanionID), 5.0) AS AvgRating,
                            (SELECT COUNT(*) FROM Ratings WHERE CompanionID = cp.CompanionID) AS TotalReviews
                        FROM CompanionProfiles cp
                        INNER JOIN Users u ON cp.UserID = u.UserID
                        WHERE cp.CompanionID = @CompanionID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CompanionID", companionId);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                litModalFullName.Text = Server.HtmlEncode(reader["FullName"].ToString());
                                litModalTag.Text = Server.HtmlEncode(reader["MainTag"].ToString());
                                // The bio is free text typed by the companion, so it must be HTML-encoded before display
                                litModalBio.Text = Server.HtmlEncode(reader["Bio"] != DBNull.Value && !string.IsNullOrWhiteSpace(reader["Bio"].ToString())
                                    ? reader["Bio"].ToString()
                                    : "No description provided by this companion yet.");
                                litModalRate.Text = Convert.ToDecimal(reader["MinRate"]).ToString("N0");

                                double avgRating = Convert.ToDouble(reader["AvgRating"]);
                                int totalReviews = Convert.ToInt32(reader["TotalReviews"]);
                                litModalRating.Text = $"{avgRating:0.0} ★ ({totalReviews} reviews)";

                                litModalProfileAvatar.Text = BuildCompanionAvatar(
                                    reader["ProfilePicturePath"],
                                    reader["FullName"],
                                    "modal-avatar");

                                hlBookThisCompanion.NavigateUrl = $"~/Customer/CompanionProfile.aspx?id={companionId}";
                                pnlCompanionModal.Visible = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadCompanionModalDetails Error: " + ex.Message);
            }
        }

        // Closes the quick-view popup.
        protected void btnCloseModal_Click(object sender, EventArgs e)
        {
            pnlCompanionModal.Visible = false;
        }

        // Clears the session and returns to the landing page.
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("~/Default.aspx");
        }

        // Quick navigation buttons on the dashboard.
        protected void btnBrowseNow_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Customer/BrowseCompanion.aspx");
        }

        protected void btnViewBooking_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Customer/CustomerMyBookings.aspx");
        }
    }
}