<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="OnlineCompanionBookingManagementSystem.Default" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <meta name="description" content="Find and book verified companions for walking, jogging, mall strolling, public events, sightseeing, and more." />
    <title>Online Companion Booking System</title>
    <link href="~/CSS/site.css" rel="stylesheet" runat="server" />
    <!-- Landing-page-only styles (naka-scope sa body.landing para hindi maapektuhan ang ibang page) -->
    <link href="~/CSS/Landing.css" rel="stylesheet" runat="server" />
</head>
<body class="landing">
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

            <div class="navigation">
                <asp:HyperLink ID="Home" runat="server" NavigateUrl="~/Default.aspx" CssClass="active">Home</asp:HyperLink>
                <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="#about">About</asp:HyperLink>
                <asp:HyperLink ID="Howitworks" runat="server" NavigateUrl="#how-it-works">How It Works</asp:HyperLink>
                <asp:HyperLink ID="CompanionsNav" runat="server" NavigateUrl="#verified-companions">Companions</asp:HyperLink>
            </div>

            <div class="nav-buttons">
                <asp:Button ID="Button1" runat="server" Text="Log in" OnClick="Button1_Click" CssClass="btn-login" />
                <asp:Button ID="Button2" runat="server" Text="Register" OnClick="Button2_Click" CssClass="btn-register" />
            </div>
        </div>

        <!-- ===== HERO SECTION ===== -->
        <div class="hero">
            <div class="hero-inner">
                <h1>Find a companion for your <span class="accent">next activity</span></h1>
                <p class="hero-lead">Browse verified companions for walking, jogging, mall strolling, public events, sightseeing, and more.</p>

                <div class="hero-actions">
                    <asp:Button ID="BrowseCompanionsBtn" runat="server" Text="Browse Companions" CssClass="hero-btn" OnClick="BrowseCompanionsBtn_Click" />
                    <a href="#how-it-works" class="hero-btn-secondary">See how it works</a>
                </div>

                <ul class="trust-row">
                    <li>ID-verified profiles</li>
                    <li>Public activities only</li>
                    <li>Clear, upfront rates</li>
                </ul>
            </div>

            <!-- Image carousel: the current image is in the middle, the previous and next ones peek in from the sides.
                 It moves only when the visitor clicks an arrow, a dot, or a side image (no automatic sliding). -->
            <div class="hero-carousel" id="heroCarousel" role="region" aria-roledescription="carousel" aria-label="Companion activities">
                <div class="carousel-viewport">
                    <img class="carousel-slide" data-pos="active" src="Image/hero.jpg" alt="Companions enjoying a walk together" decoding="async" />
                    <img class="carousel-slide" data-pos="next" src="Image/hero-jogging.jpg" alt="Companions jogging together" loading="lazy" decoding="async" aria-hidden="true" />
                    <img class="carousel-slide" data-pos="prev" src="Image/hero-sightseeing.jpg" alt="Companions sightseeing" loading="lazy" decoding="async" aria-hidden="true" />

                    <button type="button" class="carousel-btn carousel-prev" aria-label="Previous image">
                        <svg viewBox="0 0 24 24" aria-hidden="true"><path d="m15 5-7 7 7 7" /></svg>
                    </button>
                    <button type="button" class="carousel-btn carousel-next" aria-label="Next image">
                        <svg viewBox="0 0 24 24" aria-hidden="true"><path d="m9 5 7 7-7 7" /></svg>
                    </button>
                </div>
                <div class="carousel-dots" role="group" aria-label="Choose an image">
                    <button type="button" class="carousel-dot is-active" aria-label="Show image 1" aria-current="true"></button>
                    <button type="button" class="carousel-dot" aria-label="Show image 2"></button>
                    <button type="button" class="carousel-dot" aria-label="Show image 3"></button>
                </div>
                <p class="carousel-status" aria-live="polite">Image 1 of 3</p>
            </div>
        </div>

        <!-- ===== HOW IT WORKS ===== -->
        <div class="how-it-works" id="how-it-works">
            <div class="section-head">
                <span class="eyebrow">How it works</span>
                <h2>Book in three simple steps</h2>
                <p>From browsing to confirmed schedule, everything takes just a few minutes.</p>
            </div>
            <div class="steps">
                <div class="step">
                    <div class="step-number">1</div>
                    <h3>Browse Companions</h3>
                    <p>Explore verified companions based on your preferred activity and schedule.</p>
                </div>
                <div class="step">
                    <div class="step-number">2</div>
                    <h3>Choose a Package</h3>
                    <p>Select the package that best fits your needs and budget.</p>
                </div>
                <div class="step">
                    <div class="step-number">3</div>
                    <h3>Book Your Schedule</h3>
                    <p>Confirm your date and time, and you're all set to go.</p>
                </div>
            </div>
        </div>

        <!-- ===== VERIFIED COMPANIONS ===== -->
        <div class="verified-companions" id="verified-companions">
            <div class="section-head">
                <span class="eyebrow">Verified companions</span>
                <h2>Meet our top-rated companions</h2>
                <p>Every profile is reviewed by our admin team before it appears here.</p>
            </div>

            <div class="companion-cards">
                <asp:Repeater ID="rptFeaturedCompanions" runat="server">
                    <ItemTemplate>
                        <div class="companion-card">
                            <div class="companion-img-wrap">
                                <%# GetCompanionAvatar(Eval("ProfilePicture"), Eval("FullName")) %>
                                <span class="badge-icon" title="Verified"><svg class="badge-icon-svg" viewBox="0 0 24 24" aria-hidden="true"><path d="m5 12 4 4L19 6" /></svg></span>
                            </div>
                            <h4><%#: Eval("FullName") %></h4>
                            <div class="rating">
                                <span>&#9733; <%# FormatRating(Eval("AvgRating")) %></span>
                                <span class="review-count">(<%#: Eval("TotalReviews") %> reviews)</span>
                            </div>
                            <p class="companion-tag"><%#: Eval("MainTag") %></p>
                            <p class="companion-price">&#8369;<%#: Eval("MinRate", "{0:N0}") %> <small>/ hour</small></p>
                            <asp:HyperLink ID="ViewProfile1" runat="server" NavigateUrl="~/Account/Login.aspx" Text="View Profile" CssClass="view-btn" />
                        </div>
                    </ItemTemplate>
                    <FooterTemplate>
                        <%# rptFeaturedCompanions.Items.Count == 0 ? "<div class='companion-empty'>Verified companions will appear here soon. Please check back shortly.</div>" : "" %>
                    </FooterTemplate>
                </asp:Repeater>
            </div>

            <div class="companions-cta">
                <a href="Account/Login.aspx" class="hero-btn-secondary">Log in to browse all companions</a>
            </div>
        </div>

        <!-- ===== WHY USE OCBMS ===== -->
        <div class="why-us" id="about">
            <div class="section-head">
                <span class="eyebrow">Why OCBMS</span>
                <h2>Safe, simple, and organized companion booking</h2>
                <p>Everything you need to find, book, and manage a companion in one place.</p>
            </div>
            <div class="why-us-grid">
                <div class="why-card">
                    <div class="why-tag-indicator">01</div>
                    <h3>Verified Companions</h3>
                    <p>Every companion submits a valid ID and is reviewed and approved by our admin team before they can accept bookings.</p>
                </div>
                <div class="why-card">
                    <div class="why-tag-indicator">02</div>
                    <h3>Simple Booking</h3>
                    <p>Browse companion profiles, choose a package and activities, then pick your preferred date and time in just a few steps.</p>
                </div>
                <div class="why-card">
                    <div class="why-tag-indicator">03</div>
                    <h3>Clear Package Rates</h3>
                    <p>Each companion lists their packages with the duration and rate upfront, so you can see the price before you send a request.</p>
                </div>
                <div class="why-card">
                    <div class="why-tag-indicator">04</div>
                    <h3>Booking Tracking</h3>
                    <p>Follow every booking from pending to completed, with status updates and notifications, all from your dashboard.</p>
                </div>
            </div>
        </div>

        <!-- ===== CALL TO ACTION ===== -->
        <div class="cta-banner">
            <div class="cta-inner">
                <h2>Ready to find your companion?</h2>
                <p>Create a free account in minutes, or join our community of verified companions.</p>
                <div class="cta-actions">
                    <a href="Account/RegisterCustomer.aspx" class="cta-btn-primary">Register as Customer</a>
                    <a href="Account/RegisterCompanion.aspx" class="cta-btn-ghost">Become a Companion</a>
                </div>
            </div>
        </div>
        <div class="cta-spacer"></div>

        <!-- ===== FOOTER ===== -->
        <div class="footer">
            <div class="footer-content">
                <div class="footer-col">
                    <div class="footer-brand">
                        <span class="logo-badge">OC</span>
                        <span>OCBMS</span>
                    </div>
                    <p>Online Companion Booking Management System &mdash; find a verified companion for your next activity securely.</p>
                </div>

                <div class="footer-col">
                    <h4>Quick Links</h4>
                    <a href="#about">About</a>
                    <a href="#how-it-works">How It Works</a>
                    <a href="#verified-companions">Companions</a>
                </div>

                <div class="footer-col">
                    <h4>Get Started</h4>
                    <a href="Account/Login.aspx">Log In</a>
                    <a href="Account/RegisterCustomer.aspx">Register as Customer</a>
                    <a href="Account/RegisterCompanion.aspx">Become a Companion</a>
                </div>

                <div class="footer-col">
                    <h4>Support</h4>
                    <a href="mailto:support@ocbs.com">Contact Us</a>
                    <a href="#">Privacy Policy</a>
                    <a href="#">Terms of Service</a>
                </div>
            </div>

            <div class="footer-bottom">
                <p>&copy; 2026 Online Companion Booking System. All rights reserved.</p>
            </div>
        </div>

        <!-- ===== TOAST NOTIFICATION ===== -->
        <div id="toast" class="toast"></div>
        <script src="Scripts/hero-carousel.js"></script>
        <script src="Scripts/site.js"></script>

    </form>
</body>
</html>
