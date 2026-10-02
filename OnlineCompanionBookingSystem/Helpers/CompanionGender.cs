using System;
using System.Data.SqlClient;

namespace OnlineCompanionBookingSystem
{
    /// <summary>
    /// The gender a companion states when registering (column CompanionProfiles.Gender).
    /// The platform currently accepts female companions only (AllowedGender), and the administrator checks the stated
    /// gender against the valid ID before approving. Companions registered before this field existed have no value (NULL).
    /// </summary>
    public static class CompanionGender
    {
        // The only gender accepted for new companion registrations. Change it here if the rule ever changes.
        public const string AllowedGender = "Female";

        // The choices shown on the registration form
        public static readonly string[] Options = { "Female", "Male", "Other" };

        public static bool IsValidOption(string value)
        {
            return Array.IndexOf(Options, value) >= 0;
        }

        // Adds the Gender column when it is missing, so the pages work even before Database/add_gender_column.sql was run.
        // Safe to call every time: nothing happens when the column already exists.
        public static void EnsureColumn(SqlConnection connection)
        {
            const string sql = @"
                IF COL_LENGTH('dbo.CompanionProfiles', 'Gender') IS NULL
                    ALTER TABLE dbo.CompanionProfiles ADD Gender NVARCHAR(20) NULL;";
            using (var command = new SqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        // Text for admin screens: "Female", or a note for companions who registered before the field existed
        public static string Display(object value)
        {
            string text = value == null || value == DBNull.Value ? string.Empty : Convert.ToString(value).Trim();
            return text.Length == 0 ? "Not provided" : text;
        }
    }
}
