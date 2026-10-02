using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace OnlineCompanionBookingSystem
{
    /// <summary>
    /// Lahat ng email ng system (welcome, application received, approved/rejected/revoked, payment receipt).
    /// Ang SMTP settings (Smtp:*) ay nasa Web.config. Hindi nagta-throw ang mga method dito, para hindi
    /// mapigilan ang registration o ang pag-approve ng admin kapag pumalya ang email.
    /// </summary>
    public static class EmailHelper
    {
        // Pangunahing method na nagpapadala ng HTML email. Ang lahat ng iba pang method dito ay dumadaan dito.
        // Nagbabalik ng true kung nagpadala; false (at hindi nagta-throw) kung pumalya o kulang ang SMTP settings.
        public static bool TrySend(string toEmail, string subject, string htmlBody)
        {
            try
            {
                // Kunin ang SMTP settings mula sa Web.config (appSettings)
                string host = ConfigurationManager.AppSettings["Smtp:Host"];
                string user = ConfigurationManager.AppSettings["Smtp:User"];
                string pass = ConfigurationManager.AppSettings["Smtp:Password"];
                string from = ConfigurationManager.AppSettings["Smtp:From"];
                string fromName = ConfigurationManager.AppSettings["Smtp:FromName"] ?? "Online Companion Booking Management System";
                int port = int.Parse(ConfigurationManager.AppSettings["Smtp:Port"] ?? "587");
                bool ssl = !string.Equals(ConfigurationManager.AppSettings["Smtp:EnableSsl"], "false", StringComparison.OrdinalIgnoreCase);

                // Huwag subukang magpadala kung hindi pa napupunan ang placeholder na settings (YOUR_...)
                if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) ||
                    string.IsNullOrWhiteSpace(pass) || user.StartsWith("YOUR_"))
                {
                    System.Diagnostics.Debug.WriteLine("EmailHelper: SMTP settings are not configured in Web.config.");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(from)) from = user;

                // Kailangan ng Gmail at karamihan ng SMTP server ang TLS 1.2 (hindi default sa lumang .NET Framework)
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(from, fromName);
                    message.To.Add(new MailAddress(toEmail));
                    message.Subject = subject;
                    message.Body = htmlBody;
                    message.IsBodyHtml = true;

                    using (var client = new SmtpClient(host, port))
                    {
                        client.EnableSsl = ssl;
                        client.Credentials = new NetworkCredential(user, pass);
                        client.Send(message);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EmailHelper error: " + ex);
                return false;
            }
        }

        // Ipinapadala sa bagong Customer pagkatapos ng bayad. Naka-HTML-encode ang pangalan para hindi ma-inject ang HTML.
        public static bool SendCustomerWelcome(string toEmail, string fullName)
        {
            string name = HttpUtility.HtmlEncode(fullName);
            string content = $@"
<p>Hello <strong>{name}</strong>,</p>
<p>Thank you for registering as a customer. Your account has been created and you can now log in to browse verified companions and book your next activity.</p>
<p style='font-size:13px;color:#888'>If you did not create this account, please ignore this email or contact our support team.</p>";
            return TrySend(toEmail, "Welcome to OCBMS - Registration Successful",
                Layout("Welcome aboard!", content, "Log In to Your Account", LoginUrl()));
        }

        // Resibo pagkatapos ng registration payment (simulation; walang totoong singil)
        public static bool SendPaymentReceipt(PaymentReceipt r)
        {
            string name = HttpUtility.HtmlEncode(r.FullName);
            string purpose = r.Role == "Companion" ? "Companion Registration Fee" : "Customer Registration Fee";
            string row = "<tr><td style='padding:8px 0;color:#64748b'>{0}</td><td style='padding:8px 0;text-align:right;font-weight:600;color:#0f172a'>{1}</td></tr>";
            string content = $@"
<p>Hello <strong>{name}</strong>,</p>
<p>We have received your payment. Here is your receipt:</p>
<table style='width:100%;border-collapse:collapse;border-top:1px solid #e3e7ed;border-bottom:1px solid #e3e7ed;margin:12px 0'>
  {string.Format(row, "Reference No.", HttpUtility.HtmlEncode(r.ReferenceNo))}
  {string.Format(row, "Description", purpose)}
  {string.Format(row, "Payment Method", HttpUtility.HtmlEncode(r.Method))}
  {string.Format(row, "Date", r.DatePaid.ToString("MMMM dd, yyyy h:mm tt"))}
  {string.Format(row, "Amount Paid", "&#8369;" + r.Amount.ToString("N2"))}
</table>
<p style='font-size:12px;color:#888'>This is a demo transaction for system testing. No actual charge was made.</p>";
            return TrySend(r.Email, "OCBMS - Payment Receipt " + r.ReferenceNo, Layout("Payment Receipt", content));
        }

        // Ipinapadala kapag natapos ang companion registration (Pending Admin Verification)
        public static bool SendCompanionApplicationReceived(string toEmail, string fullName)
        {
            string name = HttpUtility.HtmlEncode(fullName);
            string content = $@"
<p>Hello <strong>{name}</strong>,</p>
<p>Thank you for applying to become a companion. We have received your registration and verification document.</p>
<p>Your application is now <strong style='color:#b7791f'>Pending Verification</strong>. Our admin team will review your ID and you will receive another email once a decision has been made.</p>
<p style='font-size:13px;color:#666'>You will be able to receive bookings after your account is verified.</p>
<p style='font-size:13px;color:#888'>If you did not submit this application, please contact our support team.</p>";
            return TrySend(toEmail, "OCBMS - Companion Application Received",
                Layout("Application Received", content));
        }

        // Ipinapadala kapag nag-approve o nag-reject ang admin.
        // Kapag rejected at may refund, kasama sa email ang detalye ng ibinalik na bayad.
        public static bool SendCompanionVerificationResult(string toEmail, string fullName, bool approved, RefundResult refund = null, string reason = null)
        {
            string name = HttpUtility.HtmlEncode(fullName);
            if (approved)
            {
                string content = $@"
<p>Hello <strong>{name}</strong>,</p>
<p>Great news! Your companion application has been <strong style='color:#28a745'>approved</strong>. Your account is now verified and visible to customers.</p>
{ReasonBox("Reason for approval", string.IsNullOrWhiteSpace(reason) ? "Your submitted valid ID was reviewed and your details matched your registration." : reason)}
<p>You can log in to complete your profile, set your availability, and start receiving booking requests.</p>";
                return TrySend(toEmail, "OCBMS - Your Companion Account Has Been Approved",
                    Layout("Application Approved", content, "Log In to Your Account", LoginUrl()));
            }
            else
            {
                // Refund section: only when the registration fee was really returned
                string refundHtml = string.Empty;
                if (refund != null && refund.Refunded)
                {
                    string row = "<tr><td style='padding:8px 0;color:#64748b'>{0}</td><td style='padding:8px 0;text-align:right;font-weight:600;color:#0f172a'>{1}</td></tr>";
                    refundHtml = $@"
<p>Your registration fee of <strong>&#8369;{refund.Amount.ToString("N2")}</strong> has been <strong style='color:#28a745'>refunded</strong> to the same payment method you used ({HttpUtility.HtmlEncode(refund.Method)}).</p>
<table style='width:100%;border-collapse:collapse;border-top:1px solid #e3e7ed;border-bottom:1px solid #e3e7ed;margin:12px 0'>
  {string.Format(row, "Refund Reference", HttpUtility.HtmlEncode(refund.RefundReference))}
  {string.Format(row, "Original Payment", HttpUtility.HtmlEncode(refund.PaymentReference))}
  {string.Format(row, "Refunded To", HttpUtility.HtmlEncode(refund.Method))}
  {string.Format(row, "Date", refund.DateRefunded.ToString("MMMM dd, yyyy h:mm tt"))}
  {string.Format(row, "Amount Refunded", "&#8369;" + refund.Amount.ToString("N2"))}
</table>
<p style='font-size:12px;color:#888'>This is a demo transaction for system testing. No actual money was moved.</p>";
                }

                string content = $@"
<p>Hello <strong>{name}</strong>,</p>
<p>Thank you for your interest in becoming a companion. After reviewing your application, we are unable to approve it at this time.</p>
{ReasonBox("Reason for this decision", string.IsNullOrWhiteSpace(reason) ? "Your application did not meet our verification requirements." : reason)}
<p>If you believe this decision was a mistake, please contact <a href='mailto:support@ocbs.com'>support@ocbs.com</a> and our team will be glad to assist you.</p>
{refundHtml}";
                return TrySend(toEmail, refund != null && refund.Refunded
                        ? "OCBMS - Application Update and Refund " + refund.RefundReference
                        : "OCBMS - Update on Your Companion Application",
                    Layout("Application Update", content));
            }
        }

        // Hahanapin ang email/pangalan ng companion gamit ang CompanionID at ipapadala ang resulta
        public static bool SendCompanionVerificationResultById(int companionId, bool approved, RefundResult refund = null, string reason = null)
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;
                using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
                using (var cmd = new System.Data.SqlClient.SqlCommand(
                    "SELECT u.Email, u.FullName FROM CompanionProfiles cp INNER JOIN Users u ON cp.UserID = u.UserID WHERE cp.CompanionID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", companionId);
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return false;
                        return SendCompanionVerificationResult(r["Email"].ToString(), r["FullName"].ToString(), approved, refund, reason);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EmailHelper lookup error: " + ex);
                return false;
            }
        }

        // Ipinapadala kapag binawi (revoke) ng admin ang verification; bumabalik ang status sa Pending
        public static bool SendCompanionVerificationRevoked(string toEmail, string fullName, string reason = null)
        {
            string name = HttpUtility.HtmlEncode(fullName);
            string content = $@"
<p>Hello <strong>{name}</strong>,</p>
<p>Your companion verification has been <strong style='color:#b7791f'>revoked</strong> by our admin team, and your account is now back to <strong>Pending Verification</strong>.</p>
{ReasonBox("Reason for this action", string.IsNullOrWhiteSpace(reason) ? "Your account no longer meets our verification requirements." : reason)}
<p>Your profile will not be visible to customers, and you will not be able to receive new bookings until your account is verified again.</p>
<p>If you have questions or believe this was a mistake, please contact <a href='mailto:support@ocbs.com'>support@ocbs.com</a>.</p>";
            return TrySend(toEmail, "OCBMS - Your Companion Verification Has Been Revoked",
                Layout("Verification Revoked", content));
        }

        public static bool SendCompanionVerificationRevokedById(int companionId, string reason = null)
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;
                using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
                using (var cmd = new System.Data.SqlClient.SqlCommand(
                    "SELECT u.Email, u.FullName FROM CompanionProfiles cp INNER JOIN Users u ON cp.UserID = u.UserID WHERE cp.CompanionID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", companionId);
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return false;
                        return SendCompanionVerificationRevoked(r["Email"].ToString(), r["FullName"].ToString(), reason);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EmailHelper lookup error: " + ex);
                return false;
            }
        }

        // The "reason" block shown in the verification emails. The reason is typed by an admin, so it is HTML-encoded.
        private static string ReasonBox(string label, string reason)
        {
            return "<div style='background:#f8f9fb;border-left:4px solid #007bff;border-radius:4px;padding:12px 16px;margin:16px 0'>" +
                   "<div style='font-size:12px;font-weight:600;color:#64748b;text-transform:uppercase;letter-spacing:.5px;margin-bottom:4px'>" + HttpUtility.HtmlEncode(label) + "</div>" +
                   "<div style='color:#1a1a1a'>" + HttpUtility.HtmlEncode(reason) + "</div></div>";
        }

        // ===== Booking emails =====

        // Everything the booking emails need, read from the database in one go
        private class BookingMail
        {
            public string CustomerName, CustomerEmail, CompanionName, CompanionEmail, CompanionContact;
            public string PackageName, Activities, Status;
            public decimal Rate;
            public DateTime Start, End;
            public bool HasSchedule;
        }

        private static BookingMail LoadBooking(int bookingId)
        {
            string connStr = ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;
            using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
            {
                conn.Open();
                BookingActivities.EnsureSchema(conn);   // the query below reads the BookingActivities table

                string sql = @"
                    SELECT b.BookingDate, b.BookingTime, b.Status, p.PackageName, p.Duration, p.Rate,
                           cu.FullName AS CustomerName, cu.Email AS CustomerEmail,
                           co.FullName AS CompanionName, co.Email AS CompanionEmail, co.ContactNumber AS CompanionContact,
                           " + BookingActivities.JoinedActivitiesSql + @" AS Activities
                    FROM Bookings b
                    INNER JOIN Packages p ON p.PackageID = b.PackageID
                    INNER JOIN Users cu ON cu.UserID = b.CustomerID
                    INNER JOIN CompanionProfiles cp ON cp.CompanionID = b.CompanionID
                    INNER JOIN Users co ON co.UserID = cp.UserID
                    WHERE b.BookingID = @BookingID";
                using (var cmd = new System.Data.SqlClient.SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@BookingID", bookingId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return null;
                        var mail = new BookingMail
                        {
                            CustomerName = Convert.ToString(r["CustomerName"]),
                            CustomerEmail = Convert.ToString(r["CustomerEmail"]),
                            CompanionName = Convert.ToString(r["CompanionName"]),
                            CompanionEmail = Convert.ToString(r["CompanionEmail"]),
                            CompanionContact = r["CompanionContact"] == DBNull.Value ? "" : Convert.ToString(r["CompanionContact"]),
                            PackageName = Convert.ToString(r["PackageName"]),
                            Activities = r["Activities"] == DBNull.Value ? "" : Convert.ToString(r["Activities"]),
                            Status = Convert.ToString(r["Status"]),
                            Rate = r["Rate"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Rate"])
                        };
                        DateTime start, end;
                        mail.HasSchedule = BookingStatus.TryGetSchedule(r["BookingDate"], r["BookingTime"], r["Duration"], out start, out end);
                        mail.Start = start;
                        mail.End = end;
                        return mail;
                    }
                }
            }
        }

        // "Monday, October 05, 2026 at 12:00 PM"
        private static string LongDateTime(DateTime value)
        {
            return value.ToString("dddd, MMMM dd, yyyy", System.Globalization.CultureInfo.InvariantCulture) + " at " +
                   value.ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
        }

        // A small two-column table (label, value). Both columns are HTML-encoded; rows without a value are left out.
        private static string DetailsTable(params string[][] rows)
        {
            var sb = new System.Text.StringBuilder("<table style='width:100%;border-collapse:collapse;border-top:1px solid #e3e7ed;border-bottom:1px solid #e3e7ed;margin:14px 0'>");
            foreach (string[] row in rows)
            {
                if (row == null || string.IsNullOrWhiteSpace(row[1])) continue;
                sb.Append("<tr><td style='padding:8px 0;color:#64748b;vertical-align:top;width:34%'>").Append(HttpUtility.HtmlEncode(row[0]))
                  .Append("</td><td style='padding:8px 0;text-align:right;font-weight:600;color:#0f172a'>").Append(HttpUtility.HtmlEncode(row[1])).Append("</td></tr>");
            }
            return sb.Append("</table>").ToString();
        }

        // The booking details block shared by both emails
        private static string BookingDetails(BookingMail m, params string[][] extraRows)
        {
            var rows = new System.Collections.Generic.List<string[]>
            {
                new[] { "Package", m.PackageName },
                new[] { "Activities", m.Activities },
                new[] { "Starts", m.HasSchedule ? LongDateTime(m.Start) : "" },
                new[] { "Ends", m.HasSchedule ? LongDateTime(m.End) : "" },
                new[] { "Rate", "PHP " + m.Rate.ToString("N2") }
            };
            rows.AddRange(extraRows);
            return DetailsTable(rows.ToArray());
        }

        // Sent to the companion right after a customer books them, so they know even when they are logged out.
        public static bool SendBookingRequestToCompanion(int bookingId)
        {
            try
            {
                BookingMail m = LoadBooking(bookingId);
                if (m == null) return false;

                string content = $@"
<p>Hello <strong>{HttpUtility.HtmlEncode(m.CompanionName)}</strong>,</p>
<p><strong>{HttpUtility.HtmlEncode(m.CustomerName)}</strong> has sent you a new booking request. Here are the details:</p>
{BookingDetails(m, new[] { "Requested by", m.CustomerName })}
<p>Please review the request and accept or decline it. The customer is waiting for your response.</p>";
                return TrySend(m.CompanionEmail, "OCBMS - New Booking Request from " + m.CustomerName,
                    Layout("New Booking Request", content, "Review Booking Request", AppUrl("Companion/CompanionBookingRequest.aspx")));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EmailHelper booking request error: " + ex);
                return false;
            }
        }

        // Sent to the customer when the companion accepts or declines their request.
        // An accepted booking also shows the companion's contact details, for coordination outside the system.
        public static bool SendBookingDecisionToCustomer(int bookingId, bool accepted)
        {
            try
            {
                BookingMail m = LoadBooking(bookingId);
                if (m == null) return false;

                string customer = HttpUtility.HtmlEncode(m.CustomerName);
                string companion = HttpUtility.HtmlEncode(m.CompanionName);

                if (accepted)
                {
                    string content = $@"
<p>Hello <strong>{customer}</strong>,</p>
<p>Great news! <strong>{companion}</strong> has <strong style='color:#28a745'>accepted</strong> your booking request. Your booking is now confirmed.</p>
{BookingDetails(m, new[] { "Companion", m.CompanionName })}
<p><strong>Contact details for coordination</strong></p>
{DetailsTable(new[] { "Email", m.CompanionEmail }, new[] { "Contact number", m.CompanionContact })}
<p>Please use these details to coordinate the activity with your companion. Communication takes place outside the system.</p>";
                    return TrySend(m.CustomerEmail, "OCBMS - Your Booking Has Been Confirmed",
                        Layout("Booking Confirmed", content, "View My Bookings", AppUrl("Customer/CustomerMyBookings.aspx")));
                }
                else
                {
                    string content = $@"
<p>Hello <strong>{customer}</strong>,</p>
<p>Thank you for your booking request. Unfortunately, <strong>{companion}</strong> is unable to accept it and has <strong style='color:#b7791f'>declined</strong> the request.</p>
{BookingDetails(m, new[] { "Companion", m.CompanionName })}
<p>You are welcome to choose a different date or time, or browse other available companions.</p>";
                    return TrySend(m.CustomerEmail, "OCBMS - Update on Your Booking Request",
                        Layout("Booking Request Declined", content, "Browse Companions", AppUrl("Customer/BrowseCompanion.aspx")));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EmailHelper booking decision error: " + ex);
                return false;
            }
        }

        // Sent to the companion when the customer cancels a booking (a Pending request, or a Confirmed one that has not started).
        public static bool SendBookingCancelledByCustomer(int bookingId)
        {
            try
            {
                BookingMail m = LoadBooking(bookingId);
                if (m == null) return false;

                string content = $@"
<p>Hello <strong>{HttpUtility.HtmlEncode(m.CompanionName)}</strong>,</p>
<p><strong>{HttpUtility.HtmlEncode(m.CustomerName)}</strong> has <strong style='color:#b7791f'>cancelled</strong> their booking with you. Here are the details of the cancelled booking:</p>
{BookingDetails(m, new[] { "Cancelled by", m.CustomerName })}
<p>This time is now free again, and no further action is needed from you.</p>";
                return TrySend(m.CompanionEmail, "OCBMS - Booking Cancelled by " + m.CustomerName,
                    Layout("Booking Cancelled", content, "View Notifications", AppUrl("Companion/CompanionNotifications.aspx")));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EmailHelper cancellation (customer) error: " + ex);
                return false;
            }
        }

        // Sent to both the customer and the companion when an administrator cancels a booking.
        // Returns true only when both emails were sent.
        public static bool SendBookingCancelledByAdmin(int bookingId)
        {
            try
            {
                BookingMail m = LoadBooking(bookingId);
                if (m == null) return false;

                string note = "<p>If you have questions about this decision, please contact <a href='mailto:support@ocbs.com'>support@ocbs.com</a>.</p>";

                string toCustomer = $@"
<p>Hello <strong>{HttpUtility.HtmlEncode(m.CustomerName)}</strong>,</p>
<p>Your booking with <strong>{HttpUtility.HtmlEncode(m.CompanionName)}</strong> has been <strong style='color:#b7791f'>cancelled by the administrator</strong>. Here are the details of the cancelled booking:</p>
{BookingDetails(m, new[] { "Companion", m.CompanionName })}
{note}
<p>You are welcome to make a new booking at a different time, or with another companion.</p>";
                bool sentCustomer = TrySend(m.CustomerEmail, "OCBMS - Your Booking Has Been Cancelled",
                    Layout("Booking Cancelled", toCustomer, "View My Bookings", AppUrl("Customer/CustomerMyBookings.aspx")));

                string toCompanion = $@"
<p>Hello <strong>{HttpUtility.HtmlEncode(m.CompanionName)}</strong>,</p>
<p>A booking with <strong>{HttpUtility.HtmlEncode(m.CustomerName)}</strong> has been <strong style='color:#b7791f'>cancelled by the administrator</strong>. Here are the details of the cancelled booking:</p>
{BookingDetails(m, new[] { "Customer", m.CustomerName })}
{note}
<p>This time is now free again, and no further action is needed from you.</p>";
                bool sentCompanion = TrySend(m.CompanionEmail, "OCBMS - A Booking Has Been Cancelled",
                    Layout("Booking Cancelled", toCompanion, "View Notifications", AppUrl("Companion/CompanionNotifications.aspx")));

                return sentCustomer && sentCompanion;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EmailHelper cancellation (admin) error: " + ex);
                return false;
            }
        }

        // ===== Shared HTML layout para pare-pareho ang itsura ng lahat ng email =====
        private static string Layout(string heading, string contentHtml, string buttonText = null, string buttonUrl = null)
        {
            string button = string.IsNullOrEmpty(buttonUrl) ? "" :
                $"<p style='text-align:center;margin:28px 0'><a href='{buttonUrl}' style='background:#007bff;color:#ffffff;text-decoration:none;padding:12px 28px;border-radius:6px;font-weight:600;display:inline-block'>{buttonText}</a></p>";

            return $@"
<div style='background:#f4f6f9;padding:24px 12px;font-family:Segoe UI,Arial,sans-serif'>
  <div style='max-width:560px;margin:auto;background:#ffffff;border-radius:10px;overflow:hidden;border:1px solid #e3e7ed'>
    <div style='background:#007bff;color:#ffffff;padding:20px 28px'>
      <div style='font-size:13px;letter-spacing:1px;opacity:.9'>OCBMS</div>
      <div style='font-size:18px;font-weight:600'>Online Companion Booking Management System</div>
    </div>
    <div style='padding:28px;color:#222;font-size:15px;line-height:1.6'>
      <h2 style='margin:0 0 16px;font-size:20px;color:#1a1a1a'>{heading}</h2>
      {contentHtml}
      {button}
    </div>
    <div style='background:#f8f9fb;padding:16px 28px;font-size:12px;color:#888;border-top:1px solid #e3e7ed'>
      This is an automated message, please do not reply. For help, contact support@ocbs.com.<br/>
      &copy; {DateTime.Now.Year} Online Companion Booking Management System
    </div>
  </div>
</div>";
        }

        // Full address of a page of this site (for the buttons in emails), or null when there is no web request
        private static string AppUrl(string path)
        {
            var req = HttpContext.Current?.Request;
            if (req == null) return null;
            return req.Url.GetLeftPart(UriPartial.Authority) + req.ApplicationPath.TrimEnd('/') + "/" + path;
        }

        private static string LoginUrl()
        {
            var req = HttpContext.Current?.Request;
            if (req == null) return null;
            return req.Url.GetLeftPart(UriPartial.Authority) + req.ApplicationPath.TrimEnd('/') + "/Account/Login.aspx";
        }
    }
}
