using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;
using OnlineCompanionBookingSystem;

namespace OnlineCompanionBookingManagementSystem.Customer
{
    // Customer > Browse Companions: the list of verified, active companions with search, an activity filter,
    // and sorting (rating, reviews, price). Clicking a card opens that companion's profile.
    public partial class BrowseCompanions : System.Web.UI.Page
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
                LoadActivitiesDropdown();
                LoadCompanions();
            }
        }

        // Fills the top bar and profile dropdown (name, email, picture or initials) and the unread-notification badge.
        private void LoadUserProfile(int userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    // Isinama na natin ang ProfilePicture sa query
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
                                        initials = (parts[0][0].ToString() + parts[1][0].ToString()).ToUpper();
                                    }
                                    else if (parts.Length == 1 && parts[0].Length > 0)
                                    {
                                        initials = parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
                                    }
                                }

                                litAvatarInitial.Text = initials;
                                litAvatarInitialLg.Text = initials;

                                // Pamamahala sa Profile Picture kung may naka-upload na
                                if (!string.IsNullOrEmpty(profilePic))
                                {
                                    Image imgTop = FindControl("imgTopAvatar") as Image;
                                    Image imgDrop = FindControl("imgDropAvatar") as Image;

                                    if (imgTop != null) { imgTop.ImageUrl = profilePic; imgTop.Visible = true; }
                                    if (imgDrop != null) { imgDrop.ImageUrl = profilePic; imgDrop.Visible = true; }

                                    if (litAvatarInitial != null) litAvatarInitial.Visible = false;
                                    if (litAvatarInitialLg != null) litAvatarInitialLg.Visible = false;
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

        // Fills the activity filter with every distinct activity name that any companion has added.
        private void LoadActivitiesDropdown()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    string query = "SELECT DISTINCT ActivityName FROM Activities WHERE ActivityName IS NOT NULL ORDER BY ActivityName ASC";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string act = reader["ActivityName"].ToString();
                                ddlActivity.Items.Add(new ListItem(act, act));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadActivitiesDropdown Error: " + ex.Message);
            }
        }

        // Builds the companion list. Only Verified and active companions are shown. The SQL is extended with the
        // search text, the activity filter, and the chosen sort order. User input goes through parameters
        // (@Search, @Activity), and the ORDER BY comes from a fixed switch, so nothing typed by the user is
        // concatenated into the query. MinRate is the cheapest package; rating defaults to 5.0 when there are no reviews.
        private void LoadCompanions()
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
                            (SELECT STUFF((SELECT ', ' + a.ActivityName 
                                           FROM Activities a 
                                           WHERE a.CompanionID = cp.CompanionID 
                                           FOR XML PATH('')), 1, 2, '')) AS Activities,
                            ISNULL((SELECT MIN(Rate) FROM Packages WHERE CompanionID = cp.CompanionID), 250) AS MinRate,
                            ISNULL((SELECT AVG(CAST(Score AS FLOAT)) FROM Ratings WHERE CompanionID = cp.CompanionID), 5.0) AS AvgRating,
                            (SELECT COUNT(*) FROM Ratings WHERE CompanionID = cp.CompanionID) AS TotalReviews
                        FROM CompanionProfiles cp
                        INNER JOIN Users u ON cp.UserID = u.UserID
                        WHERE cp.VerificationStatus = 'Verified' 
                          AND ISNULL(u.IsActive, 1) = 1";

                    if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                    {
                        query += " AND (u.FullName LIKE @Search OR cp.Bio LIKE @Search)";
                    }

                    if (ddlActivity.SelectedValue != "All")
                    {
                        query += " AND cp.CompanionID IN (SELECT CompanionID FROM Activities WHERE ActivityName = @Activity)";
                    }

                    switch (ddlSortBy.SelectedValue)
                    {
                        case "RatingDesc":
                            query += " ORDER BY AvgRating DESC, TotalReviews DESC";
                            break;
                        case "ReviewsDesc":
                            query += " ORDER BY TotalReviews DESC, AvgRating DESC";
                            break;
                        case "PriceAsc":
                            query += " ORDER BY MinRate ASC";
                            break;
                        case "PriceDesc":
                            query += " ORDER BY MinRate DESC";
                            break;
                        default:
                            query += " ORDER BY AvgRating DESC";
                            break;
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                        {
                            cmd.Parameters.AddWithValue("@Search", "%" + txtSearch.Text.Trim() + "%");
                        }

                        if (ddlActivity.SelectedValue != "All")
                        {
                            cmd.Parameters.AddWithValue("@Activity", ddlActivity.SelectedValue);
                        }

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            foreach (DataRow row in dt.Rows)
                            {
                                if (row["Activities"] == DBNull.Value || string.IsNullOrEmpty(row["Activities"].ToString()))
                                {
                                    row["Activities"] = "General Companion";
                                }
                            }

                            // One query for the weekly schedules of every companion on the page, shown on each card
                            if (conn.State != ConnectionState.Open) conn.Open();   // Fill closes the connection again
                            var slots = AvailabilityService.LoadFor(conn, dt.Rows.Cast<DataRow>().Select(r => Convert.ToInt32(r["CompanionID"])));
                            dt.Columns.Add("AvailabilityText", typeof(string));
                            dt.Columns.Add("HasAvailability", typeof(bool));
                            foreach (DataRow row in dt.Rows)
                            {
                                List<AvailabilityService.Slot> list;
                                bool has = slots.TryGetValue(Convert.ToInt32(row["CompanionID"]), out list);
                                row["AvailabilityText"] = AvailabilityService.Summary(has ? list : null);
                                row["HasAvailability"] = has;
                            }

                            rptCompanions.DataSource = dt;
                            rptCompanions.DataBind();

                            litCompanionCount.Text = dt.Rows.Count.ToString();
                            pnlNoResults.Visible = (dt.Rows.Count == 0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                pnlNoResults.Visible = true;
                System.Diagnostics.Debug.WriteLine("LoadCompanions Error: " + ex.Message);
            }
        }

        // HTML of the card avatar: the profile picture if there is one, otherwise a circle with initials.
        // Values are encoded to prevent HTML injection.
        protected string GetCompanionAvatar(object pathObj, object fullNameObj)
        {
            string path = pathObj == null || pathObj == DBNull.Value
                ? string.Empty
                : Convert.ToString(pathObj).Trim();
            string fullName = fullNameObj == null || fullNameObj == DBNull.Value
                ? "Companion"
                : Convert.ToString(fullNameObj).Trim();

            if (!string.IsNullOrWhiteSpace(path))
            {
                return "<img class='companion-card-avatar browse-card-avatar' src='" +
                    HttpUtility.HtmlAttributeEncode(ResolveUrl(path)) +
                    "' alt='" + HttpUtility.HtmlAttributeEncode(fullName) + "' />";
            }

            string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string initials = parts.Length > 1
                ? (parts[0][0].ToString() + parts[parts.Length - 1][0]).ToUpperInvariant()
                : (parts.Length == 1 ? parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant() : "CP");

            return $"<div class='companion-card-avatar browse-card-avatar'>{HttpUtility.HtmlEncode(initials)}</div>";
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

        // Shortens the bio to 85 characters for the card ("..." added when cut).
        protected string TruncateBio(object bioObj)
        {
            if (bioObj == DBNull.Value || bioObj == null) return "No bio available.";
            string bio = bioObj.ToString();
            if (bio.Length > 85)
            {
                bio = bio.Substring(0, 85) + "...";
            }
            // Encode last (after cutting) so an HTML entity can never be split in half; the .aspx prints this as-is.
            return HttpUtility.HtmlEncode(bio);
        }

        // Apply button: reload the list with the current search / activity / sort values.
        protected void btnFilter_Click(object sender, EventArgs e)
        {
            LoadCompanions();
        }

        // Reset button: clear all filters back to the defaults and reload.
        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            ddlActivity.SelectedValue = "All";
            ddlSortBy.SelectedValue = "RatingDesc";
            LoadCompanions();
        }

        // "View profile" on a card (CommandArgument = CompanionID): opens CompanionProfile.aspx?id=...
        protected void rptCompanions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "ViewProfile")
            {
                string companionId = e.CommandArgument.ToString();
                Response.Redirect($"~/Customer/CompanionProfile.aspx?id={companionId}");
            }
        }

        // Clears the session and returns to the landing page.
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("~/Default.aspx");
        }
    }
}