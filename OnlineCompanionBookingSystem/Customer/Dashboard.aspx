<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="OnlineCompanionBookingSystem.Customer.Dashboard" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Customer Dashboard - Online Companion Booking System</title>
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/CustomerDashboard.css" rel="stylesheet" runat="server" />
    <!-- Animations and small layout fixes (loaded last so it adds to the page CSS) -->
    <link href="~/CSS/CustomerMotion.css?v=2" rel="stylesheet" runat="server" />
</head>
<body>
    <form id="form1" runat="server">
        <!-- Kailangan ang ScriptManager para sa UpdatePanel at Modal Popup -->
        <asp:ScriptManager ID="ScriptManager1" runat="server" />

        <div class="dashboard-wrapper">

            <!-- ===== SIDEBAR ===== -->
            <div class="sidebar">
                <div class="sidebar-top">
                    <div class="sidebar-logo">
                        <span class="brand-title">OCBMS</span>
                        <span class="brand-title">OCBMS</span>
                    </div>

                    <div class="sidebar-nav">
                        <asp:HyperLink ID="NavDashboard" runat="server" NavigateUrl="~/Customer/Dashboard.aspx" CssClass="sidebar-link active">
                            <span>Dashboard</span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="NavCompanions" runat="server" NavigateUrl="~/Customer/BrowseCompanion.aspx" CssClass="sidebar-link">
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

                <!-- ===== TOPBAR ===== -->
                <div class="topbar">
                    <div class="topbar-left">
                        <h2>Dashboard Overview</h2>
                        <p class="topbar-subtitle">Welcome back, manage your bookings and activities seamlessly.</p>
                    </div>

                    <div class="topbar-right">
                        <asp:HyperLink ID="hlNotifBell" runat="server" NavigateUrl="~/Customer/CustomerNotification.aspx" CssClass="topbar-icon-btn" title="Notifications">
                            <svg class="notification-bell-icon" viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                                <path d="M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9M10 21h4" />
                            </svg>
                        </asp:HyperLink>

                        <!-- Professional User Dropdown Menu Wrapper -->
                        <div class="user-menu-container" id="userMenuContainer">
                            <div class="topbar-user" onclick="toggleUserDropdown(event)">
                                <div class="user-avatar">
                                    <asp:Image ID="imgTopAvatar" runat="server" CssClass="topbar-avatar-img" Visible="false" />
                                    <asp:Literal ID="litAvatarInitial" runat="server" Text="U" />
                                </div>
                                <div class="user-meta">
                                    <span class="user-name"><asp:Literal ID="litUserFirstName" runat="server" Text="User" /></span>
                                    <span class="user-role">Customer Account</span>
                                </div>
                                <span class="dropdown-arrow">&#9662;</span>
                            </div>

                            <!-- Dropdown Card -->
                            <div class="user-dropdown-card" id="userDropdownCard">
                                <div class="dropdown-header">
                                    <div class="dropdown-avatar-lg">
                                        <asp:Image ID="imgDropAvatar" runat="server" CssClass="dropdown-avatar-img" Visible="false" />
                                        <asp:Literal ID="litAvatarInitialLg" runat="server" Text="U" />
                                    </div>
                                    <div class="dropdown-user-info">
                                        <p class="dropdown-fullname"><asp:Literal ID="litDropdownName" runat="server" Text="Customer Name" /></p>
                                        <p class="dropdown-email"><asp:Literal ID="litDropdownEmail" runat="server" Text="customer@email.com" /></p>
                                    </div>
                                </div>
                                <div class="dropdown-divider"></div>
                                <asp:HyperLink ID="hlProfileLink" runat="server" NavigateUrl="~/Customer/CustomerProfile.aspx" CssClass="dropdown-item">
                                    <span><svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="8" r="4" /><path d="M4 21c0-4 3-7 8-7s8 3 8 7" /></svg> View & Edit Profile</span>
                                </asp:HyperLink>
                                <div class="dropdown-divider"></div>
                                <asp:Button ID="btnDropdownLogout" runat="server" Text="Log Out Securely" CssClass="dropdown-item dropdown-logout-btn" OnClick="btnLogout_Click" OnClientClick="return confirm('Are you sure you want to log out?');" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- ===== WELCOME BANNER ===== -->
                <div class="welcome-banner">
                    <div class="welcome-text">
                        <h1>Hello, <asp:Literal ID="litWelcomeName" runat="server" Text="User" />.</h1>
                        <p>Your portal for secure companion bookings and reliable activity scheduling.</p>
                        <div class="quick-actions-bar">
                            <asp:Button ID="btnBrowseNow" runat="server" Text="Browse Companions" CssClass="welcome-btn primary" OnClick="btnBrowseNow_Click" />
                            <asp:HyperLink ID="hlMyBookings" runat="server" NavigateUrl="~/Customer/CustomerMyBookings.aspx" CssClass="welcome-btn secondary">
                                View Schedule
                            </asp:HyperLink>
                        </div>
                    </div>
                </div>

                <!-- ===== QUICK STATS ===== -->
                <div class="stats-grid">
                    <div class="stat-card">
                        <div class="stat-icon icon-blue"><svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="5" width="18" height="16" rx="2" /><path d="M16 3v4M8 3v4M3 10h18" /></svg></div>
                        <div>
                            <h3><asp:Literal ID="litTotalBookings" runat="server" Text="0" /></h3>
                            <p>Total Bookings</p>
                        </div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-icon icon-orange"><svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><path d="M7 3h10M7 21h10M8 3c0 4 4 5 4 9s-4 5-4 9M16 3c0 4-4 5-4 9s4 5 4 9" /></svg></div>
                        <div>
                            <h3><asp:Literal ID="litUpcomingBookings" runat="server" Text="0" /></h3>
                            <p>Upcoming Schedules</p>
                        </div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-icon icon-green"><svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><path d="m5 12 4 4L19 6" /></svg></div>
                        <div>
                            <h3><asp:Literal ID="litCompletedBookings" runat="server" Text="0" /></h3>
                            <p>Completed Activities</p>
                        </div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-icon icon-purple">
                            <svg class="notification-bell-icon" viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                                <path d="M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9M10 21h4" />
                            </svg>
                        </div>
                        <div>
                            <h3><asp:Literal ID="litUnreadNotifs" runat="server" Text="0" /></h3>
                            <p>Notifications</p>
                        </div>
                    </div>
                </div>

                <!-- ===== UPCOMING BOOKING ===== -->
                <div class="section-block">
                    <div class="section-header">
                        <h3 class="section-title">Next Upcoming Schedule</h3>
                        <asp:HyperLink ID="hlViewAllBookings" runat="server" NavigateUrl="~/Customer/CustomerMyBookings.aspx" CssClass="section-link">
                            View All Bookings &rarr;
                        </asp:HyperLink>
                    </div>
                    
                    <asp:Panel ID="pnlHasUpcoming" runat="server">
                        <div class="upcoming-card">
                            <asp:Literal ID="litUpcomingCompanionAvatar" runat="server" />
                            <div class="upcoming-details">
                                <h4><asp:Literal ID="litUpcomingCompanionName" runat="server" /></h4>
                                <p class="upcoming-activity"><span>Package:</span> <asp:Literal ID="litUpcomingPackage" runat="server" /></p>
                                <p class="upcoming-datetime"><span>Schedule:</span> <asp:Literal ID="litUpcomingDateTime" runat="server" /></p>
                            </div>
                            <div class="upcoming-status">
                                <span class="status-badge confirmed"><asp:Literal ID="litUpcomingStatus" runat="server" Text="Confirmed" /></span>
                                <asp:Button ID="btnViewBooking" runat="server" Text="View Details" CssClass="btn-action-primary" OnClick="btnViewBooking_Click" />
                            </div>
                        </div>
                    </asp:Panel>

                    <asp:Panel ID="pnlNoUpcoming" runat="server" Visible="false">
                        <div class="empty-upcoming-card">
                            <h4>No upcoming bookings scheduled</h4>
                            <p>You have no active companion bookings right now. Explore available companions to plan your next activity.</p>
                            <asp:HyperLink ID="hlBookCompanionEmpty" runat="server" NavigateUrl="~/Customer/BrowseCompanion.aspx" CssClass="btn-action-primary">
                                Find a Companion
                            </asp:HyperLink>
                        </div>
                    </asp:Panel>
                </div>

                <!-- ===== RECOMMENDED COMPANIONS ===== -->
                <div class="section-block">
                    <div class="section-header">
                        <h3 class="section-title">Recommended Companions</h3>
                        <asp:HyperLink ID="hlBrowseAll" runat="server" NavigateUrl="~/Customer/BrowseCompanion.aspx" CssClass="section-link">
                            Explore All &rarr;
                        </asp:HyperLink>
                    </div>

                    <!-- UpdatePanel para sa pagbukas ng modal ng walang buong page reload -->
                    <asp:UpdatePanel ID="upRecommended" runat="server">
                        <ContentTemplate>
                            <div class="companion-cards-grid">
                                <asp:Repeater ID="rptRecommended" runat="server" OnItemCommand="rptRecommended_ItemCommand">
                                    <ItemTemplate>
                                        <div class="companion-card">
                                            <div class="companion-img-wrap">
                                                <%# GetCompanionCardAvatar(Eval("ProfilePicturePath"), Eval("FullName")) %>
                                                <span class="verified-badge" title="Verified Companion"><svg class="badge-icon-svg" viewBox="0 0 24 24" aria-hidden="true"><path d="m5 12 4 4L19 6" /></svg></span>
                                            </div>
                                            <div class="companion-body">
                                                <h4><%#: Eval("FullName") %></h4>
                                                <div class="rating-bar">
                                                    <span class="stars"><%# FormatStars(Eval("AvgRating")) %></span> 
                                                    <span class="review-count">(<%#: Eval("TotalReviews") %> reviews)</span>
                                                </div>
                                                <p class="companion-tag"><%#: Eval("MainTag") %></p>
                                                <div class="card-footer">
                                                    <span class="companion-price">&#8369;<%#: Eval("MinRate", "{0:N0}") %> <small>/ hr</small></span>
                                                    <asp:Button ID="btnViewProfile" runat="server" Text="View Profile" CssClass="btn-card-view" 
                                                        CommandName="ViewProfile" CommandArgument='<%# Eval("CompanionID") %>' />
                                                </div>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>

                            <!-- COMPANION DETAILS MODAL (Nakalagay sa loob ng UpdatePanel) -->
                            <asp:Panel ID="pnlCompanionModal" runat="server" CssClass="companion-modal-overlay" Visible="false">
                                <div class="companion-modal-card">
                                    <div class="modal-header-box">
                                        <h3>Companion Information</h3>
                                        <asp:Button ID="btnCloseModal" runat="server" Text="&times;" CssClass="modal-close-btn" OnClick="btnCloseModal_Click" />
                                    </div>
                                    <div class="modal-body-box">
                                        <div class="modal-profile-header">
                                            <asp:Literal ID="litModalProfileAvatar" runat="server" />
                                            <div>
                                                <h4 class="modal-name"><asp:Literal ID="litModalFullName" runat="server" /></h4>
                                                <p class="modal-tag"><span>Specialty:</span> <asp:Literal ID="litModalTag" runat="server" /></p>
                                                <p class="modal-rating">Rating: <asp:Literal ID="litModalRating" runat="server" /></p>
                                            </div>
                                        </div>
                                        <div class="modal-info-group">
                                            <label>Bio / Description:</label>
                                            <p class="modal-text-box"><asp:Literal ID="litModalBio" runat="server" Text="No bio provided." /></p>
                                        </div>
                                        <div class="modal-info-group">
                                            <label>Starting Rate:</label>
                                            <p class="modal-rate-val">&#8369;<asp:Literal ID="litModalRate" runat="server" /> / hr</p>
                                        </div>
                                    </div>
                                    <div class="modal-footer-box">
                                        <asp:HyperLink ID="hlBookThisCompanion" runat="server" CssClass="btn-action-primary" Text="Book This Companion" />
                                        <asp:Button ID="btnModalCloseAction" runat="server" Text="Close" CssClass="btn-card-view" OnClick="btnCloseModal_Click" />
                                    </div>
                                </div>
                            </asp:Panel>
                        </ContentTemplate>
                    </asp:UpdatePanel>

                </div>

            </div>
        </div>

        <div id="toast" class="toast"></div>

        <!-- Script para sa Topbar Dropdown Toggle -->
        <script type="text/javascript">
            function toggleUserDropdown(event) {
                event.stopPropagation();
                var card = document.getElementById('userDropdownCard');
                card.classList.toggle('show');
            }

            window.addEventListener('click', function () {
                var card = document.getElementById('userDropdownCard');
                if (card && card.classList.contains('show')) {
                    card.classList.remove('show');
                }
            });
        </script>
        <script src="../Scripts/site.js"></script>

        <script src="../Scripts/nav-counts.js"></script>
        <script src="../Scripts/ui-motion.js"></script>
    </form>
</body>
</html>