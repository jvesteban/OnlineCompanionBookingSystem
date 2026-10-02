<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RateCompanion.aspx.cs" Inherits="OnlineCompanionBookingManagementSystem.Customer.RateCompanion" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Rate Companion - Online Companion Booking System</title>
    <!-- Shared customer layout styles are loaded before the page-specific styles. -->
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/CustomerDashboard.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/RateCompanion.css" rel="stylesheet" runat="server" />
    <!-- Animations and small layout fixes (loaded last so it adds to the page CSS) -->
    <link href="~/CSS/CustomerMotion.css?v=2" rel="stylesheet" runat="server" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="app-shell">
            <aside class="sidebar">
                <!-- Customer navigation keeps the rating page within the existing dashboard experience. -->
                <div class="sidebar-brand">
                    <span class="sidebar-brand-text">OCBMS</span>
                </div>
                <nav class="sidebar-nav">
                    <asp:HyperLink ID="NavDashboard" runat="server" NavigateUrl="~/Customer/Dashboard.aspx" CssClass="sidebar-link">
                        <span>Dashboard</span>
                    </asp:HyperLink>
                    <asp:HyperLink ID="NavCompanions" runat="server" NavigateUrl="~/Customer/BrowseCompanion.aspx" CssClass="sidebar-link">
                        <span>Browse Companions</span>
                    </asp:HyperLink>
                    <asp:HyperLink ID="NavBookings" runat="server" NavigateUrl="~/Customer/CustomerMyBookings.aspx" CssClass="sidebar-link active">
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
                </nav>
            </aside>

            <main class="main-content">
                <div class="topbar">
                    <div class="topbar-left">
                        <h2>Rate Your Experience</h2>
                        <p class="topbar-subtitle">Share your feedback to help us maintain a trusted companion community.</p>
                    </div>
                    <div class="topbar-right">
                        <asp:HyperLink ID="hlNotifBell" runat="server" NavigateUrl="~/Customer/CustomerNotification.aspx" CssClass="topbar-icon-btn" title="Notifications">
                            <svg class="notification-bell-icon" viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                                <path d="M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9M10 21h4" />
                            </svg>
                        </asp:HyperLink>
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
                                    <span><svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="8" r="4" /><path d="M4 21c0-4 3-7 8-7s8 3 8 7" /></svg> View &amp; Edit Profile</span>
                                </asp:HyperLink>
                                <div class="dropdown-divider"></div>
                                <asp:Button ID="btnDropdownLogout" runat="server" Text="Log Out Securely" CssClass="dropdown-item dropdown-logout-btn" OnClick="btnLogout_Click" OnClientClick="return confirm('Are you sure you want to log out?');" />
                            </div>
                        </div>
                    </div>
                </div>

                <div class="rate-page">
                    <asp:Panel ID="pnlMessage" runat="server" CssClass="rate-message" Visible="false">
                        <asp:Literal ID="litMessage" runat="server" />
                    </asp:Panel>

                    <!-- This panel is hidden after a successful submission or when the booking is not eligible. -->
                    <asp:Panel ID="pnlRatingForm" runat="server" CssClass="rate-card">
                        <div class="rate-card-header">
                            <div class="rate-icon">
                                <svg viewBox="0 0 24 24" aria-hidden="true"><path d="m12 3 2.8 5.7 6.2.9-4.5 4.4 1.1 6.2-5.6-2.9-5.6 2.9 1.1-6.2L3 9.6l6.2-.9L12 3z" /></svg>
                            </div>
                            <div>
                                <h3>How was your experience?</h3>
                                <p>Your feedback helps other customers choose with confidence.</p>
                            </div>
                        </div>

                        <div class="booking-summary">
                            <div>
                                <span class="summary-label">Companion</span>
                                <strong><asp:Literal ID="litCompanionName" runat="server" /></strong>
                            </div>
                            <div>
                                <span class="summary-label">Package</span>
                                <strong><asp:Literal ID="litPackageName" runat="server" /></strong>
                            </div>
                            <div>
                                <span class="summary-label">Completed</span>
                                <strong><asp:Literal ID="litBookingDate" runat="server" /></strong>
                            </div>
                        </div>

                        <div class="form-section">
                            <label class="form-label">Your rating</label>
                            <!-- Server-side validation requires one score from 1 to 5. -->
                            <asp:RadioButtonList ID="rblScore" runat="server" RepeatDirection="Horizontal" CssClass="rating-options">
                                <asp:ListItem Text="1" Value="1" />
                                <asp:ListItem Text="2" Value="2" />
                                <asp:ListItem Text="3" Value="3" />
                                <asp:ListItem Text="4" Value="4" />
                                <asp:ListItem Text="5" Value="5" />
                            </asp:RadioButtonList>
                            <asp:CustomValidator ID="cvScore" runat="server" CssClass="field-error" Display="Dynamic" OnServerValidate="cvScore_ServerValidate" ErrorMessage="Please select a rating from 1 to 5." />
                        </div>

                        <div class="form-section">
                            <label class="form-label" for="txtComment">Comment <span>(optional)</span></label>
                            <asp:TextBox ID="txtComment" runat="server" TextMode="MultiLine" Rows="5" MaxLength="1000" CssClass="comment-input" placeholder="Tell us about your experience..." />
                            <small class="field-hint">Maximum 1,000 characters.</small>
                        </div>

                        <div class="rate-actions">
                            <asp:HyperLink ID="hlBack" runat="server" NavigateUrl="~/Customer/CustomerMyBookings.aspx" CssClass="btn-secondary">Back to My Bookings</asp:HyperLink>
                            <asp:Button ID="btnSubmitRating" runat="server" Text="Submit Rating" CssClass="btn-primary" OnClick="btnSubmitRating_Click" />
                        </div>
                    </asp:Panel>
                </div>
            </main>
        </div>
        <script src="../Scripts/nav-counts.js"></script>
    </form>
    <script src="<%= ResolveUrl("~/Scripts/site.js") %>"></script>
    <script type="text/javascript">
        (function () {
            var toggle = document.querySelector('#userMenuContainer .topbar-user');
            var container = document.getElementById('userMenuContainer');
            var card = document.getElementById('userDropdownCard');

            if (!toggle || !container || !card) return;

            toggle.onclick = function (event) {
                event.preventDefault();
                event.stopPropagation();
                var open = card.classList.toggle('show');
                card.style.display = open ? 'block' : 'none';
                card.setAttribute('aria-hidden', open ? 'false' : 'true');
            };

            document.addEventListener('click', function (event) {
                if (!container.contains(event.target)) {
                    card.classList.remove('show');
                    card.style.display = 'none';
                    card.setAttribute('aria-hidden', 'true');
                }
            });
        }());
    </script>
</body>
</html>
