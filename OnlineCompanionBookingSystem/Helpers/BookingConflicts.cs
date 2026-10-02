using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;

namespace OnlineCompanionBookingSystem
{
    /// <summary>
    /// Prevents double-booking. A booking occupies the time from its start to its end (start + package length), and two
    /// bookings conflict when those times overlap. The rules:
    ///   1. A customer cannot have two Pending / Confirmed bookings that overlap, with the same companion or a different one.
    ///   2. A companion cannot be booked at a time when they already have a Confirmed booking (from anyone).
    ///      Another customer's Pending request does not block, because the companion may still decline it.
    ///   3. A companion cannot accept a request that overlaps one they already confirmed (CheckAccept).
    /// The checks run inside the caller's transaction and lock the rows they read, so two requests made at the same
    /// moment cannot both slip through. A booking that is Declined, Cancelled, or Completed never blocks anything.
    /// </summary>
    public static class BookingConflicts
    {
        private class Existing
        {
            public int BookingId;
            public int CustomerId;
            public int CompanionId;
            public string Status;
            public DateTime Start;
            public DateTime End;
        }

        // Pending / Confirmed bookings of this customer or this companion that could reach the given time window.
        // Packages last at most 30 days, so only bookings that started up to 31 days earlier can still be running.
        private static List<Existing> LoadActive(SqlConnection connection, SqlTransaction tx, int customerId, int companionId, DateTime start, DateTime end)
        {
            const string sql = @"
                SELECT b.BookingID, b.CustomerID, b.CompanionID, b.Status, b.BookingDate, b.BookingTime, p.Duration
                FROM Bookings b WITH (UPDLOCK, HOLDLOCK)
                INNER JOIN Packages p ON p.PackageID = b.PackageID
                WHERE b.Status IN ('Pending', 'Confirmed')
                  AND b.BookingDate >= @From AND b.BookingDate <= @To
                  AND (b.CustomerID = @CustomerID OR b.CompanionID = @CompanionID)";

            var list = new List<Existing>();
            using (var command = new SqlCommand(sql, connection, tx))
            {
                command.Parameters.AddWithValue("@From", start.Date.AddDays(-31));
                command.Parameters.AddWithValue("@To", end.Date);
                command.Parameters.AddWithValue("@CustomerID", customerId);
                command.Parameters.AddWithValue("@CompanionID", companionId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime s, e;
                        if (!BookingStatus.TryGetSchedule(reader["BookingDate"], reader["BookingTime"], reader["Duration"], out s, out e)) continue;
                        list.Add(new Existing
                        {
                            BookingId = Convert.ToInt32(reader["BookingID"]),
                            CustomerId = Convert.ToInt32(reader["CustomerID"]),
                            CompanionId = Convert.ToInt32(reader["CompanionID"]),
                            Status = Convert.ToString(reader["Status"]),
                            Start = s,
                            End = e
                        });
                    }
                }
            }
            return list;
        }

        private static bool Overlaps(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
        {
            return aStart < bEnd && bStart < aEnd;
        }

        // "Oct 05, 12:00 PM - 1:00 PM" (the end shows its date too when the booking runs past midnight)
        private static string Describe(DateTime start, DateTime end)
        {
            string from = start.ToString("MMM dd, h:mm tt", CultureInfo.InvariantCulture);
            string to = end.Date == start.Date
                ? end.ToString("h:mm tt", CultureInfo.InvariantCulture)
                : end.ToString("MMM dd, h:mm tt", CultureInfo.InvariantCulture);
            return from + " - " + to;
        }

        /// <summary>
        /// Checks a new booking request before it is saved (call it inside the transaction, before the INSERT).
        /// Returns null when the time is free, otherwise the message to show the customer.
        /// </summary>
        public static string CheckNewBooking(SqlConnection connection, SqlTransaction tx, int customerId, int companionId, int packageId, DateTime bookingDate, TimeSpan bookingTime)
        {
            object duration;
            using (var command = new SqlCommand("SELECT Duration FROM Packages WHERE PackageID = @PackageID", connection, tx))
            {
                command.Parameters.AddWithValue("@PackageID", packageId);
                duration = command.ExecuteScalar();
            }
            if (duration == null) return "This package is no longer available.";

            DateTime start = bookingDate.Date + bookingTime;
            DateTime end = start + BookingStatus.ParseDuration(duration);

            foreach (Existing other in LoadActive(connection, tx, customerId, companionId, start, end))
            {
                if (!Overlaps(start, end, other.Start, other.End)) continue;

                if (other.CustomerId == customerId)
                {
                    return other.CompanionId == companionId
                        ? "You already have a " + other.Status.ToLowerInvariant() + " booking with this companion that overlaps this time (" + Describe(other.Start, other.End) + "). Please choose a different time, or cancel that booking first."
                        : "You already have a " + other.Status.ToLowerInvariant() + " booking with another companion that overlaps this time (" + Describe(other.Start, other.End) + "). Please choose a different time.";
                }

                if (other.CompanionId == companionId && other.Status == "Confirmed")
                    return "This companion is already booked from " + Describe(other.Start, other.End) + ". Please choose another time.";
            }
            return null;
        }

        /// <summary>
        /// Checks, before a companion accepts a request, that it does not overlap a booking they already confirmed.
        /// Returns null when it is fine, otherwise the message to show the companion.
        /// </summary>
        public static string CheckAccept(SqlConnection connection, SqlTransaction tx, int bookingId, int companionId)
        {
            const string sql = @"
                SELECT b.CustomerID, b.BookingDate, b.BookingTime, p.Duration
                FROM Bookings b
                INNER JOIN Packages p ON p.PackageID = b.PackageID
                WHERE b.BookingID = @BookingID AND b.CompanionID = @CompanionID";

            int customerId;
            DateTime start, end;
            using (var command = new SqlCommand(sql, connection, tx))
            {
                command.Parameters.AddWithValue("@BookingID", bookingId);
                command.Parameters.AddWithValue("@CompanionID", companionId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return null;   // the caller reports a missing booking itself
                    customerId = Convert.ToInt32(reader["CustomerID"]);
                    if (!BookingStatus.TryGetSchedule(reader["BookingDate"], reader["BookingTime"], reader["Duration"], out start, out end)) return null;
                }
            }

            foreach (Existing other in LoadActive(connection, tx, customerId, companionId, start, end))
            {
                if (other.BookingId == bookingId || other.CompanionId != companionId || other.Status != "Confirmed") continue;
                if (Overlaps(start, end, other.Start, other.End))
                    return "You already have a confirmed booking from " + Describe(other.Start, other.End) + " that overlaps this request. Decline this request or choose another.";
            }
            return null;
        }
    }
}
