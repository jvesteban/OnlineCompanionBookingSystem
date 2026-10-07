<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminBookings.aspx.cs" Inherits="OnlineCompanionBookingSystem.Admin.Bookings" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Booking Management - Admin Panel</title>
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/AdminDashboard.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/AdminBookings.css" rel="stylesheet" runat="server" />
    <!-- SweetAlert2 CDN -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <!-- Chart.js CDN -->
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
</head>
<body>
    <form id="form1" runat="server">

        <div class="admin-wrapper">

            <!-- ===== SIDEBAR ===== -->
            <div class="admin-sidebar">
                <div class="admin-sidebar-logo">
                    <span class="brand-text">OCBMS</span> <span class="badge-admin">ADMIN</span>
                </div>

                <div class="admin-sidebar-nav">
                    <asp:HyperLink ID="NavDashboard" runat="server" NavigateUrl="~/Admin/AdminDashboard.aspx" CssClass="admin-sidebar-link">
                        Dashboard Overview
                    </asp:HyperLink>
                    <asp:HyperLink ID="NavVerification" runat="server" NavigateUrl="~/Admin/AdminVerification.aspx" CssClass="admin-sidebar-link">
                        Verification Requests
                        <span class="nav-count" data-nav-count="verification" hidden></span>
                    </asp:HyperLink>
                    <asp:HyperLink ID="NavCompanions" runat="server" NavigateUrl="~/Admin/AdminCompanion.aspx" CssClass="admin-sidebar-link">
                        Companions List
                    </asp:HyperLink>
                    <asp:HyperLink ID="NavCustomers" runat="server" NavigateUrl="~/Admin/AdminCustomers.aspx" CssClass="admin-sidebar-link">
                        Customers Directory
                    </asp:HyperLink>
                    <asp:HyperLink ID="NavBookings" runat="server" NavigateUrl="~/Admin/AdminBookings.aspx" CssClass="admin-sidebar-link active">
                        System Bookings
                    </asp:HyperLink>
                </div>

                <div class="admin-sidebar-footer">
                    <asp:Button ID="btnLogout" runat="server" Text="Log Out" CssClass="admin-logout-btn" OnClick="btnLogout_Click" OnClientClick="return confirm('Are you sure you want to log out?');" />
                </div>
            </div>

            <!-- ===== MAIN CONTENT ===== -->
            <div class="admin-main">

                <div class="admin-topbar">
                    <h2>Booking Management</h2>
                    <div class="admin-topbar-user-wrapper">
                        <div class="admin-topbar-user" id="adminUserToggle" onclick="toggleAdminDropdown(event)">
                            <div class="admin-avatar">A</div>
                            <div class="admin-user-meta">
                                <span class="admin-name-label">Administrator</span>
                                <span class="admin-role-label">System Owner</span>
                            </div>
                            <span class="dropdown-caret">&#9662;</span>
                        </div>

                        <!-- Professional Clean Dropdown Card -->
                        <div class="admin-dropdown" id="adminDropdown">
                            <div class="dropdown-header">
                                <div class="admin-avatar-lg">A</div>
                                <div class="dropdown-user-info">
                                    <p class="dropdown-name">Administrator</p>
                                    <p class="dropdown-email">admin@ocbs.com</p>
                                    <span class="dropdown-role-pill">Super Admin</span>
                                </div>
                            </div>
                            <div class="dropdown-divider"></div>
                            <asp:LinkButton ID="hlAdminProfile" runat="server" CssClass="dropdown-item" OnClientClick="openAdminProfileModal(); return false;">
                                <span class="dropdown-icon"><svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="8" r="4" /><path d="M4 21c0-4 3-7 8-7s8 3 8 7" /></svg></span> View Administrator Profile
                            </asp:LinkButton>
                            <div class="dropdown-divider"></div>
                            <asp:Button ID="btnDropdownLogout" runat="server" Text="Log Out Securely" CssClass="dropdown-item dropdown-logout" OnClick="btnLogout_Click" OnClientClick="return confirm('Are you sure you want to log out?');" />
                        </div>
                    </div>
                </div>

                <!-- ===== STATS SUMMARY ===== -->
                <div class="admin-stats-grid">
                    <div class="admin-stat-card">
                        <div class="admin-stat-indicator">01</div>
                        <div>
                            <h3><asp:Literal ID="litTotalBookings" runat="server" Text="0" /></h3>
                            <p>Total Bookings</p>
                        </div>
                    </div>
                    <div class="admin-stat-card">
                        <div class="admin-stat-indicator">02</div>
                        <div>
                            <h3><asp:Literal ID="litConfirmedBookings" runat="server" Text="0" /></h3>
                            <p>Confirmed / Active</p>
                        </div>
                    </div>
                    <div class="admin-stat-card warning">
                        <div class="admin-stat-indicator warning-ind">03</div>
                        <div>
                            <h3><asp:Literal ID="litPendingBookings" runat="server" Text="0" /></h3>
                            <p>Pending Requests</p>
                        </div>
                    </div>
                    <div class="admin-stat-card danger">
                        <div class="admin-stat-indicator danger-ind">04</div>
                        <div>
                            <h3><asp:Literal ID="litCancelledBookings" runat="server" Text="0" /></h3>
                            <p>Cancelled / Declined</p>
                        </div>
                    </div>
                </div>

                <!-- ===== CHARTS SECTION ===== -->
                <div class="admin-charts-grid">
                    <div class="admin-chart-card">
                        <div class="admin-chart-header">
                            <h3>Booking Status Breakdown</h3>
                        </div>
                        <div class="chart-container" style="position: relative; height: 240px;">
                            <canvas id="chartBookingStatus"></canvas>
                        </div>
                    </div>

                    <div class="admin-chart-card">
                        <div class="admin-chart-header">
                            <h3>Top Booked Packages</h3>
                        </div>
                        <div class="chart-container" style="position: relative; height: 240px;">
                            <canvas id="chartTopPackages"></canvas>
                        </div>
                    </div>
                </div>

                <!-- ===== SEARCH AND FILTER BAR ===== -->
                <div class="booking-filter-bar">
                    <div class="filter-tabs">
                        <asp:LinkButton ID="tabAll" runat="server" CssClass="filter-tab active" OnClick="tabAll_Click">All Bookings</asp:LinkButton>
                        <asp:LinkButton ID="tabPending" runat="server" CssClass="filter-tab" OnClick="tabPending_Click">Pending</asp:LinkButton>
                        <asp:LinkButton ID="tabConfirmed" runat="server" CssClass="filter-tab" OnClick="tabConfirmed_Click">Confirmed</asp:LinkButton>
                        <asp:LinkButton ID="tabCompleted" runat="server" CssClass="filter-tab" OnClick="tabCompleted_Click">Completed</asp:LinkButton>
                        <asp:LinkButton ID="tabCancelled" runat="server" CssClass="filter-tab" OnClick="tabCancelled_Click">Cancelled</asp:LinkButton>
                    </div>

                    <div class="search-box">
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="form-input search-input" placeholder="Search customer, companion, package..."></asp:TextBox>
                        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="admin-small-btn btn-view" OnClick="btnSearch_Click" />
                    </div>
                </div>

                <!-- ===== BOOKINGS TABLE ===== -->
                <div class="admin-section">
                    <table class="admin-table">
                        <thead>
                            <tr>
                                <th>Booking ID</th>
                                <th>Customer</th>
                                <th>Companion</th>
                                <th>Package / Activity</th>
                                <th>Booking Date & Time</th>
                                <th>Rate</th>
                                <th>Status</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptBookings" runat="server" OnItemCommand="rptBookings_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td><strong>#<%#: Eval("BookingID") %></strong></td>
                                        <td><%#: Eval("CustomerName") %></td>
                                        <td><%#: Eval("CompanionName") %></td>
                                        <td><%#: Eval("PackageName") %></td>
                                        <td>
                                            <%#: Eval("BookingDate", "{0:MMM dd, yyyy}") %><br />
                                            <span class="booking-time-sub"><%#: Eval("BookingTime") %></span>
                                        </td>
                                        <td>&#8369;<%#: Eval("Rate", "{0:N2}") %></td>
                                        <td>
                                            <span class='pill <%# GetStatusPillClass(Eval("Status").ToString()) %>'>
                                                <%#: Eval("Status") %>
                                            </span>
                                        </td>
                                        <td>
                                            <button type="button" class="admin-small-btn btn-view"
                                                onclick="openBookingModal(
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("BookingID")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("CustomerName")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("CompanionName")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("PackageName")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("BookingDate", "{0:MMMM dd, yyyy}")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("BookingTime")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("Rate", "{0:N2}")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("Status")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("DateCreated", "{0:MMM dd, yyyy hh:mm tt}")) %>'
                                                )">View</button>

                                            <asp:Button ID="btnCancel" runat="server" Text="Cancel"
                                                CssClass="admin-small-btn btn-reject"
                                                CommandName="CancelBooking"
                                                CommandArgument='<%# Eval("BookingID") %>'
                                                Visible='<%# Eval("Status").ToString() == "Pending" || Eval("Status").ToString() == "Confirmed" %>' />
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>

                    <asp:Label ID="lblEmpty" runat="server" CssClass="empty-state-text" Text="No booking records found." Visible="false" />
                </div>

            </div>
        </div>

        <!-- ===== BOOKING DETAILS MODAL ===== -->
        <div class="admin-modal-overlay" id="bookingDetailModal">
            <div class="admin-modal">
                <div class="admin-modal-header">
                    <div class="modal-title-group">
                        <h3>Booking Details (#<span id="bkModalId">&mdash;</span>)</h3>
                        <p class="modal-subtitle">Comprehensive transaction information</p>
                    </div>
                    <button type="button" class="admin-modal-close" onclick="closeBookingModal()">&times;</button>
                </div>
                <div class="admin-modal-body">
                    <div class="admin-modal-info-grid">
                        <div class="info-item">
                            <span class="info-label">Customer Name</span>
                            <span class="info-value" id="bkModalCustomer">&mdash;</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">Companion Name</span>
                            <span class="info-value" id="bkModalCompanion">&mdash;</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">Package Selected</span>
                            <span class="info-value" id="bkModalPackage">&mdash;</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">Schedule Date</span>
                            <span class="info-value" id="bkModalDate">&mdash;</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">Schedule Time</span>
                            <span class="info-value" id="bkModalTime">&mdash;</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">Package Rate</span>
                            <span class="info-value text-accent" id="bkModalRate">&#8369;0.00</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">Current Status</span>
                            <span class="info-value" id="bkModalStatus">&mdash;</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">Date Created</span>
                            <span class="info-value" id="bkModalCreated">&mdash;</span>
                        </div>
                    </div>
                </div>
                <div class="admin-modal-footer">
                    <button type="button" class="admin-modal-action-btn" onclick="closeBookingModal()">Close Window</button>
                </div>
            </div>
        </div>

        <!-- ===== ADMIN PROFILE MODAL ===== -->
        <div class="admin-modal-overlay" id="adminProfileModal">
            <div class="admin-modal">
                <div class="admin-modal-header">
                    <div class="modal-title-group">
                        <h3>Administrator Profile</h3>
                        <p class="modal-subtitle">System credentials and account details</p>
                    </div>
                    <button type="button" class="admin-modal-close" onclick="closeAdminProfileModal()">&times;</button>
                </div>
                <div class="admin-modal-body">
                    <div class="admin-modal-avatar-section">
                        <div class="admin-avatar-xl">A</div>
                        <h4 id="modalAdminName"><asp:Literal ID="litAdminName" runat="server" Text="Administrator" /></h4>
                        <p class="admin-modal-email-text"><asp:Literal ID="litAdminEmail" runat="server" Text="admin@ocbs.com" /></p>
                    </div>
                    <div class="admin-modal-info-grid">
                        <div class="info-item">
                            <span class="info-label">Contact Number</span>
                            <span class="info-value"><asp:Literal ID="litAdminContact" runat="server" Text="N/A" /></span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">Date Registered</span>
                            <span class="info-value"><asp:Literal ID="litAdminDateCreated" runat="server" Text="N/A" /></span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">Access Level</span>
                            <span class="info-value text-accent">Full System Control</span>
                        </div>
                    </div>
                </div>
                <div class="admin-modal-footer">
                    <button type="button" class="admin-modal-action-btn" onclick="closeAdminProfileModal()">Close Window</button>
                </div>
            </div>
        </div>

        <asp:HiddenField ID="hfTopPackageNames" runat="server" />
        <asp:HiddenField ID="hfTopPackageCounts" runat="server" />

        <script src="../Scripts/site.js"></script>
        <script type="text/javascript">
            function openBookingModal(id, customer, companion, pkg, date, time, rate, status, created) {
                var elId = document.getElementById('bkModalId');
                var elCustomer = document.getElementById('bkModalCustomer');
                var elCompanion = document.getElementById('bkModalCompanion');
                var elPackage = document.getElementById('bkModalPackage');
                var elDate = document.getElementById('bkModalDate');
                var elTime = document.getElementById('bkModalTime');
                var elRate = document.getElementById('bkModalRate');
                var elStatus = document.getElementById('bkModalStatus');
                var elCreated = document.getElementById('bkModalCreated');
                var modal = document.getElementById('bookingDetailModal');

                if (elId) elId.textContent = id || '';
                if (elCustomer) elCustomer.textContent = customer || '';
                if (elCompanion) elCompanion.textContent = companion || '';
                if (elPackage) elPackage.textContent = pkg || '';
                if (elDate) elDate.textContent = date || '';
                if (elTime) elTime.textContent = time || '';
                if (elRate) elRate.textContent = '\u20B1' + (rate || '0.00');
                if (elStatus) elStatus.textContent = status || '';
                if (elCreated) elCreated.textContent = created || '';

                if (modal) modal.classList.add('show');
            }

            function closeBookingModal() {
                var modal = document.getElementById('bookingDetailModal');
                if (modal) modal.classList.remove('show');
            }

            window.addEventListener("load", function () {
                var ChartClass = window['Chart'];
                if (typeof ChartClass === 'undefined') return;

                // 1. Booking Status Doughnut Chart
                var confirmed = parseInt('<%= litConfirmedBookings.Text %>') || 0;
                var pending = parseInt('<%= litPendingBookings.Text %>') || 0;
                var cancelled = parseInt('<%= litCancelledBookings.Text %>') || 0;

                var elDoughnut = document.getElementById('chartBookingStatus');
                if (elDoughnut) {
                    new ChartClass(elDoughnut.getContext('2d'), {
                        type: 'doughnut',
                        data: {
                            labels: ['Confirmed', 'Pending', 'Cancelled/Declined'],
                            datasets: [{
                                data: [confirmed, pending, cancelled],
                                backgroundColor: ['#10b981', '#f59e0b', '#ef4444'],
                                borderWidth: 0
                            }]
                        },
                        options: {
                            responsive: true,
                            animation: false,   // no animation: the bars and rings appear at once
                            maintainAspectRatio: false,
                            plugins: {
                                legend: { position: 'bottom', labels: { color: '#64748b', font: { size: 12 } } }
                            }
                        }
                    });
                }

                // 2. Top Packages Bar Chart
                var hfPkgs = document.getElementById('<%= hfTopPackageNames.ClientID %>');
                var hfCounts = document.getElementById('<%= hfTopPackageCounts.ClientID %>');

                var pkgs = hfPkgs && hfPkgs.value ? hfPkgs.value.split(',') : ['No Data'];
                var counts = hfCounts && hfCounts.value ? hfCounts.value.split(',').map(Number) : [0];

                var elBar = document.getElementById('chartTopPackages');
                if (elBar) {
                    new ChartClass(elBar.getContext('2d'), {
                        type: 'bar',
                        data: {
                            labels: pkgs,
                            datasets: [{
                                label: 'Total Bookings',
                                data: counts,
                                backgroundColor: 'rgba(16, 185, 129, 0.8)',
                                borderColor: '#10b981',
                                borderWidth: 1,
                                borderRadius: 6
                            }]
                        },
                        options: {
                            responsive: true,
                            animation: false,   // no animation: the bars and rings appear at once
                            maintainAspectRatio: false,
                            plugins: { legend: { display: false } },
                            scales: {
                                y: { beginAtZero: true, ticks: { stepSize: 1, color: '#64748b' }, grid: { color: 'rgba(0, 0, 0, 0.04)' } },
                                x: { grid: { display: false }, ticks: { color: '#64748b' } }
                            }
                        }
                    });
                }
            });
        </script>

        <script src="../Scripts/nav-counts.js"></script>
    </form>
</body>
</html>