<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CompanionAvailability.aspx.cs" Inherits="OnlineCompanionBookingSystem.Companion.CompanionAvailability" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Manage Availability - Online Companion Booking System</title>
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/CompanionDashboard.css?v=2.5" rel="stylesheet" runat="server" />
    <link href="~/CSS/CompanionAvailability.css" rel="stylesheet" runat="server" />
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
                        <asp:HyperLink ID="navAvailability" runat="server" NavigateUrl="~/Companion/CompanionAvailability.aspx" CssClass="sidebar-link active">
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
                <!-- Topbar with user profile and notification shortcut -->
                <header class="topbar">
                    <div class="topbar-left">
                        <h2>Manage Availability</h2>
                        <p class="topbar-subtitle">Set your weekly schedule and working hours for client bookings.</p>
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
                    <!-- Feedback Message Label -->
                    <asp:Label ID="lblMessage" runat="server" CssClass="message-banner" Visible="false" />

                   <!-- Add Availability Form Card -->
                    <section class="content-card availability-card">
                        <div class="section-header">
                            <div>
                                <h3>Add Weekly Schedule Slot</h3>
                                <p>Specify the day and working hours when you are available to accept bookings.</p>
                            </div>
                        </div>
                        
                        <div class="availability-form-grid">
                            <div class="form-group">
                                <label class="form-label">Day of the Week</label>
                                <asp:DropDownList ID="ddlDayOfWeek" runat="server" CssClass="form-input">
                                    <asp:ListItem Text="Monday" Value="Monday" />
                                    <asp:ListItem Text="Tuesday" Value="Tuesday" />
                                    <asp:ListItem Text="Wednesday" Value="Wednesday" />
                                    <asp:ListItem Text="Thursday" Value="Thursday" />
                                    <asp:ListItem Text="Friday" Value="Friday" />
                                    <asp:ListItem Text="Saturday" Value="Saturday" />
                                    <asp:ListItem Text="Sunday" Value="Sunday" />
                                </asp:DropDownList>
                            </div>

                            <div class="form-group">
                                <label class="form-label">Start Time</label>
                                <asp:TextBox ID="txtStartTime" runat="server" TextMode="Time" CssClass="form-input"></asp:TextBox>
                            </div>

                            <div class="form-group">
                                <label class="form-label">End Time</label>
                                <asp:TextBox ID="txtEndTime" runat="server" TextMode="Time" CssClass="form-input"></asp:TextBox>
                            </div>

                            <div class="form-group form-action-group">
                                <asp:Button ID="btnAddAvailability" runat="server" Text="+ Add Slot" CssClass="btn-primary-action" OnClick="btnAddAvailability_Click" />
                            </div>
                        </div>
                    </section>

                    <!-- Active Schedule Slots List -->
                    <section class="content-card availability-list-card">
                        <div class="section-header">
                            <div>
                                <h3>Your Active Schedule Slots</h3>
                                <p>Review and manage your working hours.</p>
                            </div>
                        </div>
                        
                        <div class="availability-list">
                            <asp:Repeater ID="rptAvailability" runat="server" OnItemCommand="rptAvailability_ItemCommand">
                                <HeaderTemplate><div class="booking-list"></HeaderTemplate>
                                <ItemTemplate>
                                    <div class="booking-row">
                                        <div class="availability-icon" aria-hidden="true">
                                            <svg class="ui-icon" viewBox="0 0 24 24">
                                                <rect x="3" y="5" width="18" height="16" rx="2" />
                                                <path d="M16 3v4M8 3v4M3 10h18" />
                                            </svg>
                                        </div>
                                        <div class="booking-details">
                                            <strong><%# Server.HtmlEncode(Eval("DayOfWeek").ToString()) %></strong>
                                            <span><%# FormatTime(Eval("StartTime")) %> &mdash; <%# FormatTime(Eval("EndTime")) %></span>
                                        </div>
                                        <span class="status-badge status-confirmed">Active</span>
                                        <asp:Button ID="btnDelete" runat="server" CommandName="DeleteSlot" CommandArgument='<%# Eval("AvailabilityID") %>' Text="Remove" CssClass="btn-action-danger" OnClientClick="return confirm('Are you sure you want to remove this schedule slot?');" />
                                    </div>
                                </ItemTemplate>
                                <FooterTemplate></div></FooterTemplate>
                            </asp:Repeater>
                            <asp:Label ID="lblNoAvailability" runat="server" CssClass="empty-state" Text="No schedule slots have been added yet." Visible="false" />
                        </div>
                    </section>
                </div>
            </main>
        </div>
        <script src="../Scripts/site.js"></script>
        <script src="../Scripts/nav-counts.js"></script>
    </form>
</body>
</html>