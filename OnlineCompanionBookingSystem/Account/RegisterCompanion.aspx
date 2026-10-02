<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RegisterCompanion.aspx.cs" Inherits="OnlineCompanionBookingSystem.Account.RegisterCompanion" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Companion Registration - Online Companion Booking System</title>
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <link href="~/CSS/RegisterCompanion.css" rel="stylesheet" runat="server" />
    <!-- SweetAlert2 CDN -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</head>
<body>
    <form id="form1" runat="server" enctype="multipart/form-data">
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

        <!-- ===== SPLIT REGISTER COMPANION SECTION ===== -->
        <div class="register-split-wrapper">
            
            <!-- LEFT PANEL: Professional Branding & Companion Benefits -->
            <div class="register-left-panel">
                <div class="brand-showcase">
                    <span class="showcase-badge">Companion Partner</span>
                    <h1>Grow Your Earnings &amp; Offer Services</h1>
                    <p>
                        Join our elite network of verified companions. Set your own hourly rates, 
                        showcase your specializations, and connect securely with clients in a professional platform.
                    </p>
                    
                    <div class="showcase-features">
                        <div class="feature-item">
                            <span class="bullet-dot"></span>
                            <span>Set Your Standard Hourly Rates</span>
                        </div>
                        <div class="feature-item">
                            <span class="bullet-dot"></span>
                            <span>Secure ID Verification by Admin</span>
                        </div>
                        <div class="feature-item">
                            <span class="bullet-dot"></span>
                            <span>Manage Bookings &amp; Schedules Easily</span>
                        </div>
                    </div>
                </div>
            </div>

            <!-- RIGHT PANEL: Registration Form Card -->
            <div class="register-right-panel">
                <div class="auth-card auth-card-wide">
                    <h2>Companion Registration</h2>
                    <p class="auth-subtext">Register to offer companion services and get verified by Admin.</p>

                    <!-- ROW 1: First Name, Middle Name, Last Name -->
                    <div class="form-grid grid-3">
                        <div class="form-group">
                            <label for="txtFirstName">First Name</label>
                            <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-input" MaxLength="50" placeholder="e.g. Maria"></asp:TextBox>
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
                            <asp:TextBox ID="txtLastName" runat="server" CssClass="form-input" MaxLength="50" placeholder="e.g. Clara"></asp:TextBox>
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
                                ErrorMessage="Email address is required." CssClass="error-text" Display="Dynamic" />
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

                    <!-- ROW 2B: Gender. Companion accounts are for female applicants only, so the value is fixed to Female
                         and the applicant confirms it; the admin checks it against the valid ID. -->
                    <div class="form-grid grid-2">
                        <div class="form-group">
                            <label for="txtGenderFixed">Gender</label>
                            <input type="text" id="txtGenderFixed" class="form-input" value="Female" readonly="readonly" aria-readonly="true" />
                            <div class="checkbox-group">
                                <asp:CheckBox ID="chkConfirmFemale" runat="server" />
                                <label for="chkConfirmFemale">I confirm that I am female.</label>
                            </div>
                            <asp:CustomValidator ID="cvConfirmFemale" runat="server" ErrorMessage="Please confirm that you are female to continue."
                                CssClass="error-text" Display="Dynamic" OnServerValidate="cvConfirmFemale_ServerValidate" />
                            <small class="field-hint">Companion accounts are open to female applicants only. An administrator checks this against your valid ID.</small>
                        </div>
                    </div>

                    <!-- ROW 3: Rate per Hour & Valid ID Upload -->
                    <div class="form-grid grid-2">
                        <div class="form-group">
                            <label for="txtRate">Standard Rate per Hour (&#8369;)</label>
                            <!-- Whole pesos only, limited to the range below (keep in sync with RegistrationService.MinHourlyRate / MaxHourlyRate) -->
                            <asp:TextBox ID="txtRate" runat="server" CssClass="form-input" TextMode="Number" min="50" max="10000" step="1" inputmode="numeric" placeholder="e.g. 300"></asp:TextBox>
                            <span class="field-hint">Whole pesos, from &#8369;50 to &#8369;10,000 per hour.</span>
                            <asp:RequiredFieldValidator ID="rfvRate" runat="server" ControlToValidate="txtRate"
                                ErrorMessage="Rate per hour is required." CssClass="error-text" Display="Dynamic" />
                            <asp:RangeValidator ID="rvRate" runat="server" ControlToValidate="txtRate" Type="Integer"
                                MinimumValue="50" MaximumValue="10000"
                                ErrorMessage="Enter a whole number from ₱50 to ₱10,000." CssClass="error-text" Display="Dynamic" />
                        </div>

                        <div class="form-group">
                            <label for="fuVerificationDoc">Valid ID / Government Document</label>
                            <!-- Images only: the file picker shows only JPG/PNG, and the same rule is checked again in the browser and on the server -->
                            <asp:FileUpload ID="fuVerificationDoc" runat="server" CssClass="form-input file-upload-input" accept=".jpg,.jpeg,.png,image/jpeg,image/png" />
                            <span class="field-hint">JPG or PNG image only, up to 5 MB.</span>
                            <asp:RequiredFieldValidator ID="rfvDoc" runat="server" ControlToValidate="fuVerificationDoc"
                                ErrorMessage="Verification document is required." CssClass="error-text" Display="Dynamic" />
                            <asp:CustomValidator ID="cvDocFile" runat="server" ControlToValidate="fuVerificationDoc"
                                ErrorMessage="Please upload a JPG or PNG image." CssClass="error-text" Display="Dynamic"
                                ClientValidationFunction="validateIdFile" OnServerValidate="cvDocFile_ServerValidate" />
                        </div>
                    </div>

                    <!-- About You (optional): shown to customers in the "About Companion" section of the public profile -->
                    <div class="form-group">
                        <label for="txtBio">About You <span class="optional-label">(Optional)</span></label>
                        <asp:TextBox ID="txtBio" runat="server" CssClass="form-input" TextMode="MultiLine" Rows="4" MaxLength="500" ValidateRequestMode="Disabled"
                            placeholder="Tell customers a little about yourself: your interests and what they can expect when they book you."></asp:TextBox>
                        <span class="field-hint">Shown on your public profile. You can edit it later. <span id="bioCount">0</span> / 500 characters.</span>
                    </div>

                    <!-- ROW 4: Checkbox Activities / Specializations -->
                    <div class="form-group">
                        <label>Activities / Specializations <span class="optional-label">(Select at least one)</span></label>
                        <div class="checkbox-list-container">
                            <asp:CheckBoxList ID="cblActivities" runat="server" CssClass="form-checkbox-list" RepeatDirection="Horizontal" RepeatLayout="Flow">
                                <asp:ListItem Text="Walking" Value="Walking" />
                                <asp:ListItem Text="Jogging" Value="Jogging" />
                                <asp:ListItem Text="Mall Strolling" Value="Mall Strolling" />
                                <asp:ListItem Text="Public Events" Value="Public Events" />
                                <asp:ListItem Text="Sightseeing" Value="Sightseeing" />
                            </asp:CheckBoxList>
                        </div>
                        <asp:CustomValidator ID="cvActivities" runat="server" ErrorMessage="Please select at least one activity."
                            CssClass="error-text" Display="Dynamic" OnServerValidate="cvActivities_ServerValidate" />
                    </div>

                    <!-- ROW 5: Password & Confirm Password -->
                    <div class="form-grid grid-2">
                        <div class="form-group">
                            <label for="txtPassword">Password</label>
                            <div class="password-field">
                                <asp:TextBox ID="txtPassword" runat="server" CssClass="form-input" TextMode="Password" placeholder="Min. 8 chars with letters & numbers"></asp:TextBox>
                                <button type="button" class="pw-toggle" data-target="txtPassword" aria-label="Show password" aria-pressed="false">Show</button>
                            </div>
                            <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword"
                                ErrorMessage="Password is required." CssClass="error-text" Display="Dynamic" />
                            <asp:RegularExpressionValidator ID="revPassword" runat="server" ControlToValidate="txtPassword"
                                ErrorMessage="At least 8 chars (letters + numbers)." CssClass="error-text" Display="Dynamic"
                                ValidationExpression="^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*#?&]{8,}$" />
                        </div>

                        <div class="form-group">
                            <label for="txtConfirmPassword">Confirm Password</label>
                            <div class="password-field">
                                <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-input" TextMode="Password" placeholder="Re-enter password"></asp:TextBox>
                                <button type="button" class="pw-toggle" data-target="txtConfirmPassword" aria-label="Show password" aria-pressed="false">Show</button>
                            </div>
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
                                A one-time registration fee applies to all new companion accounts to maintain platform quality and verified services. You will complete the payment securely on the next step.
                            </p>
                        </div>
                    </div>

                    <div class="form-group checkbox-group">
                        <asp:CheckBox ID="chkAgreeFee" runat="server" />
                        <label for="chkAgreeFee">I understand and agree to pay the &#8369;50 registration fee.</label>
                        <asp:CustomValidator ID="cvAgreeFee" runat="server" ErrorMessage="You must agree to the registration fee to continue."
                            CssClass="error-text" Display="Dynamic" OnServerValidate="cvAgreeFee_ServerValidate" />
                    </div>

                    <asp:Label ID="lblMessage" runat="server" CssClass="auth-message" />

                    <asp:Button ID="btnRegisterCompanion" runat="server" Text="Continue to Payment" CssClass="auth-btn" OnClick="btnRegisterCompanion_Click" />

                    <div class="auth-footer-links">
                        <p class="auth-footer-text">
                            Already have an account?
                            <asp:HyperLink ID="hlLogin" runat="server" NavigateUrl="~/Account/Login.aspx">Log in here</asp:HyperLink>
                        </p>
                        <p class="auth-footer-text">
                            Registering as a customer instead?
                            <asp:HyperLink ID="hlRegisterCustomer" runat="server" NavigateUrl="~/Account/RegisterCustomer.aspx">Register here</asp:HyperLink>
                        </p>
                    </div>
                </div>
            </div>

        </div>

        <script src="../Scripts/site.js"></script>
        <script src="../Scripts/register-companion.js"></script>
    </form>
</body>
</html>