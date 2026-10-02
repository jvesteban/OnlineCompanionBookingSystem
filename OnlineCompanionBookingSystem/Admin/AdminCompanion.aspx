<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminCompanion.aspx.cs" Inherits="OnlineCompanionBookingSystem.Admin.AdminCompanions" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Companion Management - Admin Panel</title>
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/AdminDashboard.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/AdminCompanion.css" rel="stylesheet" runat="server" />
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
                    <asp:HyperLink ID="NavCompanions" runat="server" NavigateUrl="~/Admin/AdminCompanion.aspx" CssClass="admin-sidebar-link active">
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
                    <h2>Companion Management</h2>
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
                            <h3><asp:Literal ID="litTotalCompanions" runat="server" Text="0" /></h3>
                            <p>Total Companions</p>
                        </div>
                    </div>
                    <div class="admin-stat-card">
                        <div class="admin-stat-indicator">02</div>
                        <div>
                            <h3><asp:Literal ID="litVerifiedCompanions" runat="server" Text="0" /></h3>
                            <p>Verified Profiles</p>
                        </div>
                    </div>
                    <div class="admin-stat-card warning">
                        <div class="admin-stat-indicator warning-ind">03</div>
                        <div>
                            <h3><asp:Literal ID="litPendingCompanions" runat="server" Text="0" /></h3>
                            <p>Pending Verifications</p>
                        </div>
                    </div>
                </div>

                <!-- ===== CHARTS SECTION ===== -->
                <div class="admin-charts-grid">
                    <div class="admin-chart-card">
                        <div class="admin-chart-header">
                            <h3>Top Companions by Rating</h3>
                        </div>
                        <div class="chart-container" style="position: relative; height: 240px;">
                            <canvas id="chartTopCompanions"></canvas>
                        </div>
                    </div>

                    <div class="admin-chart-card">
                        <div class="admin-chart-header">
                            <h3>Verification Status Breakdown</h3>
                        </div>
                        <div class="chart-container" style="position: relative; height: 240px;">
                            <canvas id="chartVerificationRatio"></canvas>
                        </div>
                    </div>
                </div>

                <!-- ===== SEARCH AND FILTER BAR ===== -->
                <div class="companion-filter-bar">
                    <div class="filter-tabs">
                        <asp:LinkButton ID="tabAll" runat="server" CssClass="filter-tab active" OnClick="tabAll_Click">All Companions</asp:LinkButton>
                        <asp:LinkButton ID="tabVerified" runat="server" CssClass="filter-tab" OnClick="tabVerified_Click">Verified</asp:LinkButton>
                        <asp:LinkButton ID="tabPending" runat="server" CssClass="filter-tab" OnClick="tabPending_Click">Pending</asp:LinkButton>
                        <asp:LinkButton ID="tabRejected" runat="server" CssClass="filter-tab" OnClick="tabRejected_Click">Rejected</asp:LinkButton>
                    </div>

                    <div class="search-box">
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="form-input search-input" placeholder="Search by name or email..."></asp:TextBox>
                        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="admin-small-btn btn-view" OnClick="btnSearch_Click" />
                    </div>
                </div>

                <!-- ===== COMPANIONS TABLE ===== -->
                <div class="admin-section">
                    <table class="admin-table">
                        <thead>
                            <tr>
                                <th>Companion Name</th>
                                <th>Email</th>
                                <th>Contact Number</th>
                                <th>Activities</th>
                                <th>Rating</th>
                                <th>Bookings</th>
                                <th>Verification</th>
                                <th>Account</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptCompanions" runat="server" OnItemCommand="rptCompanions_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td>
                                            <div class="companion-profile-cell">
                                                <%# GetPhotoOrAvatar(Eval("ProfilePicture"), Eval("FullName")) %>
                                                <span class="companion-name"><%#: Eval("FullName") %></span>
                                            </div>
                                        </td>
                                        <td><%#: Eval("Email") %></td>
                                        <td><%#: Eval("ContactNumber") != DBNull.Value && !string.IsNullOrEmpty(Eval("ContactNumber").ToString()) ? Eval("ContactNumber") : "N/A" %></td>
                                        <td><span class="activity-tag-preview"><%#: Eval("Activities") %></span></td>
                                        <td><strong><%# FormatRating(Eval("AvgRating")) %></strong></td>
                                        <td><strong><%#: Eval("TotalBookings") %></strong></td>
                                        <td>
                                            <span class='pill <%# GetStatusPillClass(Eval("VerificationStatus").ToString()) %>'>
                                                <%#: Eval("VerificationStatus") %>
                                            </span>
                                        </td>
                                        <td>
                                            <span class='pill <%# Convert.ToBoolean(Eval("IsActive")) ? "confirmed" : "rejected" %>'>
                                                <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Disabled" %>
                                            </span>
                                        </td>
                                        <td>
                                            <button type="button" class="admin-small-btn btn-view"
                                                onclick="openCompanionModal(
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("FullName")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("Email")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("ContactNumber")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("Activities")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(FormatRating(Eval("AvgRating"))) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("TotalBookings")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("VerificationStatus")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("VerificationDocPath")) %>',
                                                    '<%#: OnlineCompanionBookingSystem.WebSafe.Js(Eval("Bio")) %>'
                                                )">View</button>

                                            <asp:LinkButton ID="btnToggleStatus" runat="server" 
                                                CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "admin-small-btn btn-reject" : "admin-small-btn btn-approve" %>'
                                                CommandName="ToggleStatus" 
                                                CommandArgument='<%# Eval("UserID") + "|" + Eval("IsActive") %>'
                                                Text='<%# Convert.ToBoolean(Eval("IsActive")) ? "Disable" : "Enable" %>' />
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>

                    <asp:Label ID="lblEmpty" runat="server" CssClass="empty-state-text" Text="No companion records found." Visible="false" />
                </div>

            </div>
        </div>

        <!-- ===== COMPANION DETAILS MODAL ===== -->
        <div class="admin-modal-overlay" id="companionDetailModal">
            <div class="admin-modal">
                <div class="admin-modal-header">
                    <div class="modal-title-group">
                        <h3>Companion Details</h3>
                        <p class="modal-subtitle">Full profile review and status</p>
                    </div>
                    <button type="button" class="admin-modal-close" onclick="closeCompanionModal()">&times;</button>
                </div>
                <div class="admin-modal-body">
                    <div class="admin-modal-avatar-section">
                        <div class="admin-avatar-xl" id="cpModalAvatar">C</div>
                        <h4 id="cpModalName">&mdash;</h4>
                        <p id="cpModalEmail" class="admin-modal-email-text">&mdash;</p>
                    </div>
                    <div class="admin-modal-info-grid">
                        <div class="info-item">
                            <span class="info-label">Contact Number</span>
                            <span class="info-value" id="cpModalContact">&mdash;</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">Activities</span>
                            <span class="info-value" id="cpModalActivities">&mdash;</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">Average Rating</span>
                            <span class="info-value text-accent" id="cpModalRating">0.0</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">Completed Bookings</span>
                            <span class="info-value" id="cpModalBookings">0</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">Verification Status</span>
                            <span class="info-value" id="cpModalStatus">&mdash;</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">Verification Document</span>
                            <span class="info-value">
                                <a href="#" id="cpModalDocLink" target="_blank" class="v-doc-link">View Document</a>
                            </span>
                        </div>
                        <div class="info-item bio-item">
                            <span class="info-label">Bio / Description</span>
                            <p class="info-bio-text" id="cpModalBio">No bio provided.</p>
                        </div>
                    </div>
                </div>
                <div class="admin-modal-footer">
                    <button type="button" class="admin-modal-action-btn" onclick="closeCompanionModal()">Close Window</button>
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

        <asp:HiddenField ID="hfTopCompanionNames" runat="server" />
        <asp:HiddenField ID="hfTopCompanionRatings" runat="server" />

        <script src="../Scripts/site.js"></script>
        <script type="text/javascript">
            function openCompanionModal(name, email, contact, activities, rating, bookings, status, docPath, bio) {
                var elName = document.getElementById('cpModalName');
                var elEmail = document.getElementById('cpModalEmail');
                var elContact = document.getElementById('cpModalContact');
                var elActivities = document.getElementById('cpModalActivities');
                var elRating = document.getElementById('cpModalRating');
                var elBookings = document.getElementById('cpModalBookings');
                var elStatus = document.getElementById('cpModalStatus');
                var elDoc = document.getElementById('cpModalDocLink');
                var elBio = document.getElementById('cpModalBio');
                var elAvatar = document.getElementById('cpModalAvatar');
                var modal = document.getElementById('companionDetailModal');

                if (elName) elName.textContent = name || '';
                if (elEmail) elEmail.textContent = email || '';
                if (elContact) elContact.textContent = contact || 'N/A';
                if (elActivities) elActivities.textContent = activities || 'None specified';
                if (elRating) elRating.textContent = rating || '0.0';
                if (elBookings) elBookings.textContent = bookings || '0';
                if (elStatus) elStatus.textContent = status || '';
                if (elBio) elBio.textContent = bio || 'No bio available.';

                if (elDoc) {
                    if (docPath && docPath !== '') {
                        elDoc.href = docPath;
                        elDoc.style.display = 'inline-block';
                    } else {
                        elDoc.style.display = 'none';
                    }
                }

                if (elAvatar) {
                    var strName = String(name || '');
                    var initials = strName ? strName.split(' ').map(function (n) { return n[0]; }).join('').substring(0, 2).toUpperCase() : 'CP';
                    elAvatar.textContent = initials;
                }

                if (modal) modal.classList.add('show');
            }

            function closeCompanionModal() {
                var modal = document.getElementById('companionDetailModal');
                if (modal) modal.classList.remove('show');
            }

            window.addEventListener("load", function () {
                var ChartClass = window['Chart'];
                if (typeof ChartClass === 'undefined') return;

                // 1. Render Top Companions Bar Chart
                var hfNames = document.getElementById('<%= hfTopCompanionNames.ClientID %>');
                var hfRatings = document.getElementById('<%= hfTopCompanionRatings.ClientID %>');

                var names = hfNames && hfNames.value ? hfNames.value.split(',') : ['No Data'];
                var ratings = hfRatings && hfRatings.value ? hfRatings.value.split(',').map(Number) : [0];

                var elBar = document.getElementById('chartTopCompanions');
                if (elBar) {
                    new ChartClass(elBar.getContext('2d'), {
                        type: 'bar',
                        data: {
                            labels: names,
                            datasets: [{
                                label: 'Rating (Max 5.0)',
                                data: ratings,
                                backgroundColor: 'rgba(99, 102, 241, 0.8)',
                                borderColor: '#6366f1',
                                borderWidth: 1,
                                borderRadius: 6
                            }]
                        },
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            plugins: { legend: { display: false } },
                            scales: {
                                y: { beginAtZero: true, max: 5.0, grid: { color: 'rgba(0, 0, 0, 0.04)' }, ticks: { color: '#64748b' } },
                                x: { grid: { display: false }, ticks: { color: '#64748b' } }
                            }
                        }
                    });
                }

                // 2. Render Verification Status Ratio Doughnut Chart
                var verifiedCount = parseInt('<%= litVerifiedCompanions.Text %>') || 0;
                var pendingCount = parseInt('<%= litPendingCompanions.Text %>') || 0;

                var elDoughnut = document.getElementById('chartVerificationRatio');
                if (elDoughnut) {
                    new ChartClass(elDoughnut.getContext('2d'), {
                        type: 'doughnut',
                        data: {
                            labels: ['Verified', 'Pending'],
                            datasets: [{
                                data: [verifiedCount, pendingCount],
                                backgroundColor: ['#10b981', '#f59e0b'],
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