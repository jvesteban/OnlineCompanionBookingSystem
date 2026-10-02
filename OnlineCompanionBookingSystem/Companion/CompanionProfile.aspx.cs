using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI;

namespace OnlineCompanionBookingSystem.Companion
{
    // Companion > My Profile: edit name, email, contact number, and profile picture.
    // The name is stored as one FullName column, so it is split into first / middle / surname for the form
    // and joined back together when saving.
    public partial class CompanionProfile : Page
    {
        // Connection string from Web.config
        private string ConnectionString => ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Only logged-in companions may open this page; everyone else goes back to Login.
            if (Session["UserID"] == null || !string.Equals(Session["Role"]?.ToString(), "Companion", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                LoadUserProfile(userId);
                LoadBio(userId);
                // Fill the shared profile dropdown in the top bar (name, email, photo)
                CompanionUi.FillAccountMenu(this, userId);
            }
        }

        // Loads the account into the form fields, and sets up the avatar in the top bar and on the profile card
        // (picture if there is one, otherwise initials).
        private void LoadUserProfile(int userId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    const string query = "SELECT FullName, Email, ContactNumber, ProfilePicture FROM Users WHERE UserID = @UserID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userId);
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
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
                                    txtFirstName.Text = "Companion";
                                    txtSurname.Text = "";
                                    txtMiddleName.Text = "";
                                }

                                txtEmail.Text = email;
                                txtContactNumber.Text = contactNumber;

                                string displayFirstName = txtFirstName.Text.Trim();
                                if (string.IsNullOrEmpty(displayFirstName)) displayFirstName = "Companion";

                                litUserFirstName.Text = Server.HtmlEncode(displayFirstName);

                                string initials = "C";
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
                                litProfileBigInitial.Text = initials;

                                if (!string.IsNullOrEmpty(profilePic))
                                {
                                    imgProfilePreview.ImageUrl = profilePic;
                                    imgProfilePreview.Visible = true;
                                    divFallbackInitial.Visible = false;

                                    imgTopAvatar.ImageUrl = profilePic;
                                    imgTopAvatar.Visible = true;
                                    litAvatarInitial.Visible = false;
                                }
                                else
                                {
                                    imgProfilePreview.Visible = false;
                                    divFallbackInitial.Visible = true;

                                    imgTopAvatar.Visible = false;
                                    litAvatarInitial.Visible = true;
                                }
                            }
                            else
                            {
                                ShowMessage("Warning: No user record found in the database.", "error");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Load companion profile error: " + ex);
                ShowMessage("We could not load your profile. Please try again.", "error");
            }
        }

        // Loads the "About You" text (CompanionProfiles.Bio) into its box. A separate query, because the bio
        // is stored on the companion's profile row and not in the Users table.
        private void LoadBio(int userId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = new SqlCommand("SELECT Bio FROM CompanionProfiles WHERE UserID = @UserID", connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    connection.Open();
                    object bio = command.ExecuteScalar();
                    txtBio.Text = bio == null || bio == DBNull.Value ? string.Empty : bio.ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Load companion bio error: " + ex);
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

        // Saves the profile. If a new picture was chosen it is validated (JPG/PNG only), saved to
        // ~/Uploads/Profiles with a unique name, and its path stored in Users.ProfilePicture.
        // When no new picture is chosen, the existing one is left untouched.
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                int userId = Convert.ToInt32(Session["UserID"]);

                string firstName = txtFirstName.Text.Trim();
                string middleName = txtMiddleName.Text.Trim();
                string surname = txtSurname.Text.Trim();

                // Validate everything on the server first (required fields, valid email that no other account
                // uses, PH mobile number, and a real JPG/PNG photo under 2 MB) before saving a file or touching the database.
                string validationError = ProfileValidator.ValidateProfile(firstName, middleName, surname, txtEmail.Text, txtContactNumber.Text, userId);
                if (validationError == null)
                {
                    // The "About You" text is optional, but limited in length
                    validationError = ProfileValidator.ValidateBio(txtBio.Text);
                }
                if (validationError == null && fileUploadAvatar.HasFile)
                {
                    validationError = ProfileValidator.ValidatePhoto(fileUploadAvatar.PostedFile);
                }
                if (validationError != null)
                {
                    ShowMessage(validationError, "error");
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
                        // Server-generated file name (user ID + timestamp) so the uploaded file name can never be used for path tricks
                        string fileName = "Companion_" + userId + "_" + DateTime.Now.Ticks + ext;
                        string folderPath = Server.MapPath("~/Uploads/Profiles/");

                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        string fullPath = folderPath + fileName;
                        fileUploadAvatar.SaveAs(fullPath);
                        dbPicPath = "~/Uploads/Profiles/" + fileName;
                    }
                    else
                    {
                        ShowMessage("Invalid file format. Please upload a JPG or PNG image.", "error");
                        return;
                    }
                }

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    // The account details (Users) and the "About You" text (CompanionProfiles) are saved together:
                    // either both are saved or neither is.
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        // Only include ProfilePicture in the UPDATE when a new picture was uploaded
                        string query = dbPicPath != null
                            ? "UPDATE Users SET FullName = @FullName, Email = @Email, ContactNumber = @ContactNumber, ProfilePicture = @ProfilePicture WHERE UserID = @UserID"
                            : "UPDATE Users SET FullName = @FullName, Email = @Email, ContactNumber = @ContactNumber WHERE UserID = @UserID";

                        using (SqlCommand command = new SqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@FullName", combinedFullName);
                            command.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                            command.Parameters.AddWithValue("@ContactNumber", string.IsNullOrEmpty(txtContactNumber.Text) ? (object)DBNull.Value : txtContactNumber.Text.Trim());
                            if (dbPicPath != null)
                            {
                                command.Parameters.AddWithValue("@ProfilePicture", dbPicPath);
                            }
                            command.Parameters.AddWithValue("@UserID", userId);
                            command.ExecuteNonQuery();
                        }

                        // Empty text is saved as NULL, which makes the public profile show its "no introduction yet" message
                        using (SqlCommand bioCommand = new SqlCommand("UPDATE CompanionProfiles SET Bio = @Bio WHERE UserID = @UserID", connection, transaction))
                        {
                            string bio = ProfileValidator.NormalizeBio(txtBio.Text);
                            bioCommand.Parameters.AddWithValue("@Bio", bio == null ? (object)DBNull.Value : bio);
                            bioCommand.Parameters.AddWithValue("@UserID", userId);
                            bioCommand.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }

                ShowMessage("Profile successfully updated!", "success");
                LoadUserProfile(userId);
                LoadBio(userId);
                // Fill the shared profile dropdown in the top bar (name, email, photo)
                CompanionUi.FillAccountMenu(this, userId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Update companion profile error: " + ex);
                ShowMessage("We could not update your profile. Please try again.", "error");
            }
        }

        // Shows the green/red banner. The text is HTML-encoded before it is rendered.
        private void ShowMessage(string message, string type)
        {
            lblMessage.Text = HttpUtility.HtmlEncode(message);
            lblMessage.CssClass = type == "success" ? "message-banner message-success" : "message-banner message-error";
            lblMessage.Visible = true;
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