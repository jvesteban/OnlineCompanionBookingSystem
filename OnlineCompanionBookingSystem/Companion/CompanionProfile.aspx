<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CompanionProfile.aspx.cs" Inherits="OnlineCompanionBookingSystem.Companion.CompanionProfile" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Companion Profile - Online Companion Booking System</title>
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/CompanionDashboard.css?v=2.5" rel="stylesheet" runat="server" />
    <link href="~/CSS/CompanionProfile.css" rel="stylesheet" runat="server" />
</head>
<body>
    <form id="form1" runat="server" enctype="multipart/form-data">
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
                        <asp:HyperLink ID="navMyBookings" runat="server" NavigateUrl="~/Companion/CompanionMybookings.aspx" CssClass="sidebar-link">
                            <span>My Bookings</span>
                            <span class="nav-count" data-nav-count="bookings" hidden></span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="navNotifications" runat="server" NavigateUrl="~/Companion/CompanionNotifications.aspx" CssClass="sidebar-link">
                            <span>Notifications</span>
                            <span class="nav-count" data-nav-count="notifications" hidden></span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="navProfile" runat="server" NavigateUrl="~/Companion/CompanionProfile.aspx" CssClass="sidebar-link active">
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
                        <h2>My Profile</h2>
                        <p class="topbar-subtitle">Manage your personal information, contact details, and profile photo.</p>
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
                                <asp:Image ID="imgTopAvatar" runat="server" CssClass="user-avatar-image" Visible="false" />
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

                    <!-- Profile Form Card Container -->
                    <section class="content-card profile-card">
                        <div class="section-header">
                            <div>
                                <h3>Account Profile Settings</h3>
                                <p>Update your name, contact information, and profile picture.</p>
                            </div>
                        </div>

                        <!-- AVATAR UPLOAD SECTION -->
                        <div class="profile-avatar-section">
                            <div class="current-avatar-preview">
                                <asp:Image ID="imgProfilePreview" runat="server" CssClass="profile-big-avatar" Visible="false" />
                                <div class="avatar-initial-fallback" id="divFallbackInitial" runat="server">
                                    <asp:Literal ID="litProfileBigInitial" runat="server" Text="C" />
                                </div>
                            </div>
                            <div class="avatar-upload-info">
                                <h4>Profile Photo</h4>
                                <p>Upload a clear, professional photo. (Supported formats: JPG, PNG)</p>
                                <asp:FileUpload ID="fileUploadAvatar" runat="server" CssClass="file-input-hidden" />
                                <label for="fileUploadAvatar" class="btn-upload-trigger">Choose New Photo</label>
                            </div>
                        </div>

                        <!-- NAME & DETAILS GRID -->
                        <div class="form-grid-multi">
                            <div class="form-group">
                                <label class="form-label">Surname (Apelyido)</label>
                                <asp:TextBox ID="txtSurname" runat="server" CssClass="form-input" Placeholder="Enter surname"></asp:TextBox>
                            </div>

                            <div class="form-group">
                                <label class="form-label">First Name (Pangalan)</label>
                                <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-input" Placeholder="Enter first name"></asp:TextBox>
                            </div>

                            <div class="form-group">
                                <label class="form-label">Middle Name (Gitnang Pangalan)</label>
                                <asp:TextBox ID="txtMiddleName" runat="server" CssClass="form-input" Placeholder="Optional"></asp:TextBox>
                            </div>

                            <div class="form-group">
                                <label class="form-label">Email Address</label>
                                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-input" TextMode="Email" Placeholder="Enter email address"></asp:TextBox>
                            </div>

                            <div class="form-group full-width">
                                <label class="form-label">Contact Number</label>
                                <asp:TextBox ID="txtContactNumber" runat="server" CssClass="form-input" Placeholder="e.g., 09123456789" MaxLength="11" inputmode="numeric" autocomplete="tel" data-digits-only="true" data-max-digits="11" pattern="09[0-9]{9}" title="11-digit PH mobile number starting with 09 (09XXXXXXXXX)"></asp:TextBox>
                            </div>

                            <!-- Shown to customers in the "About Companion" section of your public profile -->
                            <div class="form-group full-width">
                                <label class="form-label" for="txtBio">About You</label>
                                <asp:TextBox ID="txtBio" runat="server" CssClass="form-input form-textarea" TextMode="MultiLine" Rows="5" MaxLength="500" ValidateRequestMode="Disabled"
                                    Placeholder="Tell customers a little about yourself: your interests, the places you enjoy, and what they can expect when they book you."></asp:TextBox>
                                <span class="field-hint">Shown on your public profile. <span id="bioCount">0</span> / 500 characters.</span>
                            </div>
                        </div>

                        <div class="form-actions">
                            <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="btn-save" OnClick="btnSave_Click" />
                        </div>
                    </section>
                </div>
            </main>
        </div>
        <script src="../Scripts/site.js"></script>
        <script>
            // Live character counter for the "About You" box
            (function () {
                var box = document.getElementById('<%= txtBio.ClientID %>');
                var count = document.getElementById('bioCount');
                if (!box || !count) return;
                function update() { count.textContent = box.value.length; }
                box.addEventListener('input', update);
                update();
            })();
        </script>
        <script src="../Scripts/input-limits.js"></script>
        <script src="../Scripts/nav-counts.js"></script>
    </form>
</body>
</html>