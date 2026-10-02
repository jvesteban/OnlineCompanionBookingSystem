using System;
using System.Configuration;
using System.Data.SqlClient;

namespace OnlineCompanionBookingSystem
{
    /// <summary>
    /// Lahat ng database work para sa pagbubukas ng account (Customer o Companion) pagkatapos ng payment.
    /// Magkahiwalay ito sa mga page para iisa lang ang lugar ng registration logic.
    /// </summary>
    public static class RegistrationService
    {
        // Halaga ng one-time registration fee (PHP); ito ang ipinapakita sa checkout at sine-save sa Payments
        public const decimal RegistrationFee = 50m;

        // Allowed "Standard Rate per Hour" (whole pesos) when a companion registers. If you change these, also change
        // the RangeValidator (rvRate) and the min/max of the rate box in RegisterCompanion.aspx.
        public const int MinHourlyRate = 50;
        public const int MaxHourlyRate = 10000;

        // Mga payment method na ipinapakita sa checkout (simulation lang; walang totoong singil)
        public static readonly string[] PaymentMethods = { "GCash", "Maya", "Credit / Debit Card", "Online Banking" };

        private static string ConnStr => ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;

        // Reference number ng payment, hal. "OCB-20260930-B7CE31" (petsa + 6 random na character)
        public static string NewReferenceNo()
        {
            return "OCB-" + DateTime.Now.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant();
        }

        // Totoo kung may account na gumagamit ng email na ito (para hindi magkaroon ng dobleng account)
        public static bool EmailExists(string email)
        {
            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email = @Email", conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                conn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        // Ginagawa ang Payments table kung wala pa (at idinadagdag ang mga refund column), para hindi na kailangan
        // ng manual SQL script. Nasa PaymentService na ang mismong code para iisa lang ang lugar nito.
        private static void EnsurePaymentsTable(SqlConnection conn)
        {
            PaymentService.EnsureSchema(conn);
        }

        // Ginagawa ang account (at profile/activities/package kung Companion) at itinatala ang payment sa iisang transaction.
        public static int CompleteRegistration(PendingRegistration p, string method)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                EnsurePaymentsTable(conn);

                // Lahat ng insert sa ibaba ay nasa iisang transaction: kung may pumalya, walang matitirang kalahating account
                using (var tx = conn.BeginTransaction())
                {
                    // Tingnan ulit ang email dito (hindi lang sa registration page) kung sakaling may nauna habang nagbabayad
                    using (var check = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email = @Email", conn, tx))
                    {
                        check.Parameters.AddWithValue("@Email", p.Email);
                        if ((int)check.ExecuteScalar() > 0)
                            throw new InvalidOperationException("EmailExists");
                    }

                    int userId;
                    const string insertUser = @"
                        INSERT INTO Users (FullName, Email, PasswordHash, ContactNumber, Role, DateCreated, IsActive)
                        OUTPUT INSERTED.UserID
                        VALUES (@FullName, @Email, @PasswordHash, @Contact, @Role, GETDATE(), 1)";
                    using (var cmd = new SqlCommand(insertUser, conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@FullName", p.FullName);
                        cmd.Parameters.AddWithValue("@Email", p.Email);
                        cmd.Parameters.AddWithValue("@PasswordHash", p.PasswordHash);
                        cmd.Parameters.AddWithValue("@Contact", p.Contact);
                        cmd.Parameters.AddWithValue("@Role", p.Role);
                        userId = (int)cmd.ExecuteScalar();
                    }

                    // Para sa Companion lang: profile (Pending verification), activities, at default na hourly package
                    if (p.Role == "Companion")
                    {
                        int companionId;
                        const string insertProfile = @"
                            INSERT INTO CompanionProfiles (UserID, VerificationDocPath, VerificationStatus, Bio)
                            OUTPUT INSERTED.CompanionID
                            VALUES (@UserID, @DocPath, 'Pending', @Bio)";
                        using (var cmd = new SqlCommand(insertProfile, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@UserID", userId);
                            cmd.Parameters.AddWithValue("@DocPath", p.DocPath);
                            cmd.Parameters.AddWithValue("@Bio", string.IsNullOrWhiteSpace(p.Bio) ? (object)DBNull.Value : p.Bio);
                            companionId = (int)cmd.ExecuteScalar();
                        }

                        foreach (string activity in p.Activities)
                        {
                            using (var cmd = new SqlCommand("INSERT INTO Activities (CompanionID, ActivityName) VALUES (@CompanionID, @ActivityName)", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@CompanionID", companionId);
                                cmd.Parameters.AddWithValue("@ActivityName", activity);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        const string insertPackage = @"
                            INSERT INTO Packages (CompanionID, PackageName, Duration, Rate, Description)
                            VALUES (@CompanionID, 'Standard Hourly Rate', '1 Hour', @Rate, 'Base hourly companion service rate.')";
                        using (var cmd = new SqlCommand(insertPackage, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@CompanionID", companionId);
                            cmd.Parameters.AddWithValue("@Rate", p.HourlyRate);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Itala ang payment (IsSimulated = 1 dahil demo lang ito)
                    const string insertPayment = @"
                        INSERT INTO Payments (UserID, ReferenceNo, Purpose, Amount, Method, Status, DatePaid, IsSimulated)
                        VALUES (@UserID, @Ref, @Purpose, @Amount, @Method, 'Paid', GETDATE(), 1)";
                    using (var cmd = new SqlCommand(insertPayment, conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@Ref", p.ReferenceNo);
                        cmd.Parameters.AddWithValue("@Purpose", p.Role + " Registration Fee");
                        cmd.Parameters.AddWithValue("@Amount", p.Fee);
                        cmd.Parameters.AddWithValue("@Method", method);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                    return userId;
                }
            }
        }
    }
}
