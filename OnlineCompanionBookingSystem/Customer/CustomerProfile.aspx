<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CustomerProfile.aspx.cs" Inherits="OnlineCompanionBookingManagementSystem.Customer.Profile" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Customer Profile - Online Companion Booking System</title>
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/CustomerDashboard.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/CustomerProfile.css" rel="stylesheet" type="text/css" />
    <!-- Animations and small layout fixes (loaded last so it adds to the page CSS) -->
    <link href="~/CSS/CustomerMotion.css?v=2" rel="stylesheet" runat="server" />
</head>
<body>
    <form id="form1" runat="server" enctype="multipart/form-data">
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
                        <asp:HyperLink ID="NavNotifications" runat="server" NavigateUrl="~/Customer/CustomerNotification.aspx" CssClass="sidebar-link">
                            <span>Notifications</span>
                            <span class="nav-count" data-nav-count="notifications" hidden></span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="NavProfile" runat="server" NavigateUrl="~/Customer/CustomerProfile.aspx" CssClass="sidebar-link active">
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
                        <h2>My Profile</h2>
                        <p class="topbar-subtitle">Manage your personal information and profile picture.</p>
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
                                <a class="dropdown-item" href="CustomerProfile.aspx">
                                    <span><svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="3" /><path d="M19.4 15a1.7 1.7 0 0 0 .3 1.9l.1.1-1.8 1.8-.1-.1a1.7 1.7 0 0 0-1.9-.3 1.7 1.7 0 0 0-1 1.5v.1h-2.6v-.1a1.7 1.7 0 0 0-1-1.5 1.7 1.7 0 0 0-1.9.3l-.1.1-1.8-1.8.1-.1a1.7 1.7 0 0 0 .3-1.9 1.7 1.7 0 0 0-1.5-1H6.4v-2.6h.1a1.7 1.7 0 0 0 1.5-1 1.7 1.7 0 0 0-.3-1.9l-.1-.1 1.8-1.8.1.1a1.7 1.7 0 0 0 1.9.3 1.7 1.7 0 0 0 1-1.5V5h2.6v.1a1.7 1.7 0 0 0 1 1.5 1.7 1.7 0 0 0 1.9-.3l.1-.1 1.8 1.8-.1.1a1.7 1.7 0 0 0-.3 1.9 1.7 1.7 0 0 0 1.5 1h.1v2.6h-.1a1.7 1.7 0 0 0-1.5 1z" /></svg> Account Settings</span>
                                </a>
                                <div class="dropdown-divider"></div>
                                <asp:Button ID="btnDropdownLogout" runat="server" Text="Log Out Securely" CssClass="dropdown-item dropdown-logout-btn" OnClick="btnLogout_Click" OnClientClick="return confirm('Are you sure you want to log out?');" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- ===== PROFILE FORM CONTAINER ===== -->
                <div class="section-block">
                    <div class="profile-card">
                        <div class="profile-card-header">
                            <h3>Account Profile Settings</h3>
                            <p>Update your name, contact details, and profile avatar.</p>
                        </div>

                        <asp:Label ID="lblMessage" runat="server" CssClass="message" Visible="false" />

                        <!-- AVATAR UPLOAD SECTION -->
                        <div class="profile-avatar-section">
                            <div class="current-avatar-preview">
                                <asp:Image ID="imgProfilePreview" runat="server" CssClass="profile-big-avatar" Visible="false" />
                                <div class="avatar-initial-fallback" id="divFallbackInitial" runat="server">
                                    <asp:Literal ID="litProfileBigInitial" runat="server" Text="U" />
                                </div>
                            </div>
                            <div class="avatar-upload-info">
                                <h4>Profile Photo</h4>
                                <p>Upload a clear, professional photo. (Supported: JPG, PNG)</p>
                                <asp:FileUpload ID="fileUploadAvatar" runat="server" CssClass="file-input-hidden" />
                                <label for="fileUploadAvatar" class="btn-upload-trigger">Choose New Photo</label>
                            </div>
                        </div>

                        <!-- NAME & DETAILS GRID -->
                        <div class="form-grid-multi">
                            <div class="form-group">
                                <label>Surname (Apelyido)</label>
                                <asp:TextBox ID="txtSurname" runat="server" CssClass="form-input" Placeholder="Enter surname"></asp:TextBox>
                            </div>

                            <div class="form-group">
                                <label>First Name (Pangalan)</label>
                                <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-input" Placeholder="Enter first name"></asp:TextBox>
                            </div>

                            <div class="form-group">
                                <label>Middle Name (Gitnang Pangalan)</label>
                                <asp:TextBox ID="txtMiddleName" runat="server" CssClass="form-input" Placeholder="Optional"></asp:TextBox>
                            </div>

                            <div class="form-group">
                                <label>Email Address</label>
                                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-input" TextMode="Email" Placeholder="Enter email address"></asp:TextBox>
                            </div>

                            <div class="form-group full-width">
                                <label>Contact Number</label>
                                <asp:TextBox ID="txtContactNumber" runat="server" CssClass="form-input" Placeholder="e.g., 09123456789"></asp:TextBox>
                            </div>
                        </div>

                        <div class="form-actions">
                            <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="btn-save" OnClick="btnSave_Click" />
                        </div>
                    </div>
                </div>

            </div>
        </div>

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
    </form>
</body>
</html>