<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Payment.aspx.cs" Inherits="OnlineCompanionBookingSystem.Account.Payment" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Payment - Online Companion Booking System</title>
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/Payment.css" rel="stylesheet" runat="server" />
</head>
<body class="pay-page">
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
            <div class="pay-secure-label">Secure Checkout</div>
        </div>

        <div class="pay-wrapper">
            <div class="pay-card">

                <!-- Progress -->
                <ol class="pay-steps">
                    <li class="done"><span>1</span> Details</li>
                    <li id="stepPay" runat="server" class="active"><span>2</span> Payment</li>
                    <li id="stepDone" runat="server"><span>3</span> Complete</li>
                </ol>

                <div class="demo-banner">
                    <strong>Demo mode.</strong> This is a simulated payment. No real money will be charged and no card details are collected.
                </div>

                <!-- ORDER SUMMARY (step 1 at 2) -->
                <asp:Panel ID="pnlSummary" runat="server" CssClass="pay-summary">
                    <h3>Order Summary</h3>
                    <div class="sum-row"><span>Account type</span><strong><asp:Literal ID="litRole" runat="server" /></strong></div>
                    <div class="sum-row"><span>Name</span><strong><asp:Literal ID="litName" runat="server" /></strong></div>
                    <div class="sum-row"><span>Email</span><strong><asp:Literal ID="litEmail" runat="server" /></strong></div>
                    <div class="sum-row"><span>One-time registration fee</span><strong>&#8369;<asp:Literal ID="litFee" runat="server" /></strong></div>
                    <div class="sum-row total"><span>Total due</span><strong>&#8369;<asp:Literal ID="litTotal" runat="server" /></strong></div>
                </asp:Panel>

                <asp:Label ID="lblError" runat="server" CssClass="pay-error" Visible="false" />

                <!-- STEP: SELECT PAYMENT METHOD -->
                <asp:Panel ID="pnlSelect" runat="server">
                    <h3 class="pay-h">Select payment method</h3>
                    <div class="method-list">
                        <asp:Literal ID="litMethods" runat="server" />
                    </div>
                    <asp:Button ID="btnContinue" runat="server" Text="Continue" CssClass="pay-btn" OnClick="btnContinue_Click" />
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel and go back" CssClass="pay-link" OnClick="btnCancel_Click" CausesValidation="false" />
                </asp:Panel>

                <!-- STEP: AUTHORIZE (mock provider screen) -->
                <asp:Panel ID="pnlAuthorize" runat="server" Visible="false">
                    <div class="auth-box">
                        <div class="auth-box-head">
                            <span class="auth-provider"><asp:Literal ID="litProvider" runat="server" /></span>
                            <span class="auth-sandbox">Sandbox</span>
                        </div>
                        <p class="auth-line">You are paying <strong>Online Companion Booking System</strong></p>
                        <div class="auth-amount">&#8369;<asp:Literal ID="litAuthAmount" runat="server" /></div>
                        <div class="sum-row"><span>Reference No.</span><strong><asp:Literal ID="litRef" runat="server" /></strong></div>
                        <div class="sum-row"><span>Payment method</span><strong><asp:Literal ID="litAuthMethod" runat="server" /></strong></div>
                        <div class="sum-row"><asp:Literal ID="litAuthAccountLabel" runat="server" /><strong><asp:Literal ID="litAuthAccount" runat="server" /></strong></div>
                    </div>
                    <asp:HiddenField ID="hfMethod" runat="server" />
                    <asp:Button ID="btnAuthorize" runat="server" Text="Authorize Payment" CssClass="pay-btn" OnClick="btnAuthorize_Click"
                        OnClientClick="this.value='Processing payment...'; this.classList.add('busy');" UseSubmitBehavior="false" />
                    <asp:Button ID="btnChangeMethod" runat="server" Text="Change payment method" CssClass="pay-link" OnClick="btnChangeMethod_Click" CausesValidation="false" />
                </asp:Panel>

                <!-- STEP: SUCCESS / RECEIPT -->
                <asp:Panel ID="pnlSuccess" runat="server" Visible="false" CssClass="pay-success">
                    <div class="success-mark"></div>
                    <h2>Payment successful</h2>
                    <p class="success-lead"><asp:Literal ID="litSuccessLead" runat="server" /></p>

                    <div class="receipt">
                        <div class="sum-row"><span>Reference No.</span><strong><asp:Literal ID="litRcRef" runat="server" /></strong></div>
                        <div class="sum-row"><span>Payment method</span><strong><asp:Literal ID="litRcMethod" runat="server" /></strong></div>
                        <div class="sum-row"><span>Date</span><strong><asp:Literal ID="litRcDate" runat="server" /></strong></div>
                        <div class="sum-row total"><span>Amount paid</span><strong>&#8369;<asp:Literal ID="litRcAmount" runat="server" /></strong></div>
                    </div>
                    <p class="success-note">A receipt and confirmation email have been sent to <strong><asp:Literal ID="litRcEmail" runat="server" /></strong>.</p>
                    <asp:HyperLink ID="hlLogin" runat="server" NavigateUrl="~/Account/Login.aspx" CssClass="pay-btn pay-btn-link">Go to Login</asp:HyperLink>
                </asp:Panel>

            </div>
        </div>
    </form>
</body>
</html>
