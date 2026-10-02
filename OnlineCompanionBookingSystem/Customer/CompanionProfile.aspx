<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CompanionProfile.aspx.cs" Inherits="OnlineCompanionBookingSystem.Customer.CompanionProfile" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Companion Profile - Online Companion Booking System</title>
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/CustomerCompanionProfile.css" rel="stylesheet" runat="server" />
    <!-- Animations and small layout fixes (loaded last so it adds to the page CSS) -->
    <link href="~/CSS/CustomerMotion.css?v=2" rel="stylesheet" runat="server" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="dashboard-wrapper">
            <!-- ===== SIDEBAR ===== -->
            <div class="sidebar">
                <div class="sidebar-top">
                    <div class="sidebar-logo">
                        <span class="brand-title">OCBMS</span>
                        <span class="brand-title">OCBMS</span>
                    </div>

                    <div class="sidebar-nav">
                        <asp:HyperLink ID="NavDashboard" runat="server" NavigateUrl="~/Customer/Dashboard.aspx" CssClass="sidebar-link">
                            <span>Dashboard</span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="NavCompanions" runat="server" NavigateUrl="~/Customer/BrowseCompanion.aspx" CssClass="sidebar-link active">
                            <span>Browse Companions</span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="NavBookings" runat="server" NavigateUrl="~/Customer/CustomerMyBookings.aspx" CssClass="sidebar-link">
                            <span>My Bookings</span>
                            <span class="nav-count" data-nav-count="bookings" hidden></span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="NavNotifications" runat="server" NavigateUrl="~/Customer/CustomerNotification.aspx" CssClass="sidebar-link">
                            <span>Notifications</span>
                            <span class="nav-count" data-nav-count="notifications" hidden></span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="NavProfile" runat="server" NavigateUrl="~/Customer/CustomerProfile.aspx" CssClass="sidebar-link">
                            <span>My Profile</span>
                        </asp:HyperLink>
                    </div>
                </div>

                <div class="sidebar-footer">
                    <asp:Button ID="btnLogout" runat="server" Text="Sign Out" CssClass="logout-btn" OnClick="btnLogout_Click" OnClientClick="return confirm('Are you sure you want to log out?');" />
                </div>
            </div>

            <!-- ===== MAIN CONTENT ===== -->
            <div class="main-content">
                <!-- TOP BACK NAVIGATION -->
                <div class="back-nav-bar">
                    <asp:HyperLink ID="hlBack" runat="server" NavigateUrl="~/Customer/BrowseCompanion.aspx" CssClass="back-link">
                        &larr; Back to Browse Companions
                    </asp:HyperLink>
                </div>

                <!-- PROFILE HERO BANNER -->
                <div class="profile-hero-card">
                    <div class="profile-avatar-wrap">
                        <asp:Image ID="imgCompanionAvatar" runat="server" CssClass="profile-avatar" Visible="false" />
                        <div id="divAvatarFallback" runat="server" class="profile-avatar profile-avatar-initials">
                            <asp:Literal ID="litCompanionInitials" runat="server" Text="TT" />
                        </div>
                        <span class="verified-badge-lg" title="Verified Companion">
                            <svg class="badge-icon-svg" viewBox="0 0 24 24" aria-hidden="true"><path d="m5 12 4 4L19 6" /></svg>
                        </span>
                    </div>
                    <div class="profile-hero-info">
                        <div class="name-status-row">
                            <h1><asp:Literal ID="litFullName" runat="server" Text="Companion Name" /></h1>
                            <span class="status-pill"><asp:Literal ID="litVerificationStatus" runat="server" Text="Verified" /></span>
                        </div>
                        <p class="profile-tagline"><asp:Literal ID="litMainTag" runat="server" Text="Specialty / Activity" /></p>
                        <div class="profile-rating-bar">
                            <span class="stars"><asp:Literal ID="litStars" runat="server" Text="★★★★★" /></span>
                            <span class="rating-score"><asp:Literal ID="litAvgRating" runat="server" Text="5.0" /></span>
                            <span class="review-count">(<asp:Literal ID="litTotalReviews" runat="server" Text="0" /> customer reviews)</span>
                        </div>
                    </div>
                </div>

                <!-- CONTENT GRID LAYOUT -->
                <div class="profile-grid">
                    <!-- LEFT COLUMN: Bio, Details, Reviews -->
                    <div class="profile-left-col">
                        <!-- ABOUT SECTION -->
                        <div class="content-card">
                            <h3>About Companion</h3>
                            <p class="bio-text"><asp:Literal ID="litBio" runat="server" Text="No background information provided yet." /></p>
                        </div>

                        <!-- ACTIVITIES / EXPERTISE -->
                        <div class="content-card">
                            <h3>Offered Activities & Services</h3>
                            <div class="activities-list">
                                <!-- The customer can tick as many activities as they want to book with this companion -->
                                <asp:Panel ID="pnlActivityPicker" runat="server">
                                    <p class="activity-picker-hint">Select one or more activities you would like to book with this companion.</p>
                                    <asp:CheckBoxList ID="cblActivities" runat="server" CssClass="activity-picker" RepeatLayout="Flow" RepeatDirection="Horizontal"
                                        DataTextField="ActivityName" DataValueField="ActivityID"></asp:CheckBoxList>
                                    <p class="activity-picker-count"><span id="activityCount">None selected</span></p>
                                </asp:Panel>
                                <asp:Label ID="lblNoActivities" runat="server" Text="No specific activities listed." CssClass="empty-text" Visible="false" />
                            </div>
                        </div>

                        <!-- WEEKLY AVAILABILITY (from Companion > Availability) -->
                        <div class="content-card">
                            <h3>Weekly Availability</h3>
                            <asp:Literal ID="litAvailability" runat="server" />
                        </div>

                        <!-- CUSTOMER REVIEWS -->
                        <div class="content-card">
                            <h3>Customer Feedback & Reviews</h3>
                            <asp:Repeater ID="rptReviews" runat="server">
                                <ItemTemplate>
                                    <div class="review-item">
                                        <div class="review-header">
                                            <span class="reviewer-name"><%#: Eval("CustomerName") %></span>
                                            <span class="review-date"><%# Convert.ToDateTime(Eval("CreatedAt")).ToString("MMM dd, yyyy") %></span>
                                        </div>
                                        <div class="review-stars"><%# FormatStars(Eval("Score")) %></div>
                                        <p class="review-comment"><%#: Eval("Comment") %></p>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            <asp:Label ID="lblNoReviews" runat="server" Text="No reviews yet for this companion." CssClass="empty-text" Visible="false" />
                        </div>
                    </div>

                    <!-- RIGHT COLUMN: Packages & Booking Action Box -->
                    <div class="profile-right-col">
                        <div class="booking-box">
                            <h3>Available Packages</h3>
                            <p class="booking-box-sub">Select a package to proceed with your booking schedule.</p>
                            
                            <div class="packages-container">
                                <asp:Repeater ID="rptPackages" runat="server" OnItemCommand="rptPackages_ItemCommand">
                                    <ItemTemplate>
                                        <div class="package-card-item">
                                            <div class="pkg-info">
                                                <h4><%#: Eval("PackageName") %></h4>
                                                <p><%#: Eval("Description") %></p>
                                                <span class="pkg-duration"><%#: Eval("Duration") %></span>
                                            </div>
                                            <div class="pkg-action-row">
                                                <span class="pkg-price">&#8369;<%#: Eval("Rate", "{0:N0}") %></span>
                                                <div class="booking-schedule">
                                                    <label>Booking date</label>
                                                    <asp:TextBox ID="txtBookingDate" runat="server" TextMode="Date" CssClass="booking-input" />
                                                    <label>Time</label>
                                                    <asp:TextBox ID="txtBookingTime" runat="server" TextMode="Time" CssClass="booking-input" />
                                                    <!-- Clears this package's date and time (no page reload); only shown once something is selected -->
                                                    <button type="button" class="btn-clear-schedule" title="Clear the selected date and time" hidden>Clear</button>
                                                </div>
                                                <asp:Button ID="btnSelectPackage" runat="server" Text="Book Now" CssClass="btn-book-pkg" 
                                                    CommandName="BookPackage" CommandArgument='<%# Eval("PackageID") %>' />
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <asp:Label ID="lblNoPackages" runat="server" Text="No active packages available right now." CssClass="empty-text" Visible="false" />
                                <asp:Label ID="lblBookingMessage" runat="server" CssClass="booking-message" Visible="false" role="status" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <script>
            // "2 activities selected" counter under the activity pills
            (function () {
                var picker = document.querySelector('.activity-picker');
                var label = document.getElementById('activityCount');
                if (!picker || !label) return;
                function update() {
                    var n = picker.querySelectorAll('input:checked').length;
                    label.textContent = n === 0 ? 'None selected' : n + (n === 1 ? ' activity selected' : ' activities selected');
                    label.parentNode.classList.toggle('has-selection', n > 0);
                }
                picker.addEventListener('change', update);
                update();
            })();
        </script>
        <script>
            // "Clear" button next to each package's date and time: empties them (no reload) and shows only when something is selected
            (function () {
                function row(el) { return el.closest('.booking-schedule'); }

                function update(schedule) {
                    var inputs = schedule.querySelectorAll('.booking-input');
                    var clear = schedule.querySelector('.btn-clear-schedule');
                    var hasValue = false;
                    for (var i = 0; i < inputs.length; i++) if (inputs[i].value) hasValue = true;
                    if (clear) clear.hidden = !hasValue;
                }

                document.addEventListener('input', function (e) {
                    if (e.target.classList && e.target.classList.contains('booking-input')) update(row(e.target));
                });

                document.addEventListener('click', function (e) {
                    var button = e.target.closest ? e.target.closest('.btn-clear-schedule') : null;
                    if (!button) return;
                    var schedule = row(button);
                    var inputs = schedule.querySelectorAll('.booking-input');
                    for (var i = 0; i < inputs.length; i++) inputs[i].value = '';
                    update(schedule);
                    // Hide an old "please choose..." message, since the selection it talked about is gone
                    var message = document.getElementById('<%= lblBookingMessage.ClientID %>');
                    if (message) message.style.display = 'none';
                });

                // Fields keep their values after a failed booking attempt, so show Clear for those as well
                var schedules = document.querySelectorAll('.booking-schedule');
                for (var i = 0; i < schedules.length; i++) update(schedules[i]);
            })();
        </script>
        <script src="../Scripts/nav-counts.js"></script>
    </form>
</body>
</html>