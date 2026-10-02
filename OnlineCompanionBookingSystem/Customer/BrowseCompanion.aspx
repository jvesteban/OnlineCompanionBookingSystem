<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BrowseCompanion.aspx.cs" Inherits="OnlineCompanionBookingManagementSystem.Customer.BrowseCompanions" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Browse Companions - Online Companion Booking System</title>
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/CustomerDashboard.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/BrowseCompanion.css" rel="stylesheet" runat="server" />
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
                        <span class="brand-title">OCBMS</span>
                    </div>

                    <div class="sidebar-nav">
                        <asp:HyperLink ID="NavDashboard" runat="server" NavigateUrl="~/Customer/Dashboard.aspx" CssClass="sidebar-link">
                            <span>Dashboard</span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="NavCompanions" runat="server" NavigateUrl="~/Customer/BrowseCompanion.aspx" CssClass="sidebar-link active">
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
                        <h2>Find Your Companion</h2>
                        <p class="topbar-subtitle">Discover background-checked, trusted companions tailored for your activities.</p>
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

                <!-- ===== SEARCH & FILTER CONTROLS ===== -->
                <div class="browse-filter-card">
                    <div class="filter-inputs-grid">
                        <div class="filter-group search-group">
                            <label>Search Companion</label>
                            <asp:TextBox ID="txtSearch" runat="server" CssClass="filter-input" Placeholder="Search by name or keyword..."></asp:TextBox>
                        </div>

                        <div class="filter-group">
                            <label>Activity / Service</label>
                            <asp:DropDownList ID="ddlActivity" runat="server" CssClass="filter-select">
                                <asp:ListItem Text="All Activities" Value="All" />
                            </asp:DropDownList>
                        </div>

                        <div class="filter-group">
                            <label>Sort By</label>
                            <asp:DropDownList ID="ddlSortBy" runat="server" CssClass="filter-select">
                                <asp:ListItem Text="Highest Rated" Value="RatingDesc" />
                                <asp:ListItem Text="Most Reviews" Value="ReviewsDesc" />
                                <asp:ListItem Text="Price: Low to High" Value="PriceAsc" />
                                <asp:ListItem Text="Price: High to Low" Value="PriceDesc" />
                            </asp:DropDownList>
                        </div>

                        <div class="filter-actions">
                            <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn-filter-apply" OnClick="btnFilter_Click" />
                            <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn-filter-reset" OnClick="btnReset_Click" />
                        </div>
                    </div>
                </div>

                <!-- ===== COMPANIONS GRID ===== -->
                <div class="section-block">
                    <div class="section-header">
                        <h3 class="section-title">Available Companions (<asp:Literal ID="litCompanionCount" runat="server" Text="0" />)</h3>
                    </div>

                    <div class="browse-companions-grid">
                        <asp:Repeater ID="rptCompanions" runat="server" OnItemCommand="rptCompanions_ItemCommand">
                            <ItemTemplate>
                                <div class="browse-card">
                                    <div class="browse-img-wrap">
                                        <%# GetCompanionAvatar(Eval("ProfilePicturePath"), Eval("FullName")) %>
                                        <span class="verified-badge" title="Verified Companion"><svg class="badge-icon-svg" viewBox="0 0 24 24" aria-hidden="true"><path d="m5 12 4 4L19 6" /></svg></span>
                                    </div>

                                    <div class="browse-card-body">
                                        <h4><%#: Eval("FullName") %></h4>

                                        <div class="rating-bar">
                                            <span class="stars"><%# FormatStars(Eval("AvgRating")) %></span>
                                            <span class="review-count">(<%#: Eval("TotalReviews") %> reviews)</span>
                                        </div>

                                        <span class="companion-tag"><%#: Eval("Activities") %></span>
                                        <p class="companion-availability<%# Convert.ToBoolean(Eval("HasAvailability")) ? "" : " is-unset" %>" title="Weekly availability">
                                            <svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="5" width="18" height="16" rx="2" /><path d="M16 3v4M8 3v4M3 10h18" /></svg>
                                            <span><%#: Eval("AvailabilityText") %></span>
                                        </p>
                                        <p class="companion-bio-snippet"><%# TruncateBio(Eval("Bio")) %></p>

                                        <div class="browse-card-footer">
                                            <div class="price-container">
                                                <span class="companion-price">&#8369;<%#: Eval("MinRate", "{0:N0}") %> <small>/ hr</small></span>
                                            </div>

                                            <asp:Button ID="btnViewProfile" runat="server" Text="View Profile" CssClass="btn-view-profile" 
                                                CommandName="ViewProfile" CommandArgument='<%# Eval("CompanionID") %>' />
                                        </div>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <asp:Panel ID="pnlNoResults" runat="server" Visible="false" CssClass="no-results-panel">
                        <div class="no-results-icon"><svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><path d="M3 7h7l2 2h9v10H3z" /><path d="M3 7V5h7l2 2" /></svg></div>
                        <h4>No Companions Found</h4>
                        <p>We couldn't find any companions matching your search or filter criteria. Try clearing some filters.</p>
                        <asp:Button ID="btnResetEmpty" runat="server" Text="Clear Filters" CssClass="btn-filter-apply" OnClick="btnReset_Click" />
                    </asp:Panel>
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