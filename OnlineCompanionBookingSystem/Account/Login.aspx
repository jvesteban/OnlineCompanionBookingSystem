<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="OnlineCompanionBookingSystem.Account.Login" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Login - Online Companion Booking System</title>
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/Login.css" rel="stylesheet" runat="server" />
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

        <!-- ===== SPLIT LOGIN SECTION (TWO COLUMNS) ===== -->
        <div class="login-split-wrapper">
            
            <!-- LEFT PANEL: Professional Branding & Information Text -->
            <div class="login-left-panel">
                <div class="brand-showcase">
                    <span class="showcase-badge">Secure Portal</span>
                    <h1>Manage Your Schedule Securely</h1>
                    <p>
                        Connect safely with verified companion packages, track your active bookings, 
                        and coordinate appointments through our streamlined corporate platform.
                    </p>
                    
                    <div class="showcase-features">
                        <div class="feature-item">
                            <span class="bullet-dot"></span>
                            <span>Strictly Verified Profiles &amp; ID Checks</span>
                        </div>
                        <div class="feature-item">
                            <span class="bullet-dot"></span>
                            <span>Transparent Package Rates &amp; Bookings</span>
                        </div>
                        <div class="feature-item">
                            <span class="bullet-dot"></span>
                            <span>Reliable &amp; Organized User Dashboard</span>
                        </div>
                    </div>
                </div>
            </div>

            <!-- RIGHT PANEL: Login Card Form -->
            <div class="login-right-panel">
                <div class="auth-card">
                    <h2>Welcome Back</h2>
                    <p class="auth-subtext">Please enter your account details to log in.</p>

                    <div class="form-group">
                        <label for="txtEmail">Email Address or Username</label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-input" placeholder="Enter your email or username"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                            ErrorMessage="Email or username is required." CssClass="error-text" Display="Dynamic" />
                    </div>

                    <div class="form-group">
                        <label for="txtPassword">Password</label>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-input" TextMode="Password" placeholder="Enter your password"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword"
                            ErrorMessage="Password is required." CssClass="error-text" Display="Dynamic" />
                    </div>

                    <asp:Label ID="lblMessage" runat="server" CssClass="auth-message" />

                    <asp:Button ID="btnLogin" runat="server" Text="Log In" CssClass="auth-btn" OnClick="btnLogin_Click" />

                    <p class="auth-footer-text">
                        Don't have an account? 
                        <asp:HyperLink ID="hlRegister" runat="server" NavigateUrl="~/Account/RegisterCustomer.aspx">Register here</asp:HyperLink>
                    </p>
                </div>
            </div>

        </div>

        <script src="../Scripts/site.js"></script>
    </form>
</body>
</html>