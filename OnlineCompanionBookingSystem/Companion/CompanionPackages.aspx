<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CompanionPackages.aspx.cs" Inherits="OnlineCompanionBookingSystem.Companion.CompanionPackages" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>My Packages - Online Companion Booking System</title>
    <!-- Shared styles are loaded first so package-specific rules can extend the dashboard theme. -->
    <!-- Shared dashboard styles keep the package page consistent with the Companion portal. -->
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/CompanionDashboard.css?v=2.5" rel="stylesheet" runat="server" />
    <link href="~/CSS/CompanionPackages.css" rel="stylesheet" runat="server" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="dashboard-wrapper">
            <!-- Companion navigation keeps this page consistent with the dashboard and activities page. -->
            <aside class="sidebar">
                <div class="sidebar-top">
                    <div class="sidebar-logo">
                        <span class="brand-title">OCBMS</span>
                    </div>
                    <nav class="sidebar-nav">
                        <asp:HyperLink ID="navDashboard" runat="server" NavigateUrl="~/Companion/CompanionDashboard.aspx" CssClass="sidebar-link"><span>Dashboard</span></asp:HyperLink>
                        <asp:HyperLink ID="navActivities" runat="server" NavigateUrl="~/Companion/CompanionActivity.aspx" CssClass="sidebar-link"><span>Activities</span></asp:HyperLink>
                        <asp:HyperLink ID="navPackages" runat="server" NavigateUrl="~/Companion/CompanionPackages.aspx" CssClass="sidebar-link active"><span>Packages</span></asp:HyperLink>
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
                        <h2>My Packages</h2>
                        <p class="topbar-subtitle">Create clear service packages and rates for your customers.</p>
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

                <div class="dashboard-content packages-content">
                    <!-- Validation and database operation feedback is displayed in this panel. -->
                    <asp:Panel ID="pnlMessage" runat="server" CssClass="package-message" Visible="false">
                        <asp:Literal ID="litMessage" runat="server" />
                    </asp:Panel>

                    <section class="package-card">
                        <!-- New packages are created with a name, duration, rate, and optional description. -->
                        <div class="section-header">
                            <div>
                                <h3>Add a package</h3>
                                <p>Define the service, duration, and rate customers will see.</p>
                            </div>
                        </div>
                        <div class="package-form-grid">
                            <div class="form-field form-field-wide">
                                <label for="txtPackageName">Package name</label>
                                <asp:TextBox ID="txtPackageName" runat="server" CssClass="package-input" MaxLength="100" placeholder="e.g., Standard Companion Session" />
                                <asp:RequiredFieldValidator ID="rfvPackageName" runat="server" ValidationGroup="AddPackage" ControlToValidate="txtPackageName" CssClass="field-error" Display="Dynamic" ErrorMessage="Package name is required." />
                            </div>
                            <div class="form-field">
                                <label for="txtDuration">Duration</label>
                                <div class="duration-input-group">
                                    <asp:TextBox ID="txtDuration" runat="server" CssClass="package-input" TextMode="Number" min="1" max="30" step="1" inputmode="numeric" data-digits-only="true" data-max="30" placeholder="1" />
                                    <asp:DropDownList ID="ddlDurationUnit" runat="server" CssClass="package-input duration-unit">
                                        <asp:ListItem Text="Hour" Value="Hour" />
                                        <asp:ListItem Text="Day" Value="Day" />
                                    </asp:DropDownList>
                                </div>
                                <asp:RequiredFieldValidator ID="rfvDuration" runat="server" ValidationGroup="AddPackage" ControlToValidate="txtDuration" CssClass="field-error" Display="Dynamic" ErrorMessage="Duration is required." />
                            </div>
                            <div class="form-field">
                                <label for="txtRate">Rate (&#8369;)</label>
                                <asp:TextBox ID="txtRate" runat="server" CssClass="package-input" TextMode="Number" min="50" max="50000" step="1" inputmode="numeric" placeholder="300" data-digits-only="true" data-max="50000" />
                                <asp:RequiredFieldValidator ID="rfvRate" runat="server" ValidationGroup="AddPackage" ControlToValidate="txtRate" CssClass="field-error" Display="Dynamic" ErrorMessage="Rate is required." />
                                <asp:RangeValidator ID="rvRate" runat="server" ValidationGroup="AddPackage" ControlToValidate="txtRate" Type="Integer" MinimumValue="50" MaximumValue="50000" CssClass="field-error" Display="Dynamic" ErrorMessage="Rate must be a whole number from 50 to 50,000 pesos." />
                            </div>
                            <div class="form-field form-field-full">
                                <label for="txtDescription">Description <span>(optional)</span></label>
                                <asp:TextBox ID="txtDescription" runat="server" CssClass="package-input" TextMode="MultiLine" Rows="3" MaxLength="500" placeholder="Describe what is included in this package." />
                            </div>
                        </div>
                        <div class="form-actions">
                            <asp:Button ID="btnAddPackage" runat="server" ValidationGroup="AddPackage" Text="Add Package" CssClass="btn-primary" OnClick="btnAddPackage_Click" />
                        </div>
                    </section>

                    <section class="package-card">
                        <!-- Existing packages can be edited inline or removed when not used by a booking. -->
                        <div class="section-header">
                            <div>
                                <h3>Your packages</h3>
                                <p>Update your services whenever your offerings or rates change.</p>
                            </div>
                            <span class="package-count"><asp:Literal ID="litPackageCount" runat="server" Text="0" /> packages</span>
                        </div>
                        <asp:Repeater ID="rptPackages" runat="server" OnItemCommand="rptPackages_ItemCommand" OnItemDataBound="rptPackages_ItemDataBound">
                            <HeaderTemplate><div class="package-list"></HeaderTemplate>
                            <ItemTemplate>
                                <div class="package-row">
                                    <div class="package-icon">
                                        <svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><path d="M20 12 12 20 4 12l8-8h6l2 2v6z" /><circle cx="15.5" cy="8.5" r="1" /></svg>
                                    </div>
                                    <div class="package-edit-fields">
                                        <asp:TextBox ID="txtEditPackageName" runat="server" Text='<%# Eval("PackageName") %>' CssClass="package-edit-input package-edit-name" MaxLength="100" />
                                        <asp:TextBox ID="txtEditDescription" runat="server" Text='<%# Eval("Description") %>' CssClass="package-edit-input package-edit-description" MaxLength="500" />
                                    </div>
                                    <div class="package-edit-number">
                                        <asp:TextBox ID="txtEditDuration" runat="server" Text='<%# GetDurationNumber(Eval("Duration")) %>' CssClass="package-edit-input duration-number" TextMode="Number" min="1" max="30" inputmode="numeric" data-digits-only="true" data-max="30" />
                                        <asp:DropDownList ID="ddlEditDurationUnit" runat="server" CssClass="package-edit-input duration-unit">
                                            <asp:ListItem Text="Hour" Value="Hour" />
                                            <asp:ListItem Text="Day" Value="Day" />
                                        </asp:DropDownList>
                                    </div>
                                    <div class="package-edit-number">
                                        <asp:TextBox ID="txtEditRate" runat="server" Text='<%# Eval("Rate", "{0:0}") %>' CssClass="package-edit-input" TextMode="Number" min="50" max="50000" step="1" inputmode="numeric" data-digits-only="true" data-max="50000" />
                                        <span>&#8369;</span>
                                    </div>
                                    <asp:LinkButton ID="btnSavePackage" runat="server" CausesValidation="false" CommandName="SavePackage" CommandArgument='<%# Eval("PackageID") %>' CssClass="btn-small btn-save">Save</asp:LinkButton>
                                    <asp:LinkButton ID="btnDeletePackage" runat="server" CausesValidation="false" CommandName="DeletePackage" CommandArgument='<%# Eval("PackageID") %>' CssClass="btn-small btn-delete" OnClientClick="return confirm('Remove this package? Existing bookings will not be changed.');">Remove</asp:LinkButton>
                                </div>
                            </ItemTemplate>
                            <FooterTemplate></div></FooterTemplate>
                        </asp:Repeater>
                        <asp:Label ID="lblNoPackages" runat="server" CssClass="empty-state" Text="No packages added yet." Visible="false" />
                    </section>
                </div>
            </main>
        </div>
        <script src="../Scripts/site.js"></script>
        <script src="../Scripts/input-limits.js"></script>
        <script src="../Scripts/nav-counts.js"></script>
    </form>
</body>
</html>
