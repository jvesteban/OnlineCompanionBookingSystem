<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CustomerMyBookings.aspx.cs" Inherits="OnlineCompanionBookingManagementSystem.Customer.MyBookings" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>My Bookings - Online Companion Booking System</title>
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/CustomerDashboard.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/CustomerMyBookings.css" rel="stylesheet" runat="server" />
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
                        <h2>My Bookings</h2>
                        <p class="topbar-subtitle">Manage and track your companion booking schedules seamlessly.</p>
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

                <!-- ===== BOOKINGS CONTENT CONTAINER ===== -->
                <div class="section-block">
                    <asp:Label ID="lblMessage" runat="server" CssClass="message" Visible="false" />
                    
                    <div class="table-container-card">
                        <asp:GridView ID="gvMyBookings" runat="server" AutoGenerateColumns="False" DataKeyNames="BookingID" OnRowCommand="gvMyBookings_RowCommand" CssClass="table-bookings" GridLines="None" UseAccessibleHeader="true">
                            <Columns>
                                <asp:BoundField DataField="BookingID" HeaderText="Ref ID" />
                                <asp:BoundField DataField="CompanionName" HeaderText="Companion" />
                                <asp:BoundField DataField="PackageName" HeaderText="Package" />
                                <asp:BoundField DataField="Activities" HeaderText="Activities" NullDisplayText="-" />
                                <asp:BoundField DataField="BookingDate" HeaderText="Schedule Date" DataFormatString="{0:MMM. dd, yyyy}" />
                                <asp:BoundField DataField="BookingTime" HeaderText="Time" />
                                
                                <asp:TemplateField HeaderText="Status">
                                    <ItemTemplate>
                                        <span class="status-badge status-<%#: Eval("StatusKey") %>" data-booking-start="<%#: Eval("StartIso") %>" data-booking-end="<%#: Eval("EndIso") %>" data-now="<%#: Eval("ServerNowIso") %>" data-ongoing-note="<%#: Eval("OngoingNote") %>" data-awaiting-note="<%#: Eval("AwaitingNote") %>" data-note-id="sn-<%#: Eval("BookingID") %>">
                                            <%#: Eval("DisplayStatus") %>
                                        </span>
                                        <div class="booking-status-note" id="sn-<%#: Eval("BookingID") %>"><%#: Eval("StatusNote") %></div>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <div class="table-actions">
                                            <!-- Cancel: Pending, or Confirmed and not started yet (StatusKey is worked out from the date and time) -->
                                            <asp:Button ID="btnCancel" runat="server" CommandName="CancelBooking" CommandArgument='<%# Eval("BookingID") %>' Text="Cancel" CssClass="btn btn-cancel" Visible='<%# Eval("StatusKey").ToString() == "pending" || Eval("StatusKey").ToString() == "confirmed" %>' OnClientClick="return confirm('Cancel this booking? This cannot be undone.');" />
                                            
                                            <!-- Rate Button for Completed Bookings -->
                                            <asp:HyperLink ID="lnkRate" runat="server" NavigateUrl='<%# ResolveUrl("~/Customer/RateCompanion.aspx?BookingID=" + Eval("BookingID")) %>' CssClass="btn btn-rate" Visible='<%# Eval("Status").ToString() == "Completed" && Eval("HasRating").ToString() == "0" %>'>
                                                <svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><path d="m12 3 2.8 5.7 6.2.9-4.5 4.4 1.1 6.2-5.6-2.9-5.6 2.9 1.1-6.2L3 9.6l6.2-.9L12 3z" /></svg>
                                                <span>Rate Service</span>
                                            </asp:HyperLink>
                                            <asp:Label ID="lblRated" runat="server" Text="Rated" CssClass="btn btn-rated" Visible='<%# Eval("Status").ToString() == "Completed" && Eval("HasRating").ToString() == "1" %>' />
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>

                        <!-- Empty State Panel kung walang bookings -->
                        <asp:Panel ID="pnlEmptyBookings" runat="server" Visible="false" CssClass="empty-bookings-panel">
                            <div class="empty-icon"><svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="5" width="18" height="16" rx="2" /><path d="M16 3v4M8 3v4M3 10h18" /></svg></div>
                            <h4>No Bookings Found</h4>
                            <p>You haven't scheduled any companion bookings yet. Explore our trusted companions to get started.</p>
                            <asp:HyperLink ID="hlBrowseCompanions" runat="server" NavigateUrl="~/Customer/BrowseCompanion.aspx" CssClass="btn-browse-action">
                                Browse Companions
                            </asp:HyperLink>
                        </asp:Panel>
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
        <script src="../Scripts/booking-status.js"></script>
    </form>
</body>
</html>