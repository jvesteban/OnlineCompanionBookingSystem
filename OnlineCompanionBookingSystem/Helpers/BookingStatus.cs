using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace OnlineCompanionBookingSystem
{
    /// <summary>
    /// The status a booking is DISPLAYED with. The database only stores Pending / Confirmed / Declined / Cancelled /
    /// Completed. "On-going" and "Awaiting Completion" are worked out from the booking's date, time, and the package
    /// length, so nothing has to be saved or updated when the time arrives:
    ///
    ///   Confirmed, before the start time          -> "Confirmed"           (note: "Starts today at 3:00 PM")
    ///   Confirmed, from the start until it ends   -> "On-going"            (note: "Until 5:00 PM")
    ///   Confirmed, after the end, not completed   -> "Awaiting Completion" (the companion should mark it Completed)
    ///   Every other status                        -> shown as it is
    ///
    /// Times use the server's clock, the same clock that stamps DateCreated.
    /// </summary>
    public static class BookingStatus
    {
        public const string OnGoing = "On-going";
        public const string AwaitingCompletion = "Awaiting Completion";

        // Matches package lengths such as "1 Hour", "2 Hours", "3 Days"
        private static readonly Regex DurationPattern = new Regex(@"^\s*(\d+)\s*(hour|day)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>What to show for one booking.</summary>
        public class Info
        {
            public string DisplayStatus;   // e.g. "On-going"
            public string Key;             // for CSS: pending, confirmed, ongoing, awaiting, completed, declined, cancelled
            public string Note;            // small line under the badge ("" when there is nothing to add)
            public string StartIso = "";   // "2026-10-03T15:00:00" (only for Confirmed bookings; used to switch live in the browser)
            public string EndIso = "";
            public string OngoingNote = "";
            public string AwaitingNote = "";
            public string UntilText = "";    // "Oct. 04, 2026 at 10:00 AM": when the booking ends (shown for every status except Declined / Cancelled)
        }

        // Works out the display status of one booking. "status" is the stored status; start time is date + time text.
        public static Info Compute(string status, object bookingDate, object bookingTime, object duration, DateTime now)
        {
            status = (status ?? string.Empty).Trim();
            var info = new Info { DisplayStatus = status, Key = status.ToLowerInvariant(), Note = string.Empty };

            
            // The end date and time (start + package length) is shown for every booking that can still happen or already did
            if (!string.Equals(status, "Declined", StringComparison.OrdinalIgnoreCase) && !string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                DateTime s0;
                if (TryGetStart(bookingDate, bookingTime, out s0))
                    info.UntilText = (s0 + ParseDuration(duration)).ToString("MMM. dd, yyyy 'at' h:mm tt", CultureInfo.InvariantCulture);
            }

if (!string.Equals(status, "Confirmed", StringComparison.OrdinalIgnoreCase)) return info;

            DateTime start;
            if (!TryGetStart(bookingDate, bookingTime, out start)) return info;   // cannot tell the time: keep "Confirmed"
            DateTime end = start + ParseDuration(duration);

            info.StartIso = start.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);
            info.EndIso = end.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);
            info.OngoingNote = "Until " + DescribeEnd(start, end);
            info.AwaitingNote = "Ended " + DescribeEnd(start, end) + ". Waiting to be marked as completed.";

            if (now < start)
            {
                info.DisplayStatus = "Confirmed";
                info.Key = "confirmed";
                info.Note = "Starts " + DescribeStart(start, now) + ". Until " + DescribeEnd(start, end) + ".";
            }
            else if (now < end)
            {
                info.DisplayStatus = OnGoing;
                info.Key = "ongoing";
                info.Note = info.OngoingNote;
            }
            else
            {
                info.DisplayStatus = AwaitingCompletion;
                info.Key = "awaiting";
                info.Note = info.AwaitingNote;
            }
            return info;
        }

        // A customer may cancel a booking that is Pending, or Confirmed but not started yet.
        // Once it is On-going or past its end, only the companion can complete it.
        public static bool CanCustomerCancel(string status, object bookingDate, object bookingTime, object duration, DateTime now)
        {
            string key = Compute(status, bookingDate, bookingTime, duration, now).Key;
            return key == "pending" || key == "confirmed";
        }

        // Adds the display columns to a table of bookings. The table must have Status, BookingDate, BookingTime and
        // Duration (the package length) columns. Columns added: DisplayStatus, StatusKey, StatusNote, StartIso, EndIso,
        // OngoingNote, AwaitingNote, ServerNowIso.
        public static void Apply(DataTable table)
        {
            DateTime now = DateTime.Now;
            string nowIso = now.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);
            foreach (string name in new[] { "DisplayStatus", "StatusKey", "StatusNote", "StartIso", "EndIso", "OngoingNote", "AwaitingNote", "UntilText", "ServerNowIso" })
            {
                if (!table.Columns.Contains(name)) table.Columns.Add(name, typeof(string));
            }

            bool hasDuration = table.Columns.Contains("Duration");
            foreach (DataRow row in table.Rows)
            {
                Info info = Compute(Convert.ToString(row["Status"]), row["BookingDate"], row["BookingTime"], hasDuration ? row["Duration"] : null, now);
                row["DisplayStatus"] = info.DisplayStatus;
                row["StatusKey"] = info.Key;
                row["StatusNote"] = info.Note;
                row["StartIso"] = info.StartIso;
                row["EndIso"] = info.EndIso;
                row["OngoingNote"] = info.OngoingNote;
                row["AwaitingNote"] = info.AwaitingNote;
                row["UntilText"] = info.UntilText;
                row["ServerNowIso"] = nowIso;
            }
        }

        // Date + time of the booking. The time was typed with a time box ("14:30"), but other forms are accepted too.
        private static bool TryGetStart(object bookingDate, object bookingTime, out DateTime start)
        {
            start = DateTime.MinValue;
            if (bookingDate == null || bookingDate == DBNull.Value) return false;

            DateTime date = Convert.ToDateTime(bookingDate).Date;
            string timeText = bookingTime == null || bookingTime == DBNull.Value ? string.Empty : Convert.ToString(bookingTime).Trim();

            TimeSpan time;
            if (TimeSpan.TryParse(timeText, CultureInfo.InvariantCulture, out time) && time >= TimeSpan.Zero && time < TimeSpan.FromDays(1))
            {
                start = date + time;
                return true;
            }

            DateTime parsed;
            if (DateTime.TryParse(timeText, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                start = date + parsed.TimeOfDay;
                return true;
            }
            return false;
        }

        // "2 Hours" -> 2 hours, "1 Day" -> 24 hours. Anything unreadable counts as 1 hour.
        public static TimeSpan ParseDuration(object duration)
        {
            string text = duration == null || duration == DBNull.Value ? string.Empty : Convert.ToString(duration);
            Match m = DurationPattern.Match(text);
            if (!m.Success) return TimeSpan.FromHours(1);

            int amount = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            if (amount <= 0) return TimeSpan.FromHours(1);
            return m.Groups[2].Value.StartsWith("d", StringComparison.OrdinalIgnoreCase) ? TimeSpan.FromDays(amount) : TimeSpan.FromHours(amount);
        }

        // "today at 3:00 PM", "tomorrow at 3:00 PM", or "Oct 05 at 3:00 PM"
        private static string DescribeStart(DateTime start, DateTime now)
        {
            string time = start.ToString("h:mm tt", CultureInfo.InvariantCulture);
            int days = (start.Date - now.Date).Days;
            if (days == 0) return "today at " + time;
            if (days == 1) return "tomorrow at " + time;
            return start.ToString("MMM dd", CultureInfo.InvariantCulture) + " at " + time;
        }

        // "5:00 PM" when it ends the same day, otherwise "Oct 04, 3:00 PM"
        private static string DescribeEnd(DateTime start, DateTime end)
        {
            return end.Date == start.Date
                ? end.ToString("h:mm tt", CultureInfo.InvariantCulture)
                : end.ToString("MMM dd, h:mm tt", CultureInfo.InvariantCulture);
        }
    }
}
