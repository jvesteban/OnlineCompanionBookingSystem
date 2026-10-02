<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CustomerNotification.aspx.cs" Inherits="OnlineCompanionBookingManagementSystem.Customer.Notifications" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Notifications - Online Companion Booking System</title>
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/CustomerDashboard.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/CustomerNotification.css" rel="stylesheet" type="text/css" />
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
                    </div>

                    <div class="sidebar-nav">
                        <asp:HyperLink ID="NavDashboard" runat="server" NavigateUrl="~/Customer/Dashboard.aspx" CssClass="sidebar-link">
                            <span>Dashboard</span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="NavCompanions" runat="server" NavigateUrl="~/Customer/BrowseCompanion.aspx" CssClass="sidebar-link">
                            <span>Browse Companions</span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="NavBookings" runat="server" NavigateUrl="~/Customer/CustomerMyBookings.aspx" CssClass="sidebar-link">
                            <span>My Bookings</span>
                            <span class="nav-count" data-nav-count="bookings" hidden></span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="NavNotifications" runat="server" NavigateUrl="~/Customer/CustomerNotification.aspx" CssClass="sidebar-link active">
                            <span>Notifications</span>
                            <span class="nav-count" data-nav-count="notifications" hidden></span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="NavProfile" runat="server" NavigateUrl="~/Customer/CustomerProfile.aspx" CssClass="sidebar-link">
                            <span>My Profile</span>
                        </asp:HyperLink>
                    </div>
                </div>

                <div class="sidebar-footer">
                    <asp:Button ID="btnLogout" runat="server" Text="Sign Out" CssClass="logout-btn" OnClick="btnLogout_Click" OnClientClick="return confirm('Are you sure you want to log out?');" CausesValidation="false" />
                </div>
            </div>

            <!-- ===== MAIN CONTENT ===== -->
            <div class="main-content">

                <!-- ===== TOPBAR ===== -->
                <div class="topbar">
                    <div class="topbar-left">
                        <h2>Notifications</h2>
                        <p class="topbar-subtitle">Stay updated with your booking statuses and platform messages.</p>
                    </div>
                    
                    <div class="topbar-right">
                        <asp:HyperLink ID="hlNotifBell" runat="server" NavigateUrl="~/Customer/CustomerNotification.aspx" CssClass="topbar-icon-btn active-bell" title="Notifications">
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
                                <asp:HyperLink ID="hlSettingsLink" runat="server" NavigateUrl="~/Customer/CustomerProfile.aspx" CssClass="dropdown-item">
                                    <span><svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="3" /><path d="M19.4 15a1.7 1.7 0 0 0 .3 1.9l.1.1-1.8 1.8-.1-.1a1.7 1.7 0 0 0-1.9-.3 1.7 1.7 0 0 0-1 1.5v.1h-2.6v-.1a1.7 1.7 0 0 0-1-1.5 1.7 1.7 0 0 0-1.9.3l-.1.1-1.8-1.8.1-.1a1.7 1.7 0 0 0 .3-1.9 1.7 1.7 0 0 0-1.5-1H6.4v-2.6h.1a1.7 1.7 0 0 0 1.5-1 1.7 1.7 0 0 0-.3-1.9l-.1-.1 1.8-1.8.1.1a1.7 1.7 0 0 0 1.9.3 1.7 1.7 0 0 0 1-1.5V5h2.6v.1a1.7 1.7 0 0 0 1 1.5 1.7 1.7 0 0 0 1.9-.3l.1-.1 1.8 1.8-.1.1a1.7 1.7 0 0 0-.3 1.9 1.7 1.7 0 0 0 1.5 1h.1v2.6h-.1a1.7 1.7 0 0 0-1.5 1z" /></svg> Account Settings</span>
                                </asp:HyperLink>
                                <div class="dropdown-divider"></div>
                                <asp:Button ID="btnDropdownLogout" runat="server" Text="Log Out Securely" CssClass="dropdown-item dropdown-logout-btn" OnClick="btnLogout_Click" OnClientClick="return confirm('Are you sure you want to log out?');" CausesValidation="false" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- ===== NOTIFICATIONS LIST CONTAINER ===== -->
                <div class="section-block">
                    <div class="notif-actions-bar">
                        <asp:Button ID="btnMarkAllRead" runat="server" Text="Mark all as read" CssClass="btn-mark-read" OnClick="btnMarkAllRead_Click" CausesValidation="false" />
                    </div>

                    <div class="notifications-list">
                        <asp:Repeater ID="rptNotifications" runat="server">
                            <ItemTemplate>
                                <div class='<%# Convert.ToBoolean(Eval("IsRead")) ? "notif-card" : "notif-card unread" %>'>
                                    <div class="notif-icon"><svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><path d="M4 11v2h3l8 4V7l-8 4H4z" /><path d="M19 9a4 4 0 0 1 0 6M7 13l1 6h3l-1-5" /></svg></div>
                                    <div class="notif-content">
                                        <p><%# GetNotificationMessage(Eval("Message"), Eval("RelatedBookingID")) %></p>
                                        <span class="notif-date"><%# Convert.ToDateTime(Eval("DateCreated")).ToString("MMM. dd, yyyy • hh:mm tt") %></span>
                                        <button type="button" class="notif-action notif-details-button"
                                            style='<%# HasConfirmedBooking(Eval("RelatedBookingID")) ? "" : "display:none;" %>'
                                            data-customer-name='<%# GetAttributeValue(Eval("CustomerName"), "Customer") %>'
                                            data-customer-email='<%# GetAttributeValue(Eval("CustomerEmail"), "Not provided") %>'
                                            data-customer-contact='<%# GetAttributeValue(Eval("CustomerContact"), "Not provided") %>'
                                            data-companion-name='<%# GetAttributeValue(Eval("CompanionName"), "Companion") %>'
                                            data-companion-email='<%# GetAttributeValue(Eval("CompanionEmail"), "Not provided") %>'
                                            data-companion-contact='<%# GetAttributeValue(Eval("CompanionContact"), "Not provided") %>'
                                            data-companion-image='<%# GetImageUrl(Eval("CompanionProfilePicture")) %>'
                                            data-companion-initials='<%# GetInitialsValue(Eval("CompanionName")) %>'
                                            data-package='<%# GetAttributeValue(Eval("PackageName"), "Companion service") %>'
                                            data-schedule='<%# GetScheduleValue(Eval("BookingDate"), Eval("BookingTime")) %>'
                                            data-status='<%# GetAttributeValue(Eval("BookingStatus"), "Confirmed") %>'
                                            onclick="openNotificationDetails(this); return false;">
                                            View Details
                                        </button>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <asp:Panel ID="pnlNoNotifs" runat="server" Visible="false" CssClass="no-notifs-panel">
                        <div class="no-notifs-icon"><svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><path d="M3 6h18v13H3z" /><path d="m3 7 9 6 9-6M8 16h8" /></svg></div>
                        <h4>No Notifications Yet</h4>
                        <p>You're all caught up! We will notify you once there are updates regarding your bookings.</p>
                    </asp:Panel>
                </div>

                <div id="notificationDetailsModal" class="notification-modal" aria-hidden="true" role="dialog" aria-modal="true" aria-labelledby="notificationModalTitle">
                    <div class="notification-modal-backdrop" onclick="closeNotificationDetails()"></div>
                    <div class="notification-modal-card">
                        <div class="notification-modal-header">
                            <div>
                                <span class="notification-label" id="modalHeaderLabel">Booking details</span>
                                <h3 id="notificationModalTitle">Booking Details</h3>
                            </div>
                            <button type="button" class="notification-modal-close" onclick="closeNotificationDetails()" aria-label="Close">&times;</button>
                        </div>
                        <div class="notification-modal-body">
                            <!-- Text and color of this banner are set by openNotificationDetails() from the booking's real status -->
                            <div class="notification-modal-status" id="modalStatusBanner">
                                <span class="status-dot"></span>
                                <span id="modalStatusText"></span>
                            </div>
                            <div class="notification-modal-parties">
                                <div class="notification-modal-party">
                                    <span class="notification-label">Customer</span>
                                    <strong id="modalCustomerName"></strong>
                                    <span id="modalCustomerEmail"></span>
                                    <span id="modalCustomerContact"></span>
                                </div>
                                <div class="notification-modal-party">
                                    <div class="notification-modal-companion">
                                        <img id="modalCompanionImage" class="modal-companion-image" alt="" />
                                        <div id="modalCompanionInitials" class="notification-companion-initials modal-companion-initials"></div>
                                    </div>
                                    <span class="notification-label">Companion</span>
                                    <strong id="modalCompanionName"></strong>
                                    <span id="modalCompanionEmail"></span>
                                    <span id="modalCompanionContact"></span>
                                </div>
                            </div>
                            <div class="notification-modal-booking">
                                <div><span>Package</span><strong id="modalPackage"></strong></div>
                                <div><span>Schedule</span><strong id="modalSchedule"></strong></div>
                                <div><span>Status</span><strong id="modalStatus"></strong></div>
                            </div>
                        </div>
                        <div class="notification-modal-footer">
                            <button type="button" class="notification-modal-secondary" onclick="closeNotificationDetails()">Close</button>
                        </div>
                    </div>
                </div>

            </div>
        </div>

        <!-- Script para sa Topbar Dropdown Toggle at Modal Handling -->
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

            // What the banner and header say for each booking status. "css" picks the banner color (see CustomerNotification.css).
            var bookingStatusInfo = {
                confirmed: { label: 'Booking confirmed', text: 'This booking has been accepted by the companion.', css: '' },
                declined:  { label: 'Booking declined',  text: 'This booking request was declined by the companion.', css: 'is-declined' },
                pending:   { label: 'Booking pending',   text: 'This booking is still waiting for the companion\'s response.', css: 'is-pending' },
                cancelled: { label: 'Booking cancelled', text: 'This booking has been cancelled.', css: 'is-cancelled' },
                completed: { label: 'Booking completed', text: 'This booking has been completed.', css: 'is-completed' }
            };

            function applyBookingStatus(status) {
                var info = bookingStatusInfo[(status || '').toLowerCase()] ||
                    { label: 'Booking details', text: 'Booking status: ' + (status || 'Unknown'), css: 'is-pending' };
                var banner = document.getElementById('modalStatusBanner');
                banner.className = 'notification-modal-status' + (info.css ? ' ' + info.css : '');
                document.getElementById('modalStatusText').textContent = info.text;
                document.getElementById('modalHeaderLabel').textContent = info.label;
            }

            function openNotificationDetails(button) {
                var modal = document.getElementById('notificationDetailsModal');
                var image = document.getElementById('modalCompanionImage');
                var initials = document.getElementById('modalCompanionInitials');

                document.getElementById('modalCustomerName').textContent = button.dataset.customerName;
                document.getElementById('modalCustomerEmail').textContent = button.dataset.customerEmail;
                document.getElementById('modalCustomerContact').textContent = button.dataset.customerContact;
                document.getElementById('modalCompanionName').textContent = button.dataset.companionName;
                document.getElementById('modalCompanionEmail').textContent = button.dataset.companionEmail;
                document.getElementById('modalCompanionContact').textContent = button.dataset.companionContact;
                document.getElementById('modalPackage').textContent = button.dataset.package;
                document.getElementById('modalSchedule').textContent = button.dataset.schedule;
                document.getElementById('modalStatus').textContent = button.dataset.status;
                applyBookingStatus(button.dataset.status);

                if (button.dataset.companionImage && button.dataset.companionImage.trim() !== "") {
                    image.src = button.dataset.companionImage;
                    image.alt = button.dataset.companionName;
                    image.classList.add('is-visible');
                    initials.classList.remove('is-visible');
                } else {
                    initials.textContent = button.dataset.companionInitials;
                    initials.classList.add('is-visible');
                    image.classList.remove('is-visible');
                }

                modal.classList.add('show');
                modal.setAttribute('aria-hidden', 'false');
                document.body.classList.add('notification-modal-open');
            }

            function closeNotificationDetails() {
                var modal = document.getElementById('notificationDetailsModal');
                modal.classList.remove('show');
                modal.setAttribute('aria-hidden', 'true');
                document.body.classList.remove('notification-modal-open');
            }

            document.addEventListener('keydown', function (event) {
                if (event.key === 'Escape') {
                    closeNotificationDetails();
                }
            });
        </script>
        <script src="../Scripts/site.js"></script>
        <script src="../Scripts/nav-counts.js"></script>
    </form>
</body>
</html>