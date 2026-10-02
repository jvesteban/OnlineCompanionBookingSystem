<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminVerification.aspx.cs" Inherits="OnlineCompanionBookingSystem.Admin.Verification" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Companion Verification - Admin Panel</title>
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/AdminDashboard.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/AdminVerification.css" rel="stylesheet" runat="server" />
    <!-- SweetAlert2 CDN -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
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
                    <asp:HyperLink ID="NavVerification" runat="server" NavigateUrl="~/Admin/AdminVerification.aspx" CssClass="admin-sidebar-link active">
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
                    <h2>Companion Verification</h2>
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

                <!-- ===== FILTER & SEARCH BAR ===== -->
                <div class="v-filter-container">
                    <div class="filter-tabs">
                        <asp:LinkButton ID="tabAll" runat="server" CssClass="filter-tab active" OnClick="tabAll_Click">All Requests</asp:LinkButton>
                        <asp:LinkButton ID="tabPending" runat="server" CssClass="filter-tab" OnClick="tabPending_Click">Pending</asp:LinkButton>
                        <asp:LinkButton ID="tabVerified" runat="server" CssClass="filter-tab" OnClick="tabVerified_Click">Verified</asp:LinkButton>
                        <asp:LinkButton ID="tabRejected" runat="server" CssClass="filter-tab" OnClick="tabRejected_Click">Rejected</asp:LinkButton>
                    </div>

                    <div class="search-box">
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="form-input" placeholder="Search name or email..."></asp:TextBox>
                        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="admin-small-btn btn-view" OnClick="btnSearch_Click" />
                    </div>
                </div>

                <!-- ===== VERIFICATION CARDS CONTAINER ===== -->
                <div class="verification-list">
                    <asp:Repeater ID="rptVerifications" runat="server" OnItemCommand="rptVerifications_ItemCommand">
                        <ItemTemplate>
                            <div class="verification-card">
                                <div class="v-card-left">
                                    <div class="v-avatar"><%# GetAvatarHtml(Eval("FullName"), Eval("ProfilePicture")) %></div>
                                    <div class="v-info">
                                        <h4><%#: Eval("FullName") %></h4>
                                        <p class="v-email"><%#: Eval("Email") %></p>
                                        <p class="v-contact">Contact: <%#: Eval("ContactNo") %></p>
                                        <p class="v-activities">Activities: <%#: Eval("Activities") %></p>
                                    </div>
                                </div>
                                <div class="v-card-middle">
                                    <p class="v-label"><%#: Eval("StatusLabel") %></p>
                                    <p class="v-value"><%#: Eval("DateFormatted") %></p>
                                    <p class="v-fee v-fee-<%#: Eval("PaymentClass") %>"><%#: Eval("PaymentInfo") %></p>
                                    <asp:HyperLink ID="hlDoc" runat="server" NavigateUrl='<%# Eval("DocumentPath") %>' CssClass="v-doc-link" Target="_blank">
                                        View Uploaded Document
                                    </asp:HyperLink>
                                </div>
                                <div class="v-card-right">
                                    <span class='<%# "pill " + Eval("StatusClass") %>'><%#: Eval("VerificationStatus") %></span>
                                    <div class="v-actions">
                                        <asp:Button ID="btnApprove" runat="server" Text="Approve" CssClass="admin-small-btn btn-approve" CommandName="Approve" CommandArgument='<%# Eval("CompanionID") %>' Visible='<%# Eval("VerificationStatus").ToString() != "Verified" %>' />
                                        <asp:Button ID="btnReject" runat="server" Text="Reject" CssClass="admin-small-btn btn-reject" CommandName="Reject" CommandArgument='<%# Eval("CompanionID") %>' Visible='<%# Eval("VerificationStatus").ToString() == "Pending" %>' />
                                        <asp:Button ID="btnRevoke" runat="server" Text="Revoke" CssClass="admin-small-btn btn-reject" CommandName="Revoke" CommandArgument='<%# Eval("CompanionID") %>' Visible='<%# Eval("VerificationStatus").ToString() == "Verified" %>' />
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    
                    <asp:Label ID="lblEmpty" runat="server" Text="No verification records found." Visible="false" CssClass="empty-state-text" />
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

        <div id="toast" class="toast"></div>
        <script src="../Scripts/site.js"></script>

        <script src="../Scripts/nav-counts.js"></script>
    </form>
</body>
</html>