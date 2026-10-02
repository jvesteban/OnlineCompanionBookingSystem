<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminDashboard.aspx.cs" Inherits="OnlineCompanionBookingSystem.Admin.Dashboard" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Admin Dashboard - Online Companion Booking System</title>
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/AdminDashboard.css" rel="stylesheet" runat="server" />
    <!-- Chart.js CDN para sa Visual Analytics Graphs -->
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</head>
<body>
    <form id="form1" runat="server" action="AdminDashboard.aspx">

        <div class="admin-wrapper">

            <!-- ===== SIDEBAR ===== -->
            <div class="admin-sidebar">
                <div class="admin-sidebar-logo">
                    <span class="brand-text">OCBMS</span> <span class="badge-admin">ADMIN</span>
                </div>

                <div class="admin-sidebar-nav">
                    <asp:HyperLink ID="NavDashboard" runat="server" NavigateUrl="~/Admin/AdminDashboard.aspx" CssClass="admin-sidebar-link active">
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
                    <h2>Dashboard Overview</h2>
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

                <!-- ===== STATS ===== -->
                <div class="admin-stats-grid">
                    <div class="admin-stat-card">
                        <div class="admin-stat-indicator">01</div>
                        <div>
                            <h3><span id="statCustomers"><asp:Literal ID="litTotalCustomers" runat="server" Text="0" /></span></h3>
                            <p>Total Customers</p>
                        </div>
                    </div>
                    <div class="admin-stat-card">
                        <div class="admin-stat-indicator">02</div>
                        <div>
                            <h3><span id="statCompanions"><asp:Literal ID="litTotalCompanions" runat="server" Text="0" /></span></h3>
                            <p>Total Companions</p>
                        </div>
                    </div>
                    <div class="admin-stat-card warning">
                        <div class="admin-stat-indicator warning-ind">03</div>
                        <div>
                            <h3><span id="statPending"><asp:Literal ID="litPendingVerifications" runat="server" Text="0" /></span></h3>
                            <p>Pending Verifications</p>
                        </div>
                    </div>
                    <div class="admin-stat-card">
                        <div class="admin-stat-indicator">04</div>
                        <div>
                            <h3><span id="statBookings"><asp:Literal ID="litTotalBookings" runat="server" Text="0" /></span></h3>
                            <p>Total Bookings</p>
                        </div>
                    </div>
                </div>

                <!-- ===== VISUAL ANALYTICS CHARTS ===== -->
                <div class="admin-charts-grid">
                    <div class="admin-chart-card">
                        <div class="admin-chart-header">
                            <h3>System Metrics Overview</h3>
                        </div>
                        <div class="chart-container" style="position: relative; height: 260px;">
                            <canvas id="barChartStats"></canvas>
                        </div>
                    </div>

                    <div class="admin-chart-card">
                        <div class="admin-chart-header">
                            <h3>User Role Distribution</h3>
                        </div>
                        <div class="chart-container" style="position: relative; height: 260px;">
                            <canvas id="doughnutChartStats"></canvas>
                        </div>
                    </div>
                </div>

                <!-- ===== PENDING VERIFICATIONS PREVIEW ===== -->
                <div class="admin-section">
                    <div class="admin-section-header">
                        <h3>Pending Verifications</h3>
                        <asp:HyperLink ID="ViewAllVerifications" runat="server" NavigateUrl="~/Admin/AdminVerification.aspx" CssClass="admin-view-all">View All &rarr;</asp:HyperLink>
                    </div>
                    <table class="admin-table">
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Email</th>
                                <th>Date Applied</th>
                                <th>Status</th>
                                <th>Action</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptPendingVerifications" runat="server" OnItemCommand="rptPendingVerifications_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td><%#: Eval("FullName") %></td>
                                        <td><%#: Eval("Email") %></td>
                                        <td><%#: Eval("DateCreated", "{0:MMM dd, yyyy}") %></td>
                                        <td><span class="pill pending">Pending</span></td>
                                        <td>
                                            <button type="button" class="admin-small-btn btn-view"
                                                onclick="openVerificationModal(
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("FullName")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("Email")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("ContactNumber")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("Activities")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("DateCreated", "{0:MMM dd, yyyy}")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("VerificationDocPath")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("CompanionID")) %>'
                                                ); return false;">View</button>
                                            <asp:LinkButton ID="btnApprove" runat="server" Text="Approve" CssClass="admin-small-btn btn-approve" data-decision="Approve" data-name='<%#: Eval("FullName") %>'
                                                CommandName="Approve" CommandArgument='<%# Eval("CompanionID") %>' />
                                            <asp:LinkButton ID="btnReject" runat="server" Text="Reject" CssClass="admin-small-btn btn-reject" data-decision="Reject" data-name='<%#: Eval("FullName") %>'
                                                CommandName="Reject" CommandArgument='<%# Eval("CompanionID") %>' />
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                    <asp:Label ID="lblNoPending" runat="server" CssClass="empty-state-text" Visible="false" Text="No pending verifications at the moment." />
                </div>

                <!-- ===== RECENT BOOKINGS ===== -->
                <div class="admin-section">
                    <div class="admin-section-header">
                        <h3>Recent Bookings</h3>
                        <asp:HyperLink ID="ViewAllBookings" runat="server" NavigateUrl="~/Admin/AdminBookings.aspx" CssClass="admin-view-all">View All &rarr;</asp:HyperLink>
                    </div>
                    <table class="admin-table">
                        <thead>
                            <tr>
                                <th>Customer</th>
                                <th>Companion</th>
                                <th>Activity</th>
                                <th>Date</th>
                                <th>Status</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptRecentBookings" runat="server">
                                <ItemTemplate>
                                    <tr>
                                        <td><%#: Eval("CustomerName") %></td>
                                        <td><%#: Eval("CompanionName") %></td>
                                        <td><%#: Eval("PackageName") %></td>
                                        <td><%#: Eval("BookingDate", "{0:MMM dd, yyyy}") %></td>
                                        <td>
                                            <span class='pill <%# GetStatusClass(Eval("Status").ToString()) %>'>
                                                <%#: Eval("Status") %>
                                            </span>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                    <asp:Label ID="lblNoBookings" runat="server" CssClass="empty-state-text" Visible="false" Text="No bookings recorded yet." />
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

        <!-- ===== VERIFICATION DETAIL MODAL ===== -->
        <div class="admin-modal-overlay" id="verificationModal">
            <div class="admin-modal">
                <div class="admin-modal-header">
                    <div class="modal-title-group">
                        <h3>Verification Applicant</h3>
                        <p class="modal-subtitle">Review credentials before approval</p>
                    </div>
                    <button type="button" class="admin-modal-close" onclick="closeVerificationModal()">&times;</button>
                </div>
                <div class="admin-modal-body">
                    <div class="admin-modal-avatar-section">
                        <div class="admin-avatar-xl" id="vModalAvatar">?</div>
                        <h4 id="vModalName">&mdash;</h4>
                        <p class="admin-modal-email-text" id="vModalEmail">&mdash;</p>
                    </div>
                    <div class="admin-modal-info-grid">
                        <div class="info-item">
                            <span class="info-label">Contact Number</span>
                            <span class="info-value" id="vModalContact">&mdash;</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">Activities</span>
                            <span class="info-value" id="vModalActivities">&mdash;</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">Date Applied</span>
                            <span class="info-value" id="vModalDate">&mdash;</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">Uploaded Document</span>
                            <span class="info-value">
                                <a href="#" id="vModalDoc" target="_blank" class="v-doc-link">View Document</a>
                            </span>
                        </div>
                    </div>
                </div>
                <div class="admin-modal-footer" style="justify-content: space-between;">
                    <button type="button" class="admin-small-btn btn-reject" onclick="closeVerificationModal()">Close</button>
                    <div style="display: flex; gap: 8px;">
                        <asp:LinkButton ID="btnModalApprove" runat="server" Text="Approve" CssClass="admin-small-btn btn-approve" data-decision="Approve" data-name-from="vModalName"
                            OnClick="btnModalApprove_Click" />
                        <asp:LinkButton ID="btnModalReject" runat="server" Text="Reject" CssClass="admin-small-btn btn-reject" data-decision="Reject" data-name-from="vModalName"
                            OnClick="btnModalReject_Click" />
                    </div>
                </div>
            </div>
        </div>

        <asp:HiddenField ID="hfSelectedCompanionId" runat="server" />

        <div id="toast" class="toast"></div>
        <script src="../Scripts/site.js"></script>

        <!-- Scripts para sa Modal at Charts -->
        <script type="text/javascript">
            function openVerificationModal(name, email, contact, activities, dateApplied, docPath, companionId) {
                document.getElementById('vModalName').textContent = name || '';
                document.getElementById('vModalEmail').textContent = email || '';
                document.getElementById('vModalContact').textContent = contact || 'N/A';
                document.getElementById('vModalActivities').textContent = activities || 'None specified';
                document.getElementById('vModalDate').textContent = dateApplied || '';
                
                var docLink = document.getElementById('vModalDoc');
                if (docPath && docPath !== '') {
                    docLink.href = docPath;
                    docLink.style.display = 'inline-block';
                } else {
                    docLink.style.display = 'none';
                }

                var hf = document.getElementById('<%= hfSelectedCompanionId.ClientID %>');
                if (hf) hf.value = companionId;

                var initials = name ? name.split(' ').map(function(n) { return n[0]; }).join('').substring(0, 2).toUpperCase() : '?';
                document.getElementById('vModalAvatar').textContent = initials;

                document.getElementById('verificationModal').classList.add('show');
            }

            function closeVerificationModal() {
                document.getElementById('verificationModal').classList.remove('show');
            }

            window.addEventListener("load", function () {
                var elCustomers = document.getElementById("statCustomers");
                var elCompanions = document.getElementById("statCompanions");
                var elPending = document.getElementById("statPending");
                var elBookings = document.getElementById("statBookings");

                var customers = elCustomers ? (parseInt(elCustomers.innerText) || 0) : 0;
                var companions = elCompanions ? (parseInt(elCompanions.innerText) || 0) : 0;
                var pending = elPending ? (parseInt(elPending.innerText) || 0) : 0;
                var bookings = elBookings ? (parseInt(elBookings.innerText) || 0) : 0;

                var ChartClass = window['Chart'];
                if (typeof ChartClass !== 'undefined') {

                    // 1. Bar Chart Overview
                    var elBar = document.getElementById('barChartStats');
                    if (elBar) {
                        new ChartClass(elBar.getContext('2d'), {
                            type: 'bar',
                            data: {
                                labels: ['Customers', 'Companions', 'Pending Verif.', 'Total Bookings'],
                                datasets: [{
                                    label: 'Metrics Count',
                                    data: [customers, companions, pending, bookings],
                                    backgroundColor: [
                                        'rgba(14, 165, 233, 0.8)',
                                        'rgba(99, 102, 241, 0.8)',
                                        'rgba(245, 158, 11, 0.8)',
                                        'rgba(16, 185, 129, 0.8)'
                                    ],
                                    borderColor: ['#0ea5e9', '#6366f1', '#f59e0b', '#10b981'],
                                    borderWidth: 1,
                                    borderRadius: 6
                                }]
                            },
                            options: {
                                responsive: true,
                                maintainAspectRatio: false,
                                plugins: { legend: { display: false } },
                                scales: {
                                    y: { beginAtZero: true, grid: { color: 'rgba(0, 0, 0, 0.04)' }, ticks: { color: '#64748b' } },
                                    x: { grid: { display: false }, ticks: { color: '#64748b' } }
                                }
                            }
                        });
                    }

                    // 2. Doughnut Chart User Distribution
                    var elDoughnut = document.getElementById('doughnutChartStats');
                    if (elDoughnut) {
                        new ChartClass(elDoughnut.getContext('2d'), {
                            type: 'doughnut',
                            data: {
                                labels: ['Customers', 'Companions'],
                                datasets: [{
                                    data: [customers, companions],
                                    backgroundColor: ['#0ea5e9', '#6366f1'],
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
                }
            });
        </script>
        <!-- The reason chosen in the dialog (Scripts/admin-decision.js) travels in this field -->
        <input type="hidden" name="decisionReason" id="decisionReason" value="" />
        <script src="../Scripts/admin-decision.js"></script>
        <script src="../Scripts/nav-counts.js"></script>
    </form>
</body>
</html>