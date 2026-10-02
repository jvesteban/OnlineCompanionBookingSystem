using System;
using System.Web;

namespace OnlineCompanionBookingSystem
{
    /// <summary>
    /// Output-encoding helpers for text that came from users (names, emails, comments, bios, ...).
    /// Anything a user can type must be encoded for the place it is written to, otherwise a value such as
    /// <c>&lt;script&gt;...</c> or a name containing a quote could run as code in another user's browser (XSS).
    /// (Kept outside App_Code on purpose so the type is compiled only once.)
    /// </summary>
    public static class WebSafe
    {
        // Safe to place between HTML tags or inside an HTML attribute. Null/DBNull become "".
        // In .aspx markup, "<%#: ... %>" does the same thing automatically for data-bound text.
        public static string Html(object value)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(value) ?? string.Empty);
        }

        // Safe to place inside a JavaScript string literal, e.g. onclick="openModal('<%#: WebSafe.Js(Eval("Name")) %>')".
        // This only does the JavaScript escaping (quotes, backslashes, "<", ">", newlines...). The surrounding
        // "<%#: %>" then HTML-encodes it once, which is the correct order for JS inside an HTML attribute.
        public static string Js(object value)
        {
            return HttpUtility.JavaScriptStringEncode(Convert.ToString(value) ?? string.Empty);
        }
    }
}
