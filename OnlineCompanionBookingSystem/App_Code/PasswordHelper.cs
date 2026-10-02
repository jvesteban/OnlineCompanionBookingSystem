using System;
using System.Security.Cryptography;
using System.Text;

namespace OnlineCompanionBookingSystem
{
    /// <summary>
    /// Password hashing para sa registration at login.
    /// Note: SHA-256 na walang salt ito, sapat para sa demo/capstone. Sa production, gumamit ng
    /// PBKDF2, bcrypt, o Argon2 (may salt at mabagal) para mahirap i-crack kapag nakuha ang database.
    /// </summary>
    public static class PasswordHelper
    {
        // Ginagawang 64-character hex string ang password; ito ang sine-save sa Users.PasswordHash
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Password -> UTF-8 bytes -> SHA-256 hash (32 bytes)
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    // "x2" = bawat byte ay 2-digit lowercase hex
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Totoo kung tugma ang ininput na password sa naka-save na hash
        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            // I-hash ang ininput na password para i-compare sa nakaimbak sa database
            string hashedEnteredPassword = HashPassword(enteredPassword);

            // Suriin kung pareho sila (case-insensitive o standard string comparison)
            return string.Equals(hashedEnteredPassword, storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}