using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;

namespace OnlineCompanionBookingManagementSystem
{
    // Landing page (Default.aspx): nagpapakita ng hanggang 3 verified companions bilang featured.
    public partial class Default : System.Web.UI.Page
    {
        // Connection string mula sa Web.config
        string connString = ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Isang beses lang i-load ang data; hindi na uulitin kapag postback (hal. click ng button)
            if (!IsPostBack)
            {
                LoadFeaturedCompanions();
            }
        }

        // Kinukuha ang unang 3 companion na Verified ng admin at Active ang account.
        // Ang MainTag/MinRate ay galing sa unang package nila; ang rating ay average ng Ratings (5.0 kung wala pang review).
        private void LoadFeaturedCompanions()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"SELECT TOP 3 
                                   cp.CompanionID, 
                                   u.FullName, 
                                   u.ProfilePicture, 
                                   ISNULL((SELECT TOP 1 PackageName FROM Packages WHERE CompanionID = cp.CompanionID), 'General Companion') AS MainTag,
                                   ISNULL((SELECT TOP 1 Rate FROM Packages WHERE CompanionID = cp.CompanionID), 250.00) AS MinRate,
                                   (SELECT COUNT(*) FROM Ratings WHERE CompanionID = cp.CompanionID) AS TotalReviews,
                                   ISNULL((SELECT AVG(CAST(Score AS FLOAT)) FROM Ratings WHERE CompanionID = cp.CompanionID), 5.0) AS AvgRating
                               FROM CompanionProfiles cp
                               INNER JOIN Users u ON cp.UserID = u.UserID
                               WHERE cp.VerificationStatus = 'Verified' AND u.IsActive = 1";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        rptFeaturedCompanions.DataSource = dt;
                        rptFeaturedCompanions.DataBind();
                    }
                }
            }
        }

        // Gumagawa ng HTML ng avatar: larawan kung may profile picture, kung wala ay bilog na may initials (hal. "MI").
        // Naka-encode ang mga value para hindi makapag-inject ng HTML/script ang pangalan o path.
        protected string GetCompanionAvatar(object profilePhotoObj, object fullNameObj)
        {
            string profilePhoto = profilePhotoObj == DBNull.Value ? string.Empty : Convert.ToString(profilePhotoObj);
            string fullName = fullNameObj == DBNull.Value ? "Companion" : Convert.ToString(fullNameObj).Trim();

            if (!string.IsNullOrEmpty(profilePhoto))
            {
                return $"<img class='landing-companion-avatar' src='{HttpUtility.HtmlAttributeEncode(ResolveUrl(profilePhoto))}' alt='{HttpUtility.HtmlAttributeEncode(fullName)}' />";
            }

            string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string initials = parts.Length > 1
                ? (parts[0][0].ToString() + parts[parts.Length - 1][0]).ToUpperInvariant()
                : (parts.Length == 1 ? parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant() : "CP");

            return $"<div class='companion-card-avatar landing-companion-avatar'>{HttpUtility.HtmlEncode(initials)}</div>";
        }

        // Rating na may isang decimal (hal. 4.0); 5.0 ang default kung hindi ma-parse
        protected string FormatRating(object ratingObj)
        {
            if (ratingObj != null && double.TryParse(ratingObj.ToString(), out double rating))
            {
                return rating.ToString("0.0");
            }
            return "5.0";
        }

        // Mga button sa landing page: dumidiretso sa login o registration ang mga bisita
        protected void Button1_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Account/Login.aspx");
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Account/RegisterCustomer.aspx");
        }

        protected void BrowseCompanionsBtn_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Account/Login.aspx");
        }
    }
}