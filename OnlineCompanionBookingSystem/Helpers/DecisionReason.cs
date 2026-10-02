using System;
using System.Web;

namespace OnlineCompanionBookingSystem
{
    /// <summary>
    /// The reason an administrator gives when approving, rejecting, or revoking a companion. The admin pages collect it
    /// in a dialog (Scripts/admin-decision.js) and send it in the hidden form field "decisionReason". It is shown to
    /// the companion in the email and the in-app notification. The server checks it again, because the browser can be bypassed.
    /// </summary>
    public static class DecisionReason
    {
        public const int MaxLength = 300;

        // The reason from the submitted form, trimmed and cut to MaxLength; empty when there is none
        public static string Read(HttpRequest request)
        {
            string text = (request == null ? null : request.Form["decisionReason"]) ?? string.Empty;
            text = text.Trim();
            return text.Length > MaxLength ? text.Substring(0, MaxLength) : text;
        }

        // A rejection or a revocation must always come with a reason; an approval may go without one
        public static bool IsRequired(string action)
        {
            return string.Equals(action, "Reject", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(action, "Revoke", StringComparison.OrdinalIgnoreCase);
        }
    }
}
