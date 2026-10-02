using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using OnlineCompanionBookingSystem;

namespace OnlineCompanionBookingManagementSystem.Customer
{
    // Customer > My Profile: edit name, email, contact number, and profile picture.
    // The name is stored as one FullName column, so it is split into first / middle / surname for the form
    // and joined back together when saving.
    public partial class Profile : System.Web.UI.Page
    {
        // Connection string from Web.config
        private string ConnStr => ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Only logged-in customers may open this page; everyone else goes back to Login.
                if (Session["UserID"] == null || Session["Role"]?.ToString() != "Customer")
                {
                    Response.Redirect("~/Account/Login.aspx");
                    return;
                }

                int userID = Convert.ToInt32(Session["UserID"]);

                LoadUserProfile(userID);
            }
        }

        // Loads the account into the form fields and sets up the avatars (top bar, dropdown, profile card):
        // the picture when one exists, otherwise the initials.
        private void LoadUserProfile(int userID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    string query = "SELECT FullName, Email, ContactNumber, ProfilePicture FROM Users WHERE UserID = @UserID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userID);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string fullName = reader["FullName"] != DBNull.Value ? reader["FullName"].ToString().Trim() : string.Empty;
                                string email = reader["Email"] != DBNull.Value ? reader["Email"].ToString().Trim() : string.Empty;
                                string contactNumber = reader["ContactNumber"] != DBNull.Value ? reader["ContactNumber"].ToString().Trim() : string.Empty;
                                string profilePic = reader["ProfilePicture"] != DBNull.Value ? reader["ProfilePicture"].ToString().Trim() : string.Empty;

                                if (!string.IsNullOrEmpty(fullName))
                                {
                                    ParseFullName(fullName);
                                }
                                else
                                {
                                    txtFirstName.Text = "User";
                                    txtSurname.Text = "";
                                    txtMiddleName.Text = "";
                                }

                                txtEmail.Text = email;
                                txtContactNumber.Text = contactNumber;

                                string displayFirstName = txtFirstName.Text.Trim();
                                if (string.IsNullOrEmpty(displayFirstName)) displayFirstName = "User";

                                litUserFirstName.Text = Server.HtmlEncode(displayFirstName);
                                litDropdownName.Text = Server.HtmlEncode(!string.IsNullOrEmpty(fullName) ? fullName : "Customer Account");
                                litDropdownEmail.Text = Server.HtmlEncode(!string.IsNullOrEmpty(email) ? email : "customer@email.com");

                                string initials = "U";
                                string nameForInitials = !string.IsNullOrEmpty(fullName) ? fullName : displayFirstName;
                                if (!string.IsNullOrEmpty(nameForInitials))
                                {
                                    string[] parts = nameForInitials.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
                                litProfileBigInitial.Text = initials;

                                // Dito na natin itinatago ang mga initials kapag may naka-upload nang profile picture
                                if (!string.IsNullOrEmpty(profilePic))
                                {
                                    imgProfilePreview.ImageUrl = profilePic;
                                    imgProfilePreview.Visible = true;
                                    divFallbackInitial.Visible = false; // Itago ang fallback container sa profile preview

                                    imgTopAvatar.ImageUrl = profilePic;
                                    imgTopAvatar.Visible = true;
                                    litAvatarInitial.Visible = false; // Itago ang initial sa topbar

                                    imgDropAvatar.ImageUrl = profilePic;
                                    imgDropAvatar.Visible = true;
                                    litAvatarInitialLg.Visible = false; // Itago ang initial sa dropdown
                                }
                                else
                                {
                                    imgProfilePreview.Visible = false;
                                    divFallbackInitial.Visible = true;

                                    imgTopAvatar.Visible = false;
                                    litAvatarInitial.Visible = true;

                                    imgDropAvatar.Visible = false;
                                    litAvatarInitialLg.Visible = true;
                                }
                            }
                            else
                            {
                                lblMessage.Text = "Babala: Walang nakitang impormasyon sa database para sa User ID na ito (ID: " + userID + ").";
                                lblMessage.CssClass = "message error";
                                lblMessage.Visible = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadUserProfile error: " + ex);
                lblMessage.Text = "We could not load your profile right now.";
                lblMessage.CssClass = "message error";
                lblMessage.Visible = true;
            }
        }

        // Splits FullName into the three text boxes:
        //   1 word  -> first name only
        //   2 words -> first name + surname
        //   3+ words -> first name, middle name (2nd word), and everything else as the surname
        private void ParseFullName(string fullName)
        {
            string[] parts = fullName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                txtFirstName.Text = parts[0];
                txtSurname.Text = "";
                txtMiddleName.Text = "";
            }
            else if (parts.Length == 2)
            {
                txtFirstName.Text = parts[0];
                txtSurname.Text = parts[1];
                txtMiddleName.Text = "";
            }
            else if (parts.Length >= 3)
            {
                txtFirstName.Text = parts[0];
                txtMiddleName.Text = parts[1];
                txtSurname.Text = string.Join(" ", parts, 2, parts.Length - 2);
            }
        }

        // Saves the profile. A new picture (JPG/PNG only) is saved to ~/Uploads/Profiles with a unique,
        // server-generated name and its path stored in Users.ProfilePicture. If no picture is chosen
        // (or the file type isn't allowed) the existing picture is left as it is.
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                int userID = Convert.ToInt32(Session["UserID"]);

                string firstName = txtFirstName.Text.Trim();
                string middleName = txtMiddleName.Text.Trim();
                string surname = txtSurname.Text.Trim();

                // Validate everything on the server first (required fields, valid email that no other account
                // uses, PH mobile number, and a real JPG/PNG photo under 2 MB) before saving a file or touching the database.
                string validationError = ProfileValidator.ValidateProfile(firstName, middleName, surname, txtEmail.Text, txtContactNumber.Text, userID);
                if (validationError == null && fileUploadAvatar.HasFile)
                {
                    validationError = ProfileValidator.ValidatePhoto(fileUploadAvatar.PostedFile);
                }
                if (validationError != null)
                {
                    lblMessage.Text = Server.HtmlEncode(validationError);
                    lblMessage.CssClass = "message error";
                    lblMessage.Visible = true;
                    return;
                }

                string combinedFullName = firstName;
                if (!string.IsNullOrEmpty(middleName))
                {
                    combinedFullName += " " + middleName;
                }
                if (!string.IsNullOrEmpty(surname))
                {
                    combinedFullName += " " + surname;
                }

                string dbPicPath = null;

                if (fileUploadAvatar.HasFile)
                {
                    string ext = Path.GetExtension(fileUploadAvatar.FileName).ToLower();
                    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                    {
                        string fileName = "User_" + userID + "_" + DateTime.Now.Ticks + ext;
                        string folderPath = Server.MapPath("~/Uploads/Profiles/");

                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        string fullPath = folderPath + fileName;
                        fileUploadAvatar.SaveAs(fullPath);
                        dbPicPath = "~/Uploads/Profiles/" + fileName;
                    }
                }

                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    // Only include ProfilePicture in the UPDATE when a new picture was uploaded
                    string query = "";
                    if (dbPicPath != null)
                    {
                        query = "UPDATE Users SET FullName = @FullName, Email = @Email, ContactNumber = @ContactNumber, ProfilePicture = @ProfilePicture WHERE UserID = @UserID";
                    }
                    else
                    {
                        query = "UPDATE Users SET FullName = @FullName, Email = @Email, ContactNumber = @ContactNumber WHERE UserID = @UserID";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FullName", combinedFullName);
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@ContactNumber", string.IsNullOrEmpty(txtContactNumber.Text) ? (object)DBNull.Value : txtContactNumber.Text.Trim());
                        if (dbPicPath != null)
                        {
                            cmd.Parameters.AddWithValue("@ProfilePicture", dbPicPath);
                        }
                        cmd.Parameters.AddWithValue("@UserID", userID);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                lblMessage.Text = "Profile successfully updated!";
                lblMessage.CssClass = "message success";
                lblMessage.Visible = true;

                LoadUserProfile(userID);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Profile update error: " + ex);
                lblMessage.Text = "We could not update your profile. Please try again.";
                lblMessage.CssClass = "message error";
                lblMessage.Visible = true;
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