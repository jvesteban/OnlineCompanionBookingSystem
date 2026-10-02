using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OnlineCompanionBookingSystem
{
    /// <summary>
    /// Shared helpers for the Companion pages' common layout. All Companion pages have the same top bar and
    /// profile dropdown, so this fills the dropdown (photo or initials, name, email) in one place instead of
    /// repeating the same code in every page.
    /// </summary>
    public static class CompanionUi
    {
        // Fills the profile dropdown in the top bar. The page must contain the controls
        // litAccountName, litAccountEmail, litAvatarInitialLg, and imgDropAvatar (all Companion pages do).
        // Never throws: if something is wrong the dropdown just keeps its default text.
        public static void FillAccountMenu(Page page, int userId)
        {
            try
            {
                string fullName, email, picture;
                string connStr = ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;
                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand("SELECT FullName, Email, ProfilePicture FROM Users WHERE UserID = @UserID", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return;
                        fullName = reader["FullName"] == DBNull.Value ? "Companion" : reader["FullName"].ToString().Trim();
                        email = reader["Email"] == DBNull.Value ? "No email available" : reader["Email"].ToString().Trim();
                        picture = reader["ProfilePicture"] == DBNull.Value ? string.Empty : reader["ProfilePicture"].ToString().Trim();
                    }
                }

                var litName = page.FindControl("litAccountName") as Literal;
                var litEmail = page.FindControl("litAccountEmail") as Literal;
                var litInitials = page.FindControl("litAvatarInitialLg") as Literal;
                var img = page.FindControl("imgDropAvatar") as Image;

                // Names and emails are typed by users, so they are HTML-encoded before they are shown
                if (litName != null) litName.Text = HttpUtility.HtmlEncode(fullName);
                if (litEmail != null) litEmail.Text = HttpUtility.HtmlEncode(email);

                bool hasPicture = !string.IsNullOrWhiteSpace(picture);
                if (img != null)
                {
                    img.Visible = hasPicture;
                    if (hasPicture) img.ImageUrl = picture;
                }
                if (litInitials != null)
                {
                    litInitials.Visible = !hasPicture;
                    litInitials.Text = HttpUtility.HtmlEncode(Initials(fullName));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CompanionUi.FillAccountMenu error: " + ex);
            }
        }

        // "Juan Dela Cruz" -> "JC"; one word -> its first two letters; "C" if the name is empty
        private static string Initials(string fullName)
        {
            string[] parts = (fullName ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "C";
            return parts.Length > 1
                ? (parts[0][0].ToString() + parts[parts.Length - 1][0]).ToUpperInvariant()
                : parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();
        }
    }
}
