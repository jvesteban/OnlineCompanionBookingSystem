using System;
using System.Configuration;
using System.Data.SqlClient;

namespace OnlineCompanionBookingSystem
{
    /// <summary>The result of a refund attempt (see PaymentService.RefundCompanionRegistration).</summary>
    [Serializable]
    public class RefundResult
    {
        public bool Refunded { get; set; }            // false when there was nothing to refund
        public decimal Amount { get; set; }
        public string Method { get; set; }            // where the money goes back to, e.g. "GCash"
        public string PaymentReference { get; set; }  // reference number of the original payment
        public string RefundReference { get; set; }   // reference number of this refund, e.g. OCB-RF-20261001-A1B2C3
        public DateTime DateRefunded { get; set; }
    }

    /// <summary>
    /// Everything about the Payments table that is not part of registering: creating/updating the table and refunds.
    /// Like the registration payment, the refund is SIMULATED (it only records the refund in the database and tells
    /// the user). With a real payment gateway (e.g. PayMongo, Xendit, Maya Business) this is the place that would
    /// call the gateway's refund API instead.
    /// </summary>
    public static class PaymentService
    {
        // Creates the Payments table when it does not exist, and adds the refund columns to a table that was
        // created before refunds existed. Safe to run any number of times.
        public static void EnsureSchema(SqlConnection conn, SqlTransaction tx = null)
        {
            const string createTable = @"
                IF OBJECT_ID(N'dbo.Payments', N'U') IS NULL
                CREATE TABLE dbo.Payments (
                    PaymentID       INT IDENTITY(1,1) PRIMARY KEY,
                    UserID          INT NOT NULL CONSTRAINT FK_Payments_Users REFERENCES dbo.Users(UserID) ON DELETE CASCADE,
                    ReferenceNo     VARCHAR(40) NOT NULL CONSTRAINT UQ_Payments_ReferenceNo UNIQUE,
                    Purpose         VARCHAR(60) NOT NULL,
                    Amount          DECIMAL(10,2) NOT NULL,
                    Method          VARCHAR(30) NOT NULL,
                    Status          VARCHAR(20) NOT NULL,   -- 'Paid' or 'Refunded'
                    DatePaid        DATETIME NOT NULL CONSTRAINT DF_Payments_DatePaid DEFAULT GETDATE(),
                    IsSimulated     BIT NOT NULL CONSTRAINT DF_Payments_IsSimulated DEFAULT 1,
                    RefundedAt      DATETIME NULL,
                    RefundReference VARCHAR(40) NULL
                );";

            // For a Payments table created earlier (before refunds): add the two new columns
            const string addRefundColumns = @"
                IF COL_LENGTH(N'dbo.Payments', N'RefundedAt') IS NULL
                    ALTER TABLE dbo.Payments ADD RefundedAt DATETIME NULL;
                IF COL_LENGTH(N'dbo.Payments', N'RefundReference') IS NULL
                    ALTER TABLE dbo.Payments ADD RefundReference VARCHAR(40) NULL;";

            using (var cmd = new SqlCommand(createTable, conn, tx)) cmd.ExecuteNonQuery();
            using (var cmd = new SqlCommand(addRefundColumns, conn, tx)) cmd.ExecuteNonQuery();
        }

        // Reference number of a refund, e.g. "OCB-RF-20261001-A1B2C3"
        public static string NewRefundReference()
        {
            return "OCB-RF-" + DateTime.Now.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant();
        }

        // Called when the admin REJECTS a companion application: the registration fee is returned to the same payment
        // method it was paid with (GCash, Maya, ...). Only a payment that is still "Paid" is refunded, so calling this
        // twice can never refund twice. Returns Refunded = false when there was nothing to refund
        // (for example an account created before payments existed). Never throws: a refund problem must not
        // stop the admin's decision from being saved.
        public static RefundResult RefundCompanionRegistration(int companionId)
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    EnsureSchema(conn);
                    return RefundCore(conn, null, companionId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("RefundCompanionRegistration error: " + ex);
                return new RefundResult { Refunded = false };
            }
        }

        // When a previously rejected application is approved again, the demo payment
        // record returns to Paid so the admin verification card reflects the new decision.
        public static void RestoreCompanionRegistrationPayment(int companionId)
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    EnsureSchema(conn);

                    const string restoreSql = @"
                        UPDATE p
                        SET Status = 'Paid',
                            RefundedAt = NULL,
                            RefundReference = NULL
                        FROM Payments p
                        INNER JOIN CompanionProfiles cp ON cp.UserID = p.UserID
                        WHERE cp.CompanionID = @CompanionID
                          AND p.PaymentID = (
                              SELECT TOP 1 p2.PaymentID
                              FROM Payments p2
                              WHERE p2.UserID = p.UserID
                                AND p2.Status = 'Refunded'
                                AND p2.Purpose LIKE 'Companion%'
                              ORDER BY p2.PaymentID DESC
                          )";

                    using (var cmd = new SqlCommand(restoreSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CompanionID", companionId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // A payment restoration failure must not hide the verification result.
                System.Diagnostics.Debug.WriteLine("RestoreCompanionRegistrationPayment error: " + ex);
            }
        }

        // Same as RefundCompanionRegistration, but on a connection/transaction the caller already has (used by tests).
        public static RefundResult RefundCore(SqlConnection conn, SqlTransaction tx, int companionId)
        {
            // The newest companion registration payment of this companion that is still "Paid".
            // The NOT EXISTS part is a second safety net: once a companion's fee has been refunded, nothing more
            // is ever refunded to that account, even if another "Paid" row somehow exists.
            const string findSql = @"
                SELECT TOP 1 p.PaymentID, p.Amount, p.Method, p.ReferenceNo
                FROM Payments p
                INNER JOIN CompanionProfiles cp ON cp.UserID = p.UserID
                WHERE cp.CompanionID = @CompanionID AND p.Status = 'Paid' AND p.Purpose LIKE 'Companion%'
                  AND NOT EXISTS (SELECT 1 FROM Payments r
                                  WHERE r.UserID = p.UserID AND r.Status = 'Refunded' AND r.Purpose LIKE 'Companion%')
                ORDER BY p.PaymentID DESC";

            int paymentId;
            var result = new RefundResult();
            using (var cmd = new SqlCommand(findSql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@CompanionID", companionId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return result; // nothing to refund
                    paymentId = Convert.ToInt32(reader["PaymentID"]);
                    result.Amount = Convert.ToDecimal(reader["Amount"]);
                    result.Method = reader["Method"].ToString();
                    result.PaymentReference = reader["ReferenceNo"].ToString();
                }
            }

            string refundReference = NewRefundReference();
            // "AND Status = 'Paid'" makes this atomic: if two admins click at the same moment, only one update succeeds.
            const string updateSql = @"
                UPDATE Payments
                SET Status = 'Refunded', RefundedAt = GETDATE(), RefundReference = @RefundRef
                WHERE PaymentID = @PaymentID AND Status = 'Paid'";
            using (var cmd = new SqlCommand(updateSql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@RefundRef", refundReference);
                cmd.Parameters.AddWithValue("@PaymentID", paymentId);
                if (cmd.ExecuteNonQuery() != 1) return new RefundResult(); // someone else refunded it first
            }

            result.Refunded = true;
            result.RefundReference = refundReference;
            result.DateRefunded = DateTime.Now;
            return result;
        }
    }
}
