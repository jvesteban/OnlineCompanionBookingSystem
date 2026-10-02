using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

namespace OnlineCompanionBookingSystem
{
    /// <summary>
    /// The weekly availability a companion sets in Companion > Availability (table Availability:
    /// DayOfWeek, StartTime, EndTime), made visible and enforced on the customer side:
    ///   - Summary / ProfileHtml: the text shown on the Browse card and the companion's profile.
    ///   - Check: used when a customer books, so a booking can only be made inside the companion's open hours.
    /// A companion who has not set any slot yet is treated as "not set": nothing is blocked, and the pages say so.
    /// </summary>
    public static class AvailabilityService
    {
        public class Slot
        {
            public DayOfWeek Day;
            public TimeSpan Start;
            public TimeSpan End;
        }

        private static readonly string[] DayNames = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        // Monday = 0 ... Sunday = 6 (the order the companion sees on the Availability page)
        private static int DayIndex(DayOfWeek day) { return ((int)day + 6) % 7; }

        // Loads the slots of the given companions in one query, grouped by CompanionID. Companions without slots are not in the result.
        public static Dictionary<int, List<Slot>> LoadFor(SqlConnection connection, IEnumerable<int> companionIds)
        {
            var result = new Dictionary<int, List<Slot>>();
            List<int> ids = companionIds.Distinct().ToList();
            if (ids.Count == 0) return result;

            // The ids are integers from our own query, but they still go in as parameters
            var names = new List<string>();
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                for (int i = 0; i < ids.Count; i++)
                {
                    names.Add("@c" + i);
                    command.Parameters.AddWithValue("@c" + i, ids[i]);
                }
                command.CommandText = "SELECT CompanionID, DayOfWeek, StartTime, EndTime FROM Availability WHERE CompanionID IN (" + string.Join(",", names) + ")";

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DayOfWeek day;
                        TimeSpan start, end;
                        if (!Enum.TryParse(Convert.ToString(reader["DayOfWeek"]).Trim(), true, out day)) continue;
                        if (!TimeSpan.TryParse(Convert.ToString(reader["StartTime"]), CultureInfo.InvariantCulture, out start)) continue;
                        if (!TimeSpan.TryParse(Convert.ToString(reader["EndTime"]), CultureInfo.InvariantCulture, out end)) continue;

                        int id = Convert.ToInt32(reader["CompanionID"]);
                        List<Slot> list;
                        if (!result.TryGetValue(id, out list)) result[id] = list = new List<Slot>();
                        list.Add(new Slot { Day = day, Start = start, End = end });
                    }
                }
            }
            return result;
        }

        public static List<Slot> LoadFor(SqlConnection connection, int companionId)
        {
            List<Slot> slots;
            return LoadFor(connection, new[] { companionId }).TryGetValue(companionId, out slots) ? slots : new List<Slot>();
        }

        // "9:00 AM"
        public static string FormatTime(TimeSpan time)
        {
            return DateTime.Today.Add(time).ToString("h:mm tt", CultureInfo.InvariantCulture);
        }

        private static string Range(Slot slot) { return FormatTime(slot.Start) + " - " + FormatTime(slot.End); }

        // Short text for the Browse card, e.g. "Mon - Fri, 9:00 AM - 5:00 PM". Days that share the same hours are combined;
        // different hours are separated by " | ". Plain text (the page encodes it).
        public static string Summary(List<Slot> slots)
        {
            if (slots == null || slots.Count == 0) return "Availability not set yet";

            var parts = new List<string>();
            foreach (var group in slots.GroupBy(Range).OrderBy(g => g.Min(s => DayIndex(s.Day) * 10000 + (int)s.Start.TotalMinutes)))
            {
                parts.Add(DayList(group.Select(s => DayIndex(s.Day)).Distinct().OrderBy(d => d).ToList()) + ", " + group.Key);
            }
            return string.Join(" | ", parts);
        }

        // [0,1,2,4] -> "Mon - Wed, Fri" (three or more days in a row become a range)
        private static string DayList(List<int> days)
        {
            var pieces = new List<string>();
            int i = 0;
            while (i < days.Count)
            {
                int j = i;
                while (j + 1 < days.Count && days[j + 1] == days[j] + 1) j++;
                string first = DayNames[days[i]].Substring(0, 3);
                string last = DayNames[days[j]].Substring(0, 3);
                if (j - i >= 2) pieces.Add(first + " - " + last);
                else for (int k = i; k <= j; k++) pieces.Add(DayNames[days[k]].Substring(0, 3));
                i = j + 1;
            }
            return string.Join(", ", pieces);
        }

        // The full weekly list for the companion's profile: one row per day (Monday -> Sunday), "Not available" for days
        // without a slot. Everything is encoded; the result is safe to put in a Literal.
        public static string ProfileHtml(List<Slot> slots)
        {
            if (slots == null || slots.Count == 0)
                return "<p class=\"empty-text\">This companion has not set a weekly schedule yet. Booking requests are still welcome; the companion will confirm the time.</p>";

            var sb = new StringBuilder("<ul class=\"availability-list\">");
            for (int i = 0; i < 7; i++)
            {
                var daySlots = slots.Where(s => DayIndex(s.Day) == i).OrderBy(s => s.Start).ToList();
                sb.Append("<li class=\"availability-row").Append(daySlots.Count == 0 ? " is-off" : "").Append("\">");
                sb.Append("<span class=\"availability-day\">").Append(HttpUtility.HtmlEncode(DayNames[i])).Append("</span>");
                sb.Append("<span class=\"availability-times\">");
                sb.Append(daySlots.Count == 0 ? "Not available" : HttpUtility.HtmlEncode(string.Join(", ", daySlots.Select(Range))));
                sb.Append("</span></li>");
            }
            return sb.Append("</ul>").ToString();
        }

        /// <summary>
        /// Checks a booking request. Returns null when it is fine, otherwise a message for the customer.
        /// - The start must be in the future.
        /// - When the companion has a schedule, the start must be on one of their days and inside a slot; a package
        ///   measured in hours must also finish before that slot ends. A package measured in days only needs a valid start.
        /// </summary>
        public static string Check(List<Slot> slots, DateTime date, TimeSpan time, TimeSpan packageLength, bool packageInDays, DateTime now)
        {
            DateTime start = date.Date + time;
            if (start <= now) return "Please choose a date and time in the future.";

            if (slots == null || slots.Count == 0) return null;   // no schedule set: nothing to enforce

            var today = slots.Where(s => s.Day == date.DayOfWeek).ToList();
            TimeSpan end = time + packageLength;
            bool fits = today.Any(s => time >= s.Start && time < s.End && (packageInDays || end <= s.End));
            if (fits) return null;

            string days = Summary(slots);
            if (today.Count == 0)
                return "This companion is not available on " + date.DayOfWeek + "s. Available: " + days + ".";
            return packageInDays
                ? "That start time is outside this companion's hours on " + date.DayOfWeek + "s. Available: " + days + "."
                : "That time does not fit this companion's hours on " + date.DayOfWeek + "s (the package must finish before the slot ends). Available: " + days + ".";
        }
    }
}
