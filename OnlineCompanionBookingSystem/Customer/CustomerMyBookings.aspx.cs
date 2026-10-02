using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using OnlineCompanionBookingSystem;

namespace OnlineCompanionBookingManagementSystem.Customer
{
    // Customer > My Bookings: all of the customer's bookings with their status. A Pending booking can be
    // cancelled while Pending, or Confirmed but not started yet; a Completed booking links to the rating page (unless it was already rated).
    public partial class MyBookings : System.Web.UI.Page
    {
        // Connection string from Web.config
        private string connString => ConfigurationManager.ConnectionStrings["OCBSConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Siguraduhing may nakalogin at Customer ang role
                if (Session["UserID"] == null || Session["Role"]?.ToString() != "Customer")
                {
                    Response.Redirect("../Account/Login.aspx");
                    return;
                }

                int customerUserID = Convert.ToInt32(Session["UserID"]);

                LoadUserData(customerUserID);
                BindCustomerBookings(customerUserID);

                // The customer has now seen the update on their bookings, so the sidebar's "new" counter goes away
                NavCounts.MarkSeen(NavCounts.Customer, customerUserID, "bookings");
            }
        }

        // Kunin ang FullName, Email, at ProfilePicture ng user mula sa Database
        private void LoadUserData(int userID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string query = "SELECT FullName, Email, ProfilePicture FROM Users WHERE UserID = @UserID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userID);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string fullName = reader["FullName"] != DBNull.Value ? reader["FullName"].ToString().Trim() : "User";
                                string email = reader["Email"] != DBNull.Value ? reader["Email"].ToString().Trim() : "customer@ocbs.com";
                                string profilePic = reader["ProfilePicture"] != DBNull.Value ? reader["ProfilePicture"].ToString().Trim() : string.Empty;
                                string firstName = fullName.Split(' ')[0];

                                // Topbar Name & Dropdown info
                                litUserFirstName.Text = Server.HtmlEncode(firstName);
                                litDropdownName.Text = Server.HtmlEncode(fullName);
                                litDropdownEmail.Text = Server.HtmlEncode(email);

                                // Pagbuo ng Initial Avatar (Halimbawa: Maria Santos -> MS)
                                string initials = "U";
                                if (!string.IsNullOrEmpty(fullName))
                                {
                                    string[] parts = fullName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (parts.Length >= 2)
                                    {
                                        initials = (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
                                    }
                                    else if (parts.Length == 1 && parts[0].Length > 0)
                                    {
                                        initials = parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
                                    }
                                }

                                litAvatarInitial.Text = initials;
                                litAvatarInitialLg.Text = initials;

                                // Pamamahala sa Profile Picture kung may naka-upload na
                                Image imgTop = FindControl("imgTopAvatar") as Image;
                                Image imgDrop = FindControl("imgDropAvatar") as Image;

                                if (!string.IsNullOrEmpty(profilePic))
                                {
                                    if (imgTop != null) { imgTop.ImageUrl = profilePic; imgTop.Visible = true; }
                                    if (imgDrop != null) { imgDrop.ImageUrl = profilePic; imgDrop.Visible = true; }

                                    if (litAvatarInitial != null) litAvatarInitial.Visible = false;
                                    if (litAvatarInitialLg != null) litAvatarInitialLg.Visible = false;
                                }
                                else
                                {
                                    if (imgTop != null) imgTop.Visible = false;
                                    if (imgDrop != null) imgDrop.Visible = false;

                                    if (litAvatarInitial != null) litAvatarInitial.Visible = true;
                                    if (litAvatarInitialLg != null) litAvatarInitialLg.Visible = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadUserData Error: " + ex.Message);
            }
        }

        // Lists this customer's bookings, newest first. "HasRating" (1/0) tells the page whether to show the
        // "Rate" button, so a booking can't be rated twice.
        private void BindCustomerBookings(int customerID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // The query below reads the BookingActivities table, so make sure it exists first
                    conn.Open();
                    BookingActivities.EnsureSchema(conn);

                    string query = @"SELECT b.BookingID,
                                            uComp.FullName AS CompanionName, 
                                            p.PackageName, p.Duration, 
                                            b.BookingDate, 
                                            b.BookingTime, 
                                            b.Status,
                                            " + BookingActivities.JoinedActivitiesSql + @" AS Activities,
                                            CASE WHEN EXISTS (
                                                SELECT 1
                                                FROM Ratings r
                                                WHERE r.BookingID = b.BookingID
                                                  AND r.CustomerID = b.CustomerID
                                            ) THEN 1 ELSE 0 END AS HasRating
                                     FROM Bookings b
                                     INNER JOIN CompanionProfiles cp ON b.CompanionID = cp.CompanionID
                                     INNER JOIN Users uComp ON cp.UserID = uComp.UserID
                                     INNER JOIN Packages p ON b.PackageID = p.PackageID
                                     WHERE b.CustomerID = @CustomerID
                                     ORDER BY b.BookingID DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CustomerID", customerID);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
BookingStatus.Apply(dt);   // adds the display status (On-going, ...) worked out from the date and time

                            if (dt.Rows.Count > 0)
                            {
                                gvMyBookings.DataSource = dt;
                                gvMyBookings.DataBind();
                                gvMyBookings.Visible = true;
                                pnlEmptyBookings.Visible = false;
                            }
                            else
                            {
                                gvMyBookings.Visible = false;
                                pnlEmptyBookings.Visible = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("BindCustomerBookings Error: " + ex.Message);
                gvMyBookings.Visible = false;
                pnlEmptyBookings.Visible = true;
            }
        }

        // Cancel button on a row (CommandArgument = BookingID); refreshes the list afterwards.
        protected void gvMyBookings_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "CancelBooking")
            {
                int bookingID = Convert.ToInt32(e.CommandArgument);
                UpdateBookingStatusToCancelled(bookingID);

                int customerUserID = Convert.ToInt32(Session["UserID"]);
                BindCustomerBookings(customerUserID);
            }
        }

        // Cancels one of this customer's bookings. Allowed while it is Pending, or Confirmed and not started yet
        // (BookingStatus.CanCustomerCancel); an On-going or finished booking can no longer be cancelled.
        // The UPDATE repeats the status check, so if the companion changed the booking in the meantime
        // (for example just declined it) nothing is changed and the customer is told. The companion is notified.
        private void UpdateBookingStatusToCancelled(int bookingID)
        {
            try
            {
                int customerID = Convert.ToInt32(Session["UserID"]);
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    using (SqlTransaction tx = conn.BeginTransaction())
                    {
                        // 1. Read the booking (only if it belongs to this customer)
                        string status = null;
                        object date = null, time = null, duration = null;
                        string packageName = string.Empty, customerName = string.Empty;
                        int companionUserID = 0;

                        const string readQuery = @"
                            SELECT b.Status, b.BookingDate, b.BookingTime, p.Duration, p.PackageName, cu.FullName, cp.UserID
                            FROM Bookings b
                            INNER JOIN Packages p ON b.PackageID = p.PackageID
                            INNER JOIN CompanionProfiles cp ON b.CompanionID = cp.CompanionID
                            INNER JOIN Users cu ON cu.UserID = b.CustomerID
                            WHERE b.BookingID = @BookingID AND b.CustomerID = @CustomerID";
                        using (SqlCommand read = new SqlCommand(readQuery, conn, tx))
                        {
                            read.Parameters.AddWithValue("@BookingID", bookingID);
                            read.Parameters.AddWithValue("@CustomerID", customerID);
                            using (SqlDataReader r = read.ExecuteReader())
                            {
                                if (r.Read())
                                {
                                    status = Convert.ToString(r["Status"]);
                                    date = r["BookingDate"];
                                    time = r["BookingTime"];
                                    duration = r["Duration"];
                                    packageName = Convert.ToString(r["PackageName"]);
                                    customerName = Convert.ToString(r["FullName"]);
                                    companionUserID = Convert.ToInt32(r["UserID"]);
                                }
                            }
                        }

                        if (status == null || !BookingStatus.CanCustomerCancel(status, date, time, duration, DateTime.Now))
                        {
                            tx.Rollback();
                            lblMessage.Text = "This booking can no longer be cancelled. It may have already started, finished, or been declined.";
                            lblMessage.Visible = true;
                            return;
                        }

                        // 2. Cancel it (the status in the WHERE clause must still be the one we just read)
                        const string updateQuery = @"UPDATE Bookings SET Status = 'Cancelled'
                                                     WHERE BookingID = @BookingID AND CustomerID = @CustomerID AND Status = @OldStatus";
                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@BookingID", bookingID);
                            cmd.Parameters.AddWithValue("@CustomerID", customerID);
                            cmd.Parameters.AddWithValue("@OldStatus", status);
                            if (cmd.ExecuteNonQuery() != 1)
                            {
                                tx.Rollback();
                                lblMessage.Text = "This booking can no longer be cancelled. Its status has just changed.";
                                lblMessage.Visible = true;
                                return;
                            }
                        }

                        // 3. Tell the companion, so a cancelled booking does not just disappear from their list
                        string when = date == null || date == DBNull.Value ? string.Empty : Convert.ToDateTime(date).ToString("MMM dd, yyyy") + " at " + FormatTimeText(time);
                        string message = customerName + " cancelled the " + (status == "Confirmed" ? "confirmed " : "") +
                                         "booking for " + packageName + (when.Length > 0 ? " on " + when : "") + ".";
                        const string notifyQuery = @"INSERT INTO Notifications (UserID, Message, IsRead, DateCreated, RelatedBookingID)
                                                     VALUES (@UserID, @Message, 0, GETDATE(), @BookingID)";
                        using (SqlCommand notify = new SqlCommand(notifyQuery, conn, tx))
                        {
                            notify.Parameters.AddWithValue("@UserID", companionUserID);
                            notify.Parameters.AddWithValue("@Message", message);
                            notify.Parameters.AddWithValue("@BookingID", bookingID);
                            notify.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                }
                lblMessage.Text = "Your booking has been successfully cancelled.";
                lblMessage.Visible = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateBookingStatusToCancelled Error: " + ex.Message);
                lblMessage.Text = "Something went wrong while cancelling your booking. Please try again.";
                lblMessage.Visible = true;
            }
        }

        // "15:00" from a TIME column (TimeSpan) or text
        private static string FormatTimeText(object time)
        {
            TimeSpan ts;
            if (time is TimeSpan) return ((TimeSpan)time).ToString(@"hh\:mm");
            return TimeSpan.TryParse(Convert.ToString(time), out ts) ? ts.ToString(@"hh\:mm") : Convert.ToString(time);
        }

        // Clears the session and returns to the login page.
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("../Account/Login.aspx");
        }
    }
}