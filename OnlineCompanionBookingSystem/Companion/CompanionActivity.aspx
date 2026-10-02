<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CompanionActivity.aspx.cs" Inherits="OnlineCompanionBookingSystem.Companion.CompanionActivity" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>My Activities - Online Companion Booking System</title>
    <!-- Shared dashboard styles provide the common OCBMS visual language. -->
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/CompanionDashboard.css?v=2.5" rel="stylesheet" runat="server" />
    <link href="~/CSS/CompanionActivity.css" rel="stylesheet" runat="server" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="dashboard-wrapper">
            <!-- Navigation remains consistent with the Companion dashboard. -->
            <aside class="sidebar">
                <div class="sidebar-top">
                    <div class="sidebar-logo">
                        <span class="brand-title">OCBMS</span>
                    </div>
                    <nav class="sidebar-nav">
                        <asp:HyperLink ID="navDashboard" runat="server" NavigateUrl="~/Companion/CompanionDashboard.aspx" CssClass="sidebar-link"><span>Dashboard</span></asp:HyperLink>
                        <asp:HyperLink ID="navActivities" runat="server" NavigateUrl="~/Companion/CompanionActivity.aspx" CssClass="sidebar-link active"><span>Activities</span></asp:HyperLink>
                        <asp:HyperLink ID="navPackages" runat="server" NavigateUrl="~/Companion/CompanionPackages.aspx" CssClass="sidebar-link"><span>Packages</span></asp:HyperLink>
                        <asp:HyperLink ID="navAvailability" runat="server" NavigateUrl="~/Companion/CompanionAvailability.aspx" CssClass="sidebar-link"><span>Availability</span></asp:HyperLink>
                        <asp:HyperLink ID="navBookingRequests" runat="server" NavigateUrl="~/Companion/CompanionBookingRequest.aspx" CssClass="sidebar-link"><span>Booking Requests</span><span class="nav-count" data-nav-count="requests" hidden></span></asp:HyperLink>
                        <asp:HyperLink ID="navMyBookings" runat="server" NavigateUrl="~/Companion/CompanionMybookings.aspx" CssClass="sidebar-link"><span>My Bookings</span><span class="nav-count" data-nav-count="bookings" hidden></span></asp:HyperLink>
                        <asp:HyperLink ID="navNotifications" runat="server" NavigateUrl="~/Companion/CompanionNotifications.aspx" CssClass="sidebar-link"><span>Notifications</span><span class="nav-count" data-nav-count="notifications" hidden></span></asp:HyperLink>
                        <asp:HyperLink ID="navProfile" runat="server" NavigateUrl="~/Companion/CompanionProfile.aspx" CssClass="sidebar-link"><span>My Profile</span></asp:HyperLink>
                    </nav>
                </div>
               <div class="sidebar-footer">
    <asp:Button ID="btnLogout" runat="server" Text="Sign Out" CssClass="logout-btn" OnClick="btnLogout_Click" CausesValidation="false" OnClientClick="return confirm('Are you sure you want to log out?');" />
</div>
            </aside>

            <main class="main-content">
                <header class="topbar">
                    <div class="topbar-left">
                        <h2>My Activities</h2>
                        <p class="topbar-subtitle">Manage the activities and services shown on your companion profile.</p>
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
                                <asp:Image ID="imgTopAvatar" runat="server" CssClass="user-avatar-image" Visible="false" AlternateText="Companion profile photo" />
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

                <div class="dashboard-content activities-content">
                    <asp:Panel ID="pnlMessage" runat="server" CssClass="activity-message" Visible="false">
                        <asp:Literal ID="litMessage" runat="server" />
                    </asp:Panel>

                    <section class="activity-card">
                        <div class="section-header">
                            <div>
                                <h3>Offer an activity</h3>
                                <p>Add a service that customers can see when browsing your profile.</p>
                            </div>
                        </div>
                        <div class="activity-form">
                            <asp:TextBox ID="txtActivityName" runat="server" CssClass="activity-input" MaxLength="100" placeholder="e.g., City tour, jogging, event companion" />
                            <asp:Button ID="btnAddActivity" runat="server" Text="Add Activity" CssClass="btn-primary" OnClick="btnAddActivity_Click" />
                        </div>
                        <asp:RequiredFieldValidator ID="rfvActivityName" runat="server" ControlToValidate="txtActivityName" CssClass="field-error" Display="Dynamic" ErrorMessage="Please enter an activity name." />
                    </section>

                    <section class="activity-card">
                        <div class="section-header">
                            <div>
                                <h3>Your activities</h3>
                                <p>Keep your list accurate so customers know what you offer.</p>
                            </div>
                            <span class="activity-count"><asp:Literal ID="litActivityCount" runat="server" Text="0" /> activities</span>
                        </div>
                        <asp:Repeater ID="rptActivities" runat="server" OnItemCommand="rptActivities_ItemCommand">
                            <HeaderTemplate><div class="activity-list"></HeaderTemplate>
                            <ItemTemplate>
                                <div class="activity-row">
                                    <div class="activity-mark">
                                        <svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><path d="m5 12 4 4L19 6" /></svg>
                                    </div>
                                    <asp:TextBox ID="txtEditActivity" runat="server" Text='<%# Eval("ActivityName") %>' CssClass="activity-edit-input" MaxLength="100" />
                                    <asp:LinkButton ID="btnSaveActivity" runat="server" CommandName="SaveActivity" CommandArgument='<%# Eval("ActivityID") %>' CssClass="btn-small btn-save">Save</asp:LinkButton>
                                    <asp:LinkButton ID="btnDeleteActivity" runat="server" CommandName="DeleteActivity" CommandArgument='<%# Eval("ActivityID") %>' CssClass="btn-small btn-delete" OnClientClick="return confirm('Remove this activity from your profile?');">Remove</asp:LinkButton>
                                </div>
                            </ItemTemplate>
                            <FooterTemplate></div></FooterTemplate>
                        </asp:Repeater>
                        <asp:Label ID="lblNoActivities" runat="server" CssClass="empty-state" Text="No activities added yet." Visible="false" />
                    </section>
                </div>
            </main>
        </div>
        <script src="../Scripts/site.js"></script>
        <script src="../Scripts/nav-counts.js"></script>
    </form>
</body>
</html>
