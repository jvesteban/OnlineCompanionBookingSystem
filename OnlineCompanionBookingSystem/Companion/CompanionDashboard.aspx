<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CompanionDashboard.aspx.cs" Inherits="OnlineCompanionBookingSystem.Companion.CompanionDashboard" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <title>Companion Dashboard - Online Companion Booking System</title>
    <!-- Shared site styles are loaded before the Companion-specific dashboard styles. -->
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/CompanionDashboard.css?v=2.5" rel="stylesheet" runat="server" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="dashboard-wrapper">
            <!-- Companion navigation links to the planned portal modules. -->
            <aside class="sidebar">
                <div class="sidebar-top">
                    <div class="sidebar-logo">
                        <span class="brand-title">OCBMS</span>
                        <span class="brand-title">OCBMS</span>
                    </div>
                    <nav class="sidebar-nav">
                        <asp:HyperLink ID="navDashboard" runat="server" NavigateUrl="~/Companion/CompanionDashboard.aspx" CssClass="sidebar-link active">
                            <span>Dashboard</span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="navActivities" runat="server" NavigateUrl="~/Companion/CompanionActivity.aspx" CssClass="sidebar-link">
                            <span>Activities</span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="navPackages" runat="server" NavigateUrl="~/Companion/CompanionPackages.aspx" CssClass="sidebar-link">
                            <span>Packages</span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="navAvailability" runat="server" NavigateUrl="~/Companion/CompanionAvailability.aspx" CssClass="sidebar-link">
                            <span>Availability</span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="navBookingRequests" runat="server" NavigateUrl="~/Companion/CompanionBookingRequest.aspx" CssClass="sidebar-link">
                            <span>Booking Requests</span>
                            <span class="nav-count" data-nav-count="requests" hidden></span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="navMyBookings" runat="server" NavigateUrl="~/Companion/CompanionMybookings.aspx" CssClass="sidebar-link">
                            <span>My Bookings</span>
                            <span class="nav-count" data-nav-count="bookings" hidden></span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="navNotifications" runat="server" NavigateUrl="~/Companion/CompanionNotifications.aspx" CssClass="sidebar-link">
                            <span>Notifications</span>
                            <span class="nav-count" data-nav-count="notifications" hidden></span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="navProfile" runat="server" NavigateUrl="~/Companion/CompanionProfile.aspx" CssClass="sidebar-link">
                            <span>My Profile</span>
                        </asp:HyperLink>
                    </nav>
                </div>
                <div class="sidebar-footer">
                    <asp:Button ID="btnLogout" runat="server" Text="Sign Out" CssClass="logout-btn" OnClick="btnLogout_Click" OnClientClick="return confirm('Are you sure you want to log out?');" />
                </div>
            </aside>

            <main class="main-content">
                <!-- Header shows the logged-in companion and notification shortcut. -->
                <header class="topbar">
                    <div class="topbar-left">
                        <h2>Companion Dashboard</h2>
                        <p class="topbar-subtitle">Manage your companion services and booking activity.</p>
                    </div>
                    <div class="topbar-right">
                        <asp:HyperLink ID="hlNotifications" runat="server" NavigateUrl="~/Companion/CompanionNotifications.aspx" CssClass="topbar-icon-btn" title="Notifications">
                            <svg class="notification-bell-icon" viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                                <path d="M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9M10 21h4" />
                            </svg>
                        </asp:HyperLink>
                        <div class="user-menu-container companion-account-menu" id="companionAccountMenu">
                        <div class="topbar-user" onclick="toggleCompanionAccount(event)" role="button" tabindex="0" aria-haspopup="true" aria-expanded="false">
                            <div class="user-avatar">
                                <asp:Image ID="imgAvatar" runat="server" CssClass="user-avatar-image" Visible="false" />
                                <asp:Literal ID="litAvatarInitial" runat="server" Text="C" />
                            </div>
                            <div class="user-meta">
                                <span class="user-name"><asp:Literal ID="litUserFirstName" runat="server" Text="Companion" /></span>
                                <span class="user-role">Companion Account</span>
                            </div>
                                <span class="dropdown-arrow">&#9662;</span>
                            </div>
                            <div class="user-dropdown-card companion-account-dropdown" id="companionAccountDropdown" aria-hidden="true">
                                <div class="dropdown-header">
                                    <div class="dropdown-avatar-lg">
                                        <asp:Image ID="imgDropAvatar" runat="server" CssClass="dropdown-avatar-img" Visible="false" AlternateText="Companion profile photo" />
                                        <asp:Literal ID="litAvatarInitialLg" runat="server" Text="C" />
                                    </div>
                                    <div class="dropdown-user-info">
                                        <p class="dropdown-fullname"><asp:Literal ID="litAccountName" runat="server" Text="Companion" /></p>
                                        <p class="dropdown-email"><asp:Literal ID="litAccountEmail" runat="server" Text="companion@email.com" /></p>
                                    </div>
                                </div>
                                <div class="dropdown-divider"></div>
                                <a class="dropdown-item" href="CompanionProfile.aspx"><span><svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="8" r="4" /><path d="M4 21c0-4 3-7 8-7s8 3 8 7" /></svg> View &amp; Edit Profile</span></a>
                                <div class="dropdown-divider"></div>
                                <asp:Button ID="btnDropdownLogout" runat="server" Text="Log Out Securely" CssClass="dropdown-item dropdown-logout-btn" OnClick="btnLogout_Click" CausesValidation="false" OnClientClick="return confirm('Are you sure you want to log out?');" />
                            </div>
                            </div>
                    </div>
                </header>

                <div class="dashboard-content">
                    <!-- Profile verification and companion identity summary. -->
                    <section class="welcome-banner">
                        <div>
                            <p class="eyebrow">COMPANION PORTAL</p>
                            <h1>Welcome, <asp:Literal ID="litWelcomeName" runat="server" Text="Companion" />.</h1>
                            <p>Stay on top of your schedules, requests, and service performance.</p>
                        </div>
                        <div class="verification-summary">
                            <span class="summary-label">Account status</span>
                            <strong><asp:Literal ID="litVerificationStatus" runat="server" Text="Pending" /></strong>
                        </div>
                    </section>

                    <!-- Statistics are populated from the companion's booking and rating records. -->
                    <section class="stats-grid" aria-label="Companion booking statistics">
                        <div class="stat-card">
                            <div class="stat-icon icon-blue"><svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="5" width="18" height="16" rx="2" /><path d="M16 3v4M8 3v4M3 10h18" /></svg></div>
                            <div><h3><asp:Literal ID="litTotalBookings" runat="server" Text="0" /></h3><p>Total Bookings</p></div>
                        </div>
                        <div class="stat-card">
                            <div class="stat-icon icon-orange"><svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><path d="M7 3h10M7 21h10M8 3c0 4 4 5 4 9s-4 5-4 9M16 3c0 4-4 5-4 9s4 5 4 9" /></svg></div>
                            <div><h3><asp:Literal ID="litPendingBookings" runat="server" Text="0" /></h3><p>Pending Requests</p></div>
                        </div>
                        <div class="stat-card">
                            <div class="stat-icon icon-green"><svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><path d="m5 12 4 4L19 6" /></svg></div>
                            <div><h3><asp:Literal ID="litCompletedBookings" runat="server" Text="0" /></h3><p>Completed Services</p></div>
                        </div>
                        <div class="stat-card">
                            <div class="stat-icon icon-purple"><svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><path d="m12 3 2.8 5.7 6.2.9-4.5 4.4 1.1 6.2-5.6-2.9-5.6 2.9 1.1-6.2L3 9.6l6.2-.9L12 3z" /></svg></div>
                            <div><h3><asp:Literal ID="litAverageRating" runat="server" Text="0.0" /></h3><p>Average Rating</p></div>
                        </div>
                    </section>

                    <!-- Recent requests are displayed from the companion's latest bookings. -->
                    <section class="content-card" id="notifications">
                        <div class="section-header">
                            <div><h3>Recent Booking Requests</h3><p>Review your latest customer activity.</p></div>
                            <span class="notification-count"><asp:Literal ID="litUnreadNotifications" runat="server" Text="0" /> unread notifications</span>
                        </div>
                        <asp:Repeater ID="rptRecentBookings" runat="server">
                            <HeaderTemplate><div class="booking-list"></HeaderTemplate>
                            <ItemTemplate>
                                <div class="booking-row">
                                    <div class="booking-avatar">
                                        <asp:Image ID="imgCustomer" runat="server" ImageUrl='<%# Eval("CustomerProfilePicture") %>' Visible='<%# Eval("CustomerProfilePicture") != DBNull.Value && !string.IsNullOrWhiteSpace(Eval("CustomerProfilePicture").ToString()) %>' CssClass="dashboard-customer-img" />
                                        <asp:Literal ID="litCustInitials" runat="server" Text='<%# GetInitials(Eval("CustomerName")) %>' Visible='<%# Eval("CustomerProfilePicture") == DBNull.Value || string.IsNullOrWhiteSpace(Eval("CustomerProfilePicture").ToString()) %>' />
                                    </div>
                                    <div class="booking-details">
                                        <strong><%# Server.HtmlEncode(Eval("CustomerName").ToString()) %></strong>
                                        <span><%# Server.HtmlEncode(Eval("PackageName").ToString()) %> &middot; <%# Eval("BookingDate", "{0:MMM. dd, yyyy}") %></span>
                                    </div>
                                    <span class="status-badge <%# GetStatusClass(Eval("StatusKey")) %>" data-booking-start="<%#: Eval("StartIso") %>" data-booking-end="<%#: Eval("EndIso") %>" data-now="<%#: Eval("ServerNowIso") %>" data-ongoing-note="<%#: Eval("OngoingNote") %>" data-awaiting-note="<%#: Eval("AwaitingNote") %>"BookingID") %>"><%#: Eval("DisplayStatus") %></span>
                                </div>
                            </ItemTemplate>
                            <FooterTemplate></div></FooterTemplate>
                        </asp:Repeater>
                        <asp:Label ID="lblNoBookings" runat="server" CssClass="empty-state" Text="No booking activity is available yet." Visible="false" />
                    </section>

                    <section class="content-card companion-reviews-card">
                        <div class="section-header">
                            <div><h3>Customer Reviews</h3><p>See the ratings and feedback customers leave after completed bookings.</p></div>
                        </div>
                        <asp:Repeater ID="rptRecentRatings" runat="server">
                            <HeaderTemplate><div class="review-list"></HeaderTemplate>
                            <ItemTemplate>
                                <article class="review-row">
                                    <div class="review-avatar"><%# GetInitials(Eval("CustomerName")) %></div>
                                    <div class="review-content">
                                        <div class="review-heading">
                                            <strong><%# Server.HtmlEncode(Eval("CustomerName").ToString()) %></strong>
                                            <span class="review-score" title="<%# Eval("Score") %> out of 5"><%# GetStarsHtml(Eval("Score")) %><span class="review-score-num"><%# Eval("Score") %>/5</span></span>
                                        </div>
                                        <time><%# Eval("DateCreated", "{0:MMM dd, yyyy}") %></time>
                                        <p class="review-comment"><%# Eval("Comment") == DBNull.Value || string.IsNullOrWhiteSpace(Eval("Comment").ToString()) ? "<em>No written comment provided.</em>" : Server.HtmlEncode(Eval("Comment").ToString()) %></p>
                                    </div>
                                </article>
                            </ItemTemplate>
                            <FooterTemplate></div></FooterTemplate>
                        </asp:Repeater>
                        <asp:Label ID="lblNoRatings" runat="server" CssClass="empty-state" Text="No customer reviews yet." Visible="false" />
                    </section>
                </div>
            </main>
        </div>
        <script src="../Scripts/site.js"></script>
        <script src="../Scripts/nav-counts.js"></script>
        <script src="../Scripts/booking-status.js"></script>
    </form>
</body>
</html>