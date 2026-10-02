using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OnlineCompanionBookingSystem.Customer
{
    // Customer > Companion Profile: a companion's public page (bio, activities, reviews, packages).
    // This is also where the customer books a package: pick a date and time, and a Pending booking
    // plus a notification for the companion is created.
    public partial class CompanionProfile : Page
    {
        // Connection string from Web.config
        private string ConnectionString => ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;

        // Which companion to show, from the URL (?id=5). 0 when missing or not a number.
        private int CompanionId => int.TryParse(Request.QueryString["id"], out int id) ? id : 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Only logged-in customers may open this page; everyone else goes back to Login.
            if (Session["UserID"] == null || Session["Role"]?.ToString() != "Customer")
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                if (CompanionId <= 0)
                {
                    Response.Redirect("~/Customer/BrowseCompanion.aspx");
                    return;
                }

                LoadCompanionDetails(CompanionId);
                LoadActivities(CompanionId);
                LoadReviews(CompanionId);
                LoadPackages(CompanionId);
                LoadAvailability(CompanionId);
            }
        }

        // Name, picture (or initials), bio, average rating (5.0 when there are no reviews yet), and review count.
        // Redirects back to the browse page if the companion does not exist.
        private void LoadCompanionDetails(int companionId)
        {
            const string query = @"
                SELECT TOP 1 
                    u.FullName, 
                    u.ProfilePicture, 
                    cp.Bio,
                    ISNULL((SELECT AVG(CAST(r.Score AS DECIMAL(10,2))) FROM Ratings r WHERE r.CompanionID = cp.CompanionID), 5.0) AS AvgRating,
                    ISNULL((SELECT COUNT(r.RatingID) FROM Ratings r WHERE r.CompanionID = cp.CompanionID), 0) AS TotalReviews
                FROM CompanionProfiles cp
                INNER JOIN Users u ON cp.UserID = u.UserID
                WHERE cp.CompanionID = @CompanionID";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CompanionID", companionId);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string fullName = reader["FullName"] != DBNull.Value ? reader["FullName"].ToString().Trim() : "Companion";
                        string profilePic = reader["ProfilePicture"] != DBNull.Value ? reader["ProfilePicture"].ToString().Trim() : string.Empty;
                        // The bio is optional: NULL or only spaces both count as "not written yet"
                        string bio = reader["Bio"] != DBNull.Value ? reader["Bio"].ToString().Trim() : string.Empty;
                        decimal avgRating = reader["AvgRating"] != DBNull.Value ? Convert.ToDecimal(reader["AvgRating"]) : 5.0m;
                        int totalReviews = reader["TotalReviews"] != DBNull.Value ? Convert.ToInt32(reader["TotalReviews"]) : 0;

                        litFullName.Text = Server.HtmlEncode(fullName);
                        // The bio is typed by the companion, so it is HTML-encoded; line breaks are kept by the page's CSS.
                        // The "not written yet" message is our own fixed text.
                        litBio.Text = bio.Length > 0
                            ? Server.HtmlEncode(bio)
                            : "<em class=\"bio-empty\">This companion hasn't written an introduction yet.</em>";
                        litAvgRating.Text = avgRating.ToString("0.0");
                        litTotalReviews.Text = totalReviews.ToString();
                        litStars.Text = GetStarString(Math.Round(avgRating, 0));

                        // Initials calculation
                        string initials = "C";
                        string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 1)
                        {
                            initials = (parts[0][0].ToString() + parts[parts.Length - 1][0]).ToUpperInvariant();
                        }
                        else if (parts.Length == 1 && parts[0].Length > 0)
                        {
                            initials = parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();
                        }
                        litCompanionInitials.Text = initials;

                        if (!string.IsNullOrWhiteSpace(profilePic))
                        {
                            imgCompanionAvatar.ImageUrl = profilePic;
                            imgCompanionAvatar.Visible = true;
                            divAvatarFallback.Visible = false;
                        }
                        else
                        {
                            imgCompanionAvatar.Visible = false;
                            divAvatarFallback.Visible = true;
                        }
                    }
                    else
                    {
                        Response.Redirect("~/Customer/BrowseCompanion.aspx");
                    }
                }
            }
        }

        // The activity tags on the profile. The first activity is also used as the headline specialty.
        // Activities are stored directly in the Activities table (one row per companion + activity name),
        // the same table the registration form and the Companion > Activities page write to.
        private void LoadActivities(int companionId)
        {
            const string query = @"
                SELECT ActivityID, ActivityName
                FROM Activities
                WHERE CompanionID = @CompanionID
                ORDER BY ActivityName";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CompanionID", companionId);
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        adapter.Fill(dt);
                    }
                    catch (Exception ex)
                    {
                        // Don't hide the failure: log it, then show the page with the "no activities" message
                        // instead of crashing (the rest of the profile can still be shown).
                        System.Diagnostics.Debug.WriteLine("LoadActivities error: " + ex);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        // Each activity becomes a checkbox the customer can tick (text = name, value = ActivityID)
                        cblActivities.DataSource = dt;
                        cblActivities.DataBind();
                        pnlActivityPicker.Visible = true;
                        lblNoActivities.Visible = false;
                        litMainTag.Text = Server.HtmlEncode(dt.Rows[0]["ActivityName"].ToString());
                    }
                    else
                    {
                        pnlActivityPicker.Visible = false;
                        lblNoActivities.Visible = true;
                        litMainTag.Text = "Companion Specialist";
                    }
                }
            }
        }

        // All reviews for this companion, newest first.
        private void LoadReviews(int companionId)
        {
            const string query = @"
                SELECT u.FullName AS CustomerName, r.Score, r.Comment, r.DateCreated AS CreatedAt
                FROM Ratings r
                INNER JOIN Users u ON r.CustomerID = u.UserID
                WHERE r.CompanionID = @CompanionID
                ORDER BY r.RatingID DESC";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CompanionID", companionId);
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        rptReviews.DataSource = dt;
                        rptReviews.DataBind();
                        rptReviews.Visible = true;
                        lblNoReviews.Visible = false;
                    }
                    else
                    {
                        rptReviews.Visible = false;
                        lblNoReviews.Visible = true;
                    }
                }
            }
        }

        // The packages the customer can book (each row has its own date/time boxes and a Book button).
        private void LoadPackages(int companionId)
        {
            const string query = @"
                SELECT PackageID, PackageName, Description, Rate, Duration
                FROM Packages
                WHERE CompanionID = @CompanionID";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CompanionID", companionId);
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        rptPackages.DataSource = dt;
                        rptPackages.DataBind();
                        rptPackages.Visible = true;
                        lblNoPackages.Visible = false;
                    }
                    else
                    {
                        rptPackages.Visible = false;
                        lblNoPackages.Visible = true;
                    }
                }
            }
        }

        // The companion's weekly schedule (set in Companion > Availability), shown as one row per day.
        private void LoadAvailability(int companionId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    litAvailability.Text = AvailabilityService.ProfileHtml(AvailabilityService.LoadFor(connection, companionId));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadAvailability error: " + ex);
                litAvailability.Text = "<p class=\"empty-text\">The schedule could not be loaded right now.</p>";
            }
        }

        // Checks the requested date and time before anything is saved: the package must belong to this companion,
        // the start must be in the future, and it must fall inside the companion's weekly availability.
        // Returns null when everything is fine, otherwise the message for the customer.
        private string ValidateSchedule(int packageId, DateTime bookingDate, string bookingTime)
        {
            TimeSpan time;
            if (!TimeSpan.TryParse(bookingTime, System.Globalization.CultureInfo.InvariantCulture, out time) || time < TimeSpan.Zero || time >= TimeSpan.FromDays(1))
                return "Please enter a valid booking time.";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                object duration;
                using (SqlCommand command = new SqlCommand("SELECT Duration FROM Packages WHERE PackageID = @PackageID AND CompanionID = @CompanionID", connection))
                {
                    command.Parameters.AddWithValue("@PackageID", packageId);
                    command.Parameters.AddWithValue("@CompanionID", CompanionId);
                    duration = command.ExecuteScalar();
                }
                if (duration == null) return "This package is no longer available.";

                string durationText = Convert.ToString(duration);
                bool inDays = durationText.IndexOf("day", StringComparison.OrdinalIgnoreCase) >= 0;
                return AvailabilityService.Check(AvailabilityService.LoadFor(connection, CompanionId), bookingDate, time,
                    BookingStatus.ParseDuration(durationText), inDays, DateTime.Now);
            }
        }

        // "Book" button on a package row (CommandArgument = PackageID).
        // Creates a Pending booking and a notification for the companion in one transaction,
        // so a booking can never exist without the companion being told about it.
        protected void rptPackages_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "BookPackage")
            {
                int packageId = Convert.ToInt32(e.CommandArgument);
                TextBox txtDate = e.Item.FindControl("txtBookingDate") as TextBox;
                TextBox txtTime = e.Item.FindControl("txtBookingTime") as TextBox;
                Label lblMsg = lblBookingMessage;

                if (txtDate == null || string.IsNullOrWhiteSpace(txtDate.Text) ||
                    txtTime == null || string.IsNullOrWhiteSpace(txtTime.Text))
                {
                    if (lblMsg != null)
                    {
                        lblMsg.Text = "Please select both a booking date and time before continuing.";
                        lblMsg.CssClass = "booking-message error";
                        lblMsg.Visible = true;
                    }
                    return;
                }

                // The activities the customer ticked (several allowed). If the companion offers activities,
                // at least one must be chosen. They are checked against the companion again when saved.
                var selectedActivityIds = new List<int>();
                foreach (ListItem item in cblActivities.Items)
                {
                    int id;
                    if (item.Selected && int.TryParse(item.Value, out id)) selectedActivityIds.Add(id);
                }

                if (cblActivities.Items.Count > 0 && selectedActivityIds.Count == 0)
                {
                    if (lblMsg != null)
                    {
                        lblMsg.Text = "Please select at least one activity you would like to book with this companion.";
                        lblMsg.CssClass = "booking-message error";
                        lblMsg.Visible = true;
                    }
                    return;
                }

                try
                {
                    int customerId = Convert.ToInt32(Session["UserID"]);
                    DateTime bookingDate = Convert.ToDateTime(txtDate.Text);
                    string bookingTime = txtTime.Text.Trim();
                    string activitiesText = string.Empty;

                    // Make sure the date/time is in the future and inside the companion's availability
                    string scheduleProblem = ValidateSchedule(packageId, bookingDate, bookingTime);
                    if (scheduleProblem != null)
                    {
                        lblMsg.Text = scheduleProblem;
                        lblMsg.CssClass = "booking-message error";
                        lblMsg.Visible = true;
                        return;
                    }

                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();
                        BookingActivities.EnsureSchema(connection);   // creates the BookingActivities table the first time
                        using (SqlTransaction transaction = connection.BeginTransaction())
                        {
                            // No double-booking: the time must not overlap this customer's other bookings or one the
                            // companion already confirmed (checked inside the transaction, so two requests cannot both pass)
                            string conflict = BookingConflicts.CheckNewBooking(connection, transaction, customerId, CompanionId, packageId, bookingDate, TimeSpan.Parse(bookingTime, System.Globalization.CultureInfo.InvariantCulture));
                            if (conflict != null)
                            {
                                transaction.Rollback();
                                lblMsg.Text = conflict;
                                lblMsg.CssClass = "booking-message error";
                                lblMsg.Visible = true;
                                return;
                            }

                            const string query = @"
                                INSERT INTO Bookings (CustomerID, CompanionID, PackageID, BookingDate, BookingTime, Status, DateCreated)
                                OUTPUT INSERTED.BookingID
                                VALUES (@CustomerID, @CompanionID, @PackageID, @BookingDate, @BookingTime, 'Pending', GETDATE())";

                            int newBookingId;
                            using (SqlCommand command = new SqlCommand(query, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@CustomerID", customerId);
                                command.Parameters.AddWithValue("@CompanionID", CompanionId);
                                command.Parameters.AddWithValue("@PackageID", packageId);
                                command.Parameters.AddWithValue("@BookingDate", bookingDate);
                                command.Parameters.AddWithValue("@BookingTime", bookingTime);
                                newBookingId = Convert.ToInt32(command.ExecuteScalar());
                            }

                            // Save the chosen activities with the booking (same transaction: all or nothing)
                            if (selectedActivityIds.Count > 0)
                            {
                                List<string> activityNames = BookingActivities.Save(connection, transaction, newBookingId, CompanionId, selectedActivityIds);
                                activitiesText = string.Join(", ", activityNames);
                            }

                            // Abisuhan ang companion (gamit ang UserID niya) na may bagong booking request,
                            // kasama ang mga activity na napili
                            const string notificationQuery = @"
                                INSERT INTO Notifications (UserID, Message, IsRead, DateCreated, RelatedBookingID)
                                SELECT cp.UserID,
                                       u.FullName + ' sent a new booking request for ' + p.PackageName +
                                       CASE WHEN @Activities = '' THEN '' ELSE ' (' + @Activities + ')' END +
                                       ' on ' + CONVERT(VARCHAR(12), @BookingDate, 107) + ' at ' + @BookingTime + '.',
                                       0, GETDATE(), @BookingID
                                FROM CompanionProfiles cp, Users u, Packages p
                                WHERE cp.CompanionID = @CompanionID AND u.UserID = @CustomerID AND p.PackageID = @PackageID";

                            using (SqlCommand notificationCommand = new SqlCommand(notificationQuery, connection, transaction))
                            {
                                notificationCommand.Parameters.AddWithValue("@CustomerID", customerId);
                                notificationCommand.Parameters.AddWithValue("@CompanionID", CompanionId);
                                notificationCommand.Parameters.AddWithValue("@PackageID", packageId);
                                notificationCommand.Parameters.AddWithValue("@BookingDate", bookingDate);
                                notificationCommand.Parameters.AddWithValue("@BookingTime", bookingTime);
                                notificationCommand.Parameters.AddWithValue("@BookingID", newBookingId);
                                notificationCommand.Parameters.AddWithValue("@Activities", activitiesText);
                                notificationCommand.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                    }

                    // Booking done: untick the activities so the next booking starts fresh
                    foreach (ListItem item in cblActivities.Items) item.Selected = false;

                    if (lblMsg != null)
                    {
                        lblMsg.Text = activitiesText.Length > 0
                            ? "Booking request submitted for: " + Server.HtmlEncode(activitiesText) + ". Your booking is now pending confirmation."
                            : "Booking request submitted successfully. Your booking is now pending confirmation.";
                        lblMsg.CssClass = "booking-message success";
                        lblMsg.Visible = true;
                    }
                }
                catch (Exception ex)
                {
                    // Log the real reason for developers; the customer only sees a friendly message
                    System.Diagnostics.Debug.WriteLine("Booking error: " + ex);
                    if (lblMsg != null)
                    {
                        lblMsg.Text = "We could not submit your booking right now. Please check your details and try again.";
                        lblMsg.CssClass = "booking-message error";
                        lblMsg.Visible = true;
                    }
                }
            }
        }

        // Stars out of 5 for a rating, e.g. 4 -> "★★★★☆" (minimum 1 star).
        protected string GetStarString(decimal score)
        {
            int stars = Math.Max(1, Math.Min(5, (int)Math.Round(score, 0)));
            return new string('★', stars) + new string('☆', 5 - stars);
        }

        // Same as GetStarString but for a raw value from the data source (defaults to 5 if it is not a number).
        protected string FormatStars(object scoreObj)
        {
            if (scoreObj == null || !decimal.TryParse(scoreObj.ToString(), out decimal score))
            {
                score = 5;
            }
            int stars = Math.Max(1, Math.Min(5, (int)Math.Round(score, 0)));
            return new string('★', stars) + new string('☆', 5 - stars);
        }

        // Clears the session and returns to the login page.
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Account/Login.aspx");
        }
    }
}