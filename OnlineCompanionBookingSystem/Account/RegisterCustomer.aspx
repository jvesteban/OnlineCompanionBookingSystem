<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RegisterCustomer.aspx.cs" Inherits="OnlineCompanionBookingSystem.Account.Register" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Register Customer - Online Companion Booking System</title>
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/RegisterCustomer.css" rel="stylesheet" runat="server" />
    <!-- SweetAlert2 CDN -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</head>
<body>
    <form id="form1" runat="server">
        <!-- ===== NAVBAR ===== -->
        <div class="navbar">
            <div class="logo">
                <asp:HyperLink ID="LogoLink" runat="server" NavigateUrl="~/Default.aspx" CssClass="brand-logo">
                    <span class="logo-badge">OC</span>
                    <div class="logo-text-group">
                        <span class="brand-name">OCBMS</span>
                        <span class="brand-sub">Companion Booking</span>
                    </div>
                </asp:HyperLink>
            </div>
            <div class="nav-right">
                <div class="auth-nav">
                    <asp:HyperLink ID="Home" runat="server" NavigateUrl="~/Default.aspx" CssClass="back-home-link">
                        <span class="back-arrow">&larr;</span> Back to Home
                    </asp:HyperLink>
                </div>
            </div>
        </div>

        <!-- ===== SPLIT REGISTER SECTION (TWO COLUMNS) ===== -->
        <div class="register-split-wrapper">
            
            <!-- LEFT PANEL: Professional Branding & Registration Benefits -->
            <div class="register-left-panel">
                <div class="brand-showcase">
                    <span class="showcase-badge">Customer Portal</span>
                    <h1>Join Our Trusted Community</h1>
                    <p>
                        Create your customer account today to securely browse verified companions, 
                        select customized activity packages, and manage your schedules seamlessly.
                    </p>
                    
                    <div class="showcase-features">
                        <div class="feature-item">
                            <span class="bullet-dot"></span>
                            <span>Access to 100% ID-Verified Companions</span>
                        </div>
                        <div class="feature-item">
                            <span class="bullet-dot"></span>
                            <span>Transparent &amp; Secure Booking Process</span>
                        </div>
                        <div class="feature-item">
                            <span class="bullet-dot"></span>
                            <span>Dedicated Customer Support &amp; History</span>
                        </div>
                    </div>
                </div>
            </div>

            <!-- RIGHT PANEL: Registration Form Card -->
            <div class="register-right-panel">
                <div class="auth-card auth-card-wide">
                    <h2>Create Customer Account</h2>
                    <p class="auth-subtext">Sign up to start booking verified companions.</p>

                    <!-- ROW 1: First Name, Middle Name, Last Name -->
                    <div class="form-grid grid-3">
                        <div class="form-group">
                            <label for="txtFirstName">First Name</label>
                            <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-input" MaxLength="50" placeholder="e.g. Juan"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvFirstName" runat="server" ControlToValidate="txtFirstName"
                                ErrorMessage="First name is required." CssClass="error-text" Display="Dynamic" />
                            <asp:RegularExpressionValidator ID="revFirstName" runat="server" ControlToValidate="txtFirstName"
                                ErrorMessage="First name contains invalid characters." CssClass="error-text" Display="Dynamic"
                                ValidationExpression="^[^&lt;&gt;]*$" />
                        </div>

                        <div class="form-group">
                            <label for="txtMiddleName">Middle Name <span class="optional-label">(Optional)</span></label>
                            <asp:TextBox ID="txtMiddleName" runat="server" CssClass="form-input" MaxLength="50" placeholder="e.g. Santos"></asp:TextBox>
                            <asp:RegularExpressionValidator ID="revMiddleName" runat="server" ControlToValidate="txtMiddleName"
                                ErrorMessage="Middle name contains invalid characters." CssClass="error-text" Display="Dynamic"
                                ValidationExpression="^[^&lt;&gt;]*$" />
                        </div>

                        <div class="form-group">
                            <label for="txtLastName">Last Name</label>
                            <asp:TextBox ID="txtLastName" runat="server" CssClass="form-input" MaxLength="50" placeholder="e.g. Dela Cruz"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvLastName" runat="server" ControlToValidate="txtLastName"
                                ErrorMessage="Last name is required." CssClass="error-text" Display="Dynamic" />
                            <asp:RegularExpressionValidator ID="revLastName" runat="server" ControlToValidate="txtLastName"
                                ErrorMessage="Last name contains invalid characters." CssClass="error-text" Display="Dynamic"
                                ValidationExpression="^[^&lt;&gt;]*$" />
                        </div>
                    </div>

                    <!-- ROW 2: Email & Contact Number -->
                    <div class="form-grid grid-2">
                        <div class="form-group">
                            <label for="txtEmail">Email Address</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-input" TextMode="Email" placeholder="name@example.com"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                                ErrorMessage="Email is required." CssClass="error-text" Display="Dynamic" />
                            <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                                ErrorMessage="Valid email required." CssClass="error-text" Display="Dynamic"
                                ValidationExpression="^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$" />
                        </div>

                        <div class="form-group">
                            <label for="txtContact">Contact Number</label>
                            <asp:TextBox ID="txtContact" runat="server" CssClass="form-input" placeholder="09171234567" MaxLength="11"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvContact" runat="server" ControlToValidate="txtContact"
                                ErrorMessage="Contact number is required." CssClass="error-text" Display="Dynamic" />
                            <asp:RegularExpressionValidator ID="revContact" runat="server" ControlToValidate="txtContact"
                                ErrorMessage="Must be 11-digit PH mobile (09XXXXXXXXX)." CssClass="error-text" Display="Dynamic"
                                ValidationExpression="^09\d{9}$" />
                        </div>
                    </div>

                    <!-- ROW 3: Password & Confirm Password -->
                    <div class="form-grid grid-2">
                        <div class="form-group">
                            <label for="txtPassword">Password</label>
                            <asp:TextBox ID="txtPassword" runat="server" CssClass="form-input" TextMode="Password" placeholder="Min. 8 chars with letters & numbers"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword"
                                ErrorMessage="Password is required." CssClass="error-text" Display="Dynamic" />
                            <asp:RegularExpressionValidator ID="revPassword" runat="server" ControlToValidate="txtPassword"
                                ErrorMessage="At least 8 chars (letters + numbers)." CssClass="error-text" Display="Dynamic"
                                ValidationExpression="^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*#?&]{8,}$" />
                        </div>

                        <div class="form-group">
                            <label for="txtConfirmPassword">Confirm Password</label>
                            <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-input" TextMode="Password" placeholder="Re-enter password"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword"
                                ErrorMessage="Please confirm password." CssClass="error-text" Display="Dynamic" />
                            <asp:CompareValidator ID="cvPassword" runat="server" ControlToValidate="txtConfirmPassword"
                                ControlToCompare="txtPassword" ErrorMessage="Passwords do not match." CssClass="error-text" Display="Dynamic" />
                        </div>
                    </div>

                    <!-- ===== REGISTRATION FEE NOTICE ===== -->
                    <div class="fee-notice">
                        <div class="fee-notice-header">
                            <h4>Registration Fee Notice</h4>
                        </div>
                        <div class="fee-body">
                            <span class="fee-amount">&#8369;50.00</span>
                            <p class="fee-description">
                                A one-time registration fee applies to all new customer accounts to maintain platform quality and verified services. You will complete the payment securely on the next step.
                            </p>
                        </div>
                    </div>

                    <div class="form-group checkbox-group">
                        <asp:CheckBox ID="chkAgreeFee" runat="server" />
                        <label for="chkAgreeFee">I understand and agree to pay the &#8369;50 registration fee.</label>
                        <asp:CustomValidator ID="cvAgreeFee" runat="server" ErrorMessage="You must agree to the fee to continue."
                            CssClass="error-text" Display="Dynamic" OnServerValidate="cvAgreeFee_ServerValidate" />
                    </div>

                    <asp:Label ID="lblMessage" runat="server" CssClass="auth-message" />

                    <asp:Button ID="btnRegister" runat="server" Text="Continue to Payment" CssClass="auth-btn" OnClick="btnRegister_Click" />

                    <div class="auth-footer-links">
                        <p class="auth-footer-text">
                            Already have an account?
                            <asp:HyperLink ID="hlLogin" runat="server" NavigateUrl="~/Account/Login.aspx">Log in here</asp:HyperLink>
                        </p>
                        <p class="auth-footer-text">
                            Are you a companion?
                            <asp:HyperLink ID="hlRegisterCompanion" runat="server" NavigateUrl="~/Account/RegisterCompanion.aspx">Register here</asp:HyperLink>
                        </p>
                    </div>
                </div>
            </div>

        </div>

        <script src="../Scripts/site.js"></script>
    </form>
</body>
</html>