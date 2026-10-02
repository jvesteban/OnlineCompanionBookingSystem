<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminCustomers.aspx.cs" Inherits="OnlineCompanionBookingSystem.Admin.AdminCustomers" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Customer Management - Admin Panel</title>
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/AdminDashboard.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/AdminCustomers.css" rel="stylesheet" runat="server" />
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
                    <asp:HyperLink ID="NavCustomers" runat="server" NavigateUrl="~/Admin/AdminCustomers.aspx" CssClass="admin-sidebar-link active">
                        Customers Directory
                    </asp:HyperLink>
                    <asp:HyperLink ID="NavBookings" runat="server" NavigateUrl="~/Admin/AdminBookings.aspx" CssClass="admin-sidebar-link">
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
                    <h2>Customer Management</h2>
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
                            <h3><asp:Literal ID="litTotalCustomers" runat="server" Text="0" /></h3>
                            <p>Total Registered Customers</p>
                        </div>
                    </div>
                    <div class="admin-stat-card">
                        <div class="admin-stat-indicator">02</div>
                        <div>
                            <h3><asp:Literal ID="litActiveCustomers" runat="server" Text="0" /></h3>
                            <p>Active Accounts</p>
                        </div>
                    </div>
                    <div class="admin-stat-card danger">
                        <div class="admin-stat-indicator warning-ind">03</div>
                        <div>
                            <h3><asp:Literal ID="litInactiveCustomers" runat="server" Text="0" /></h3>
                            <p>Deactivated / Suspended</p>
                        </div>
                    </div>
                </div>

                <!-- ===== CHARTS SECTION ===== -->
                <div class="admin-charts-grid">
                    <div class="admin-chart-card">
                        <div class="admin-chart-header">
                            <h3>Top Customers by Bookings</h3>
                        </div>
                        <div class="chart-container" style="position: relative; height: 240px;">
                            <canvas id="chartTopCustomers"></canvas>
                        </div>
                    </div>

                    <div class="admin-chart-card">
                        <div class="admin-chart-header">
                            <h3>Account Status Distribution</h3>
                        </div>
                        <div class="chart-container" style="position: relative; height: 240px;">
                            <canvas id="chartStatusRatio"></canvas>
                        </div>
                    </div>
                </div>

                <!-- ===== SEARCH AND FILTER BAR ===== -->
                <div class="customer-filter-bar">
                    <div class="filter-tabs">
                        <asp:LinkButton ID="tabAll" runat="server" CssClass="filter-tab active" OnClick="tabAll_Click">All Customers</asp:LinkButton>
                        <asp:LinkButton ID="tabActive" runat="server" CssClass="filter-tab" OnClick="tabActive_Click">Active</asp:LinkButton>
                        <asp:LinkButton ID="tabInactive" runat="server" CssClass="filter-tab" OnClick="tabInactive_Click">Deactivated</asp:LinkButton>
                    </div>

                    <div class="search-box">
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="form-input search-input" placeholder="Search by name or email..."></asp:TextBox>
                        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="admin-small-btn btn-view" OnClick="btnSearch_Click" />
                    </div>
                </div>

                <!-- ===== CUSTOMERS TABLE ===== -->
                <div class="admin-section">
                    <table class="admin-table">
                        <thead>
                            <tr>
                                <th>Customer Name</th>
                                <th>Email</th>
                                <th>Contact Number</th>
                                <th>Total Bookings</th>
                                <th>Date Registered</th>
                                <th>Account Status</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptCustomers" runat="server" OnItemCommand="rptCustomers_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td>
                                            <div class="customer-profile-cell">
                                                <div class="customer-avatar"><%# GetCustomerAvatar(Eval("ProfilePicture"), Eval("FullName")) %></div>
                                                <span class="customer-name"><%#: Eval("FullName") %></span>
                                            </div>
                                        </td>
                                        <td><%#: Eval("Email") %></td>
                                        <td><%#: Eval("ContactNumber") != DBNull.Value && !string.IsNullOrEmpty(Eval("ContactNumber").ToString()) ? Eval("ContactNumber") : "N/A" %></td>
                                        <td><strong><%#: Eval("TotalBookings") %></strong></td>
                                        <td><%#: Eval("DateCreated", "{0:MMM dd, yyyy}") %></td>
                                        <td>
                                            <span class='pill <%# Convert.ToBoolean(Eval("IsActive")) ? "confirmed" : "rejected" %>'>
                                                <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Deactivated" %>
                                            </span>
                                        </td>
                                        <td>
                                            <button type="button" class="admin-small-btn btn-view"
                                                onclick="openCustomerModal(
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("FullName")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("Email")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("ContactNumber")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("DateCreated", "{0:MMMM dd, yyyy}")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("TotalBookings")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Deactivated") %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(GetPhotoUrl(Eval("ProfilePicture"))) %>'
                                                )">View</button>

                                            <asp:LinkButton ID="btnToggleStatus" runat="server" 
                                                CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "admin-small-btn btn-reject" : "admin-small-btn btn-approve" %>'
                                                CommandName="ToggleStatus" 
                                                CommandArgument='<%# Eval("UserID") + "|" + Eval("IsActive") %>'
                                                Text='<%# Convert.ToBoolean(Eval("IsActive")) ? "Deactivate" : "Activate" %>' />
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>

                    <asp:Label ID="lblEmpty" runat="server" CssClass="empty-state-text" Text="No customer records found." Visible="false" />
                </div>

            </div>
        </div>

        <!-- ===== CUSTOMER DETAILS MODAL ===== -->
        <div class="admin-modal-overlay" id="customerDetailModal">
            <div class="admin-modal">
                <div class="admin-modal-header">
                    <div class="modal-title-group">
                        <h3>Customer Profile</h3>
                        <p class="modal-subtitle">Full account review and details</p>
                    </div>
                    <button type="button" class="admin-modal-close" onclick="closeCustomerModal()">&times;</button>
                </div>
                <div class="admin-modal-body">
                    <div class="admin-modal-avatar-section">
                        <div class="admin-avatar-xl" id="cModalAvatar">C</div>
                        <h4 id="cModalName">&mdash;</h4>
                        <p id="cModalEmail" class="admin-modal-email-text">&mdash;</p>
                    </div>
                    <div class="admin-modal-info-grid">
                        <div class="info-item">
                            <span class="info-label">Contact Number</span>
                            <span class="info-value" id="cModalContact">&mdash;</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">Date Registered</span>
                            <span class="info-value" id="cModalDate">&mdash;</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">Total Bookings Made</span>
                            <span class="info-value" id="cModalBookings">0</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">Account Status</span>
                            <span class="info-value text-accent" id="cModalStatus">&mdash;</span>
                        </div>
                    </div>
                </div>
                <div class="admin-modal-footer">
                    <button type="button" class="admin-modal-action-btn" onclick="closeCustomerModal()">Close Window</button>
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

        <asp:HiddenField ID="hfTopCustomerNames" runat="server" />
        <asp:HiddenField ID="hfTopCustomerBookings" runat="server" />

        <script src="../Scripts/site.js"></script>
        <script type="text/javascript">
            function openCustomerModal(name, email, contact, date, bookings, status, photo) {
                var elName = document.getElementById('cModalName');
                var elEmail = document.getElementById('cModalEmail');
                var elContact = document.getElementById('cModalContact');
                var elDate = document.getElementById('cModalDate');
                var elBookings = document.getElementById('cModalBookings');
                var elStatus = document.getElementById('cModalStatus');
                var elAvatar = document.getElementById('cModalAvatar');
                var modal = document.getElementById('customerDetailModal');

                if (elName) elName.textContent = name || '';
                if (elEmail) elEmail.textContent = email || '';
                if (elContact) elContact.textContent = contact || 'N/A';
                if (elDate) elDate.textContent = date || '';
                if (elBookings) elBookings.textContent = bookings || '0';
                if (elStatus) elStatus.textContent = status || '';

                if (elAvatar) {
                    var strName = String(name || '');
                    var initials = strName ? strName.split(' ').map(function (n) { return n[0]; }).join('').substring(0, 2).toUpperCase() : 'CU';
                    elAvatar.textContent = initials;
                    // Show the customer's uploaded photo instead of the initials when there is one
                    if (photo) {
                        var img = document.createElement('img');
                        img.src = photo;
                        img.alt = strName;
                        img.className = 'customer-avatar-img';
                        elAvatar.textContent = '';
                        elAvatar.appendChild(img);
                    }
                }

                if (modal) modal.classList.add('show');
            }

            function closeCustomerModal() {
                var modal = document.getElementById('customerDetailModal');
                if (modal) modal.classList.remove('show');
            }

            window.addEventListener("load", function () {
                var ChartClass = window['Chart'];
                if (typeof ChartClass === 'undefined') return;

                // 1. Render Top Customers Bar Chart
                var hfNames = document.getElementById('<%= hfTopCustomerNames.ClientID %>');
                var hfBookings = document.getElementById('<%= hfTopCustomerBookings.ClientID %>');

                var names = hfNames && hfNames.value ? hfNames.value.split(',') : ['No Data'];
                var bookings = hfBookings && hfBookings.value ? hfBookings.value.split(',').map(Number) : [0];

                var elBar = document.getElementById('chartTopCustomers');
                if (elBar) {
                    new ChartClass(elBar.getContext('2d'), {
                        type: 'bar',
                        data: {
                            labels: names,
                            datasets: [{
                                label: 'Bookings',
                                data: bookings,
                                backgroundColor: 'rgba(14, 165, 233, 0.8)',
                                borderColor: '#0ea5e9',
                                borderWidth: 1,
                                borderRadius: 6
                            }]
                        },
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            plugins: { legend: { display: false } },
                            scales: {
                                y: { beginAtZero: true, ticks: { stepSize: 1, color: '#64748b' }, grid: { color: 'rgba(0, 0, 0, 0.04)' } },
                                x: { grid: { display: false }, ticks: { color: '#64748b' } }
                            }
                        }
                    });
                }

                // 2. Render Status Ratio Doughnut Chart
                var activeCount = parseInt('<%= litActiveCustomers.Text %>') || 0;
                var inactiveCount = parseInt('<%= litInactiveCustomers.Text %>') || 0;

                var elDoughnut = document.getElementById('chartStatusRatio');
                if (elDoughnut) {
                    new ChartClass(elDoughnut.getContext('2d'), {
                        type: 'doughnut',
                        data: {
                            labels: ['Active', 'Deactivated'],
                            datasets: [{
                                data: [activeCount, inactiveCount],
                                backgroundColor: ['#10b981', '#ef4444'],
                                borderWidth: 0
                            }]
                        },
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            plugins: {
                                legend: { position: 'bottom', labels: { color: '#64748b', font: { size: 12 } } }
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