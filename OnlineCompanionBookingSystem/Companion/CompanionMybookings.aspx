<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CompanionMybookings.aspx.cs" Inherits="OnlineCompanionBookingSystem.Companion.CompanionMybookings" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>My Bookings - Online Companion Booking System</title>
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/CompanionDashboard.css?v=2.5" rel="stylesheet" runat="server" />
    <link href="~/CSS/CompanionMybookings.css" rel="stylesheet" runat="server" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="dashboard-wrapper">
            <!-- Sidebar navigation -->
            <aside class="sidebar">
                <div class="sidebar-top">
                    <div class="sidebar-logo">
                        <span class="brand-title">OCBMS</span>
                        <span class="brand-title">OCBMS</span>
                    </div>
                    <nav class="sidebar-nav">
                        <asp:HyperLink ID="navDashboard" runat="server" NavigateUrl="~/Companion/CompanionDashboard.aspx" CssClass="sidebar-link">
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
                        <asp:HyperLink ID="navMyBookings" runat="server" NavigateUrl="~/Companion/CompanionMybookings.aspx" CssClass="sidebar-link active">
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
                <!-- Topbar -->
                <header class="topbar">
                    <div class="topbar-left">
                        <h2>My Bookings</h2>
                        <p class="topbar-subtitle">View and manage your scheduled and completed client bookings.</p>
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
                    <!-- Message Banner -->
                    <asp:Label ID="lblMessage" runat="server" CssClass="message-banner" Visible="false" />

                    <!-- My Bookings Card List -->
                    <section class="content-card mybookings-container-card">
                        <div class="section-header">
                            <div>
                                <h3>Scheduled Bookings</h3>
                                <p>Track your confirmed and completed client sessions.</p>
                            </div>
                        </div>
                        
                        <div style="margin-top: 16px;">
                            <asp:Repeater ID="rptMyBookings" runat="server" OnItemCommand="rptMyBookings_ItemCommand">
                                <HeaderTemplate><div class="mybooking-list"></HeaderTemplate>
                                <ItemTemplate>
                                    <div class="mybooking-row">
                                        <div class="mybooking-customer-avatar">
                                            <asp:Image ID="imgCustomer" runat="server" ImageUrl='<%# Eval("CustomerProfilePicture") %>' Visible='<%# Eval("CustomerProfilePicture") != DBNull.Value && !string.IsNullOrWhiteSpace(Eval("CustomerProfilePicture").ToString()) %>' CssClass="mybooking-customer-img" />
                                            <asp:Literal ID="litCustInitials" runat="server" Text='<%# GetInitials(Eval("CustomerName")) %>' Visible='<%# Eval("CustomerProfilePicture") == DBNull.Value || string.IsNullOrWhiteSpace(Eval("CustomerProfilePicture").ToString()) %>' />
                                        </div>
                                        <div class="mybooking-details">
                                            <div class="mybooking-top-info">
                                                <strong><%# Server.HtmlEncode(Eval("CustomerName").ToString()) %></strong>
                                                <span class="status-badge <%# GetStatusClass(Eval("StatusKey")) %>" data-booking-start="<%#: Eval("StartIso") %>" data-booking-end="<%#: Eval("EndIso") %>" data-now="<%#: Eval("ServerNowIso") %>" data-ongoing-note="<%#: Eval("OngoingNote") %>" data-awaiting-note="<%#: Eval("AwaitingNote") %>" data-note-id="sn-<%#: Eval("BookingID") %>"><%#: Eval("DisplayStatus") %></span>
                                            </div>
                                            <p class="mybooking-package">Package: <span><%# Server.HtmlEncode(Eval("PackageName").ToString()) %></span></p>
                                            <%# Eval("Activities") == DBNull.Value ? "" : "<p class=\"mybooking-package\">Activities: <span>" + Server.HtmlEncode(Eval("Activities").ToString()) + "</span></p>" %>
                                            <p class="mybooking-schedule">Schedule: <span><%# Eval("BookingDate", "{0:MMM. dd, yyyy}") %> at <%# Server.HtmlEncode(Eval("BookingTime").ToString()) %></span></p>
                                            <%# Eval("UntilText") == DBNull.Value || Convert.ToString(Eval("UntilText")) == "" ? "" : "<p class=\"mybooking-schedule\">Until: <span>" + Server.HtmlEncode(Eval("UntilText").ToString()) + "</span></p>" %>
                                            <p class="booking-status-note" id="sn-<%#: Eval("BookingID") %>"><%#: Eval("StatusNote") %></p>
                                        </div>
                                        
                                        <!-- Action Button to complete booking if confirmed -->
                                        <div class="mybooking-actions">
                                            <asp:Button ID="btnComplete" runat="server" CommandName="CompleteBooking" CommandArgument='<%# Eval("BookingID") %>' Text="Mark as Completed" CssClass="btn-action-complete" Visible='<%# string.Equals(Eval("Status").ToString(), "Confirmed", StringComparison.OrdinalIgnoreCase) %>' />
                                        </div>
                                    </div>
                                </ItemTemplate>
                                <FooterTemplate></div></FooterTemplate>
                            </asp:Repeater>

                            <!-- Empty State Panel -->
                            <asp:Label ID="lblNoBookings" runat="server" CssClass="empty-state" Text="No bookings found in your schedule." Visible="false" />
                        </div>
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