using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;

namespace OnlineCompanionBookingSystem
{
    /// <summary>
    /// Server-side validation for the "My Profile" pages (Customer and Companion), so both follow the same
    /// rules as registration. Browser checks can always be bypassed, so these checks run on the server.
    /// Every method returns null when the value is fine, or a user-friendly message when it is not.
    /// </summary>
    public static class ProfileValidator
    {
        public const int MaxNameLength = 50;
        public const int MaxEmailLength = 100;
        public const long MaxPhotoBytes = 2L * 1024 * 1024; // 2 MB (profile photo)
        public const long MaxIdImageBytes = 5L * 1024 * 1024; // 5 MB (ID photo from a phone camera is usually bigger)

        // Same format as registration: 11-digit Philippine mobile number starting with 09
        private static readonly Regex PhMobile = new Regex(@"^09\d{9}$", RegexOptions.Compiled);

        // Checks name, email, and contact number. userId is the logged-in user, so their own current
        // email is not treated as a duplicate.
        public static string ValidateProfile(string firstName, string middleName, string surname, string email, string contact, int userId)
        {
            firstName = (firstName ?? string.Empty).Trim();
            middleName = (middleName ?? string.Empty).Trim();
            surname = (surname ?? string.Empty).Trim();
            email = (email ?? string.Empty).Trim();
            contact = (contact ?? string.Empty).Trim();

            if (firstName.Length == 0) return "First name is required.";
            if (surname.Length == 0) return "Surname is required.";

            string nameError = CheckName(firstName, "First name") ?? CheckName(middleName, "Middle name") ?? CheckName(surname, "Surname");
            if (nameError != null) return nameError;

            if (email.Length == 0) return "Email address is required.";
            if (email.Length > MaxEmailLength) return "Email address is too long.";
            if (!IsValidEmail(email)) return "Please enter a valid email address.";

            if (contact.Length == 0) return "Contact number is required.";
            if (!PhMobile.IsMatch(contact)) return "Contact number must be an 11-digit PH mobile number (09XXXXXXXXX).";

            if (EmailUsedByAnotherUser(email, userId)) return "This email address is already used by another account.";

            return null;
        }

        public const int MaxBioLength = 500;

        // The "About" text a companion writes about themselves (shown on their public profile). It is optional.
        // Line breaks are counted as one character each, the same way the browser counts them.
        public static string ValidateBio(string bio)
        {
            string normalized = NormalizeBio(bio);
            if (normalized != null && normalized.Length > MaxBioLength)
                return "The About section is too long (maximum " + MaxBioLength + " characters).";
            return null;
        }

        // Cleans the bio before saving: one kind of line break, no spaces around it, and null (not "") when it is empty.
        public static string NormalizeBio(string bio)
        {
            if (bio == null) return null;
            string cleaned = bio.Replace("\r\n", "\n").Replace('\r', '\n').Trim();
            return cleaned.Length == 0 ? null : cleaned;
        }

        // Profile photo: JPG or PNG, at most 2 MB, and the file contents must really be that image type
        // (the first bytes are checked, so a renamed .exe or .html file is rejected).
        public static string ValidatePhoto(HttpPostedFile file)
        {
            return ValidateImageFile(file, MaxPhotoBytes, "photo");
        }

        // Same checks for any uploaded image. "what" is how the file is called in the messages ("photo", "ID image"),
        // and maxBytes is the size limit (the message shows it in MB).
        public static string ValidateImageFile(HttpPostedFile file, long maxBytes, string what)
        {
            if (file == null || file.ContentLength == 0) return "The selected " + what + " is empty.";

            string ext = Path.GetExtension(file.FileName ?? string.Empty).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png") return "Invalid file format. Please upload a JPG or PNG image.";
            if (file.ContentLength > maxBytes) return "The " + what + " is too large. Please choose an image under " + (maxBytes / (1024 * 1024)) + " MB.";

            byte[] header = new byte[8];
            int read = file.InputStream.Read(header, 0, header.Length);
            file.InputStream.Position = 0; // rewind so the file can still be saved afterwards

            bool isJpeg = read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
            bool isPng = read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
                         && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;

            if ((ext == ".png" && !isPng) || ((ext == ".jpg" || ext == ".jpeg") && !isJpeg))
                return "The file is not a valid image. Please upload a real JPG or PNG " + what + ".";

            return null;
        }

        // Names may not contain "<" or ">" (stops markup being typed into a name) and have a length limit.
        private static string CheckName(string value, string label)
        {
            if (value.Length > MaxNameLength) return label + " is too long (maximum " + MaxNameLength + " characters).";
            if (value.IndexOfAny(new[] { '<', '>' }) >= 0) return label + " contains invalid characters.";
            return null;
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                // MailAddress also accepts "Name <a@b.com>"; requiring the address to equal the input rules that out.
                var address = new MailAddress(email);
                return address.Address == email && address.Host.Contains(".");
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static bool EmailUsedByAnotherUser(string email, int userId)
        {
            string connStr = ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;
            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email = @Email AND UserID <> @UserID", conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@UserID", userId);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
    }
}
