// site.js: shared front-end behavior for the whole site (landing page effects, user/admin dropdowns,
// modals, and the toast helper). Each feature is guarded with "if (element exists)", so the same file is
// safe to load on every page even when a page doesn't have that element.

// ===== HERO IMAGE SLIDER (Auto & Seamless Loop) =====
// Note: on the landing page the slider also scrolls through CSS (the scrollSlider animation); this script
// only runs when the track exists, and sets a transform that the CSS animation overrides.
document.addEventListener("DOMContentLoaded", function () {
    normalizeCompanionAccountDropdown();
    const track = document.getElementById('sliderTrack');
    if (track) {
        let currentSlide = 0;
        const totalRealSlides = 3;
        const dots = document.querySelectorAll('.dot');

        track.style.transition = 'transform 0.8s ease-in-out';

        function nextSlide() {
            currentSlide++;
            track.style.transition = 'transform 0.8s ease-in-out';
            track.style.transform = `translateX(-${currentSlide * 100}%)`;

            // Update dots kung mayroon man
            if (dots && dots.length > 0) {
                const activeDotIndex = currentSlide % totalRealSlides;
                dots.forEach((dot, i) => {
                    dot.classList.toggle('active', i === activeDotIndex);
                });

                // ===== CUSTOMER/COMPANION USER DROPDOWN =====
                function toggleUserDropdown(event) {
                    if (event) event.stopPropagation();

                    const card = document.getElementById('userDropdownCard');
                    if (!card) return;

                    const isOpen = card.classList.toggle('show');
                    card.style.display = isOpen ? 'block' : 'none';
                    card.setAttribute('aria-hidden', isOpen ? 'false' : 'true');
                }

                document.addEventListener('click', function (event) {
                    const card = document.getElementById('userDropdownCard');
                    const container = document.getElementById('userMenuContainer');

                    if (card && container && !container.contains(event.target)) {
                        card.classList.remove('show');
                        card.style.display = 'none';
                        card.setAttribute('aria-hidden', 'true');
                    }
                });
            }

            // Kapag umabot na sa duplicate slide sa dulo, i-reset nang tahimik pabalik sa slide 0
            if (currentSlide === totalRealSlides) {
                setTimeout(() => {
                    track.style.transition = 'none';
                    currentSlide = 0;
                    track.style.transform = `translateX(0%)`;
                }, 800);
            }
        }

        setInterval(nextSlide, 4000);
    }
});

// Older Companion pages still contain the original compact account markup. Normalize it
// here so every Companion page uses the same account card as the Dashboard.
function normalizeCompanionAccountDropdown() {
    const menu = document.getElementById('companionAccountMenu');
    const dropdown = document.getElementById('companionAccountDropdown');
    if (!menu || !dropdown || !dropdown.querySelector('.companion-account-summary')) return;

    const toggle = menu.querySelector('.topbar-user');
    const name = toggle ? (toggle.querySelector('.user-name') || {}).textContent : 'Companion';
    const avatar = toggle ? toggle.querySelector('.user-avatar') : null;
    const image = avatar ? avatar.querySelector('img') : null;
    const initials = avatar ? avatar.textContent.trim() : 'C';
    const logoutId = menu.closest('form') ? menu.closest('form').querySelector('[id$="btnLogout"]') : null;

    dropdown.innerHTML = `
        <div class="dropdown-header">
            <div class="dropdown-avatar-lg">
                ${image && image.src && image.style.display !== 'none'
                    ? `<img class="dropdown-avatar-img" src="${image.src}" alt="Companion profile photo">`
                    : `<span>${initials || 'C'}</span>`}
            </div>
            <div class="dropdown-user-info">
                <p class="dropdown-fullname">${escapeDropdownText(name.trim() || 'Companion')}</p>
                <p class="dropdown-email">Companion Account</p>
            </div>
        </div>
        <div class="dropdown-divider"></div>
        <a class="dropdown-item" href="CompanionProfile.aspx"><span><svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="8" r="4" /><path d="M4 21c0-4 3-7 8-7s8 3 8 7" /></svg> View &amp; Edit Profile</span></a>
        <a class="dropdown-item" href="CompanionProfile.aspx"><span><svg class="ui-icon" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="3" /><path d="M19.4 15a1.7 1.7 0 0 0 .3 1.9l.1.1-1.8 1.8-.1-.1a1.7 1.7 0 0 0-1.9-.3 1.7 1.7 0 0 0-1 1.5v.1h-2.6v-.1a1.7 1.7 0 0 0-1-1.5 1.7 1.7 0 0 0-1.9.3l-.1.1-1.8-1.8.1-.1a1.7 1.7 0 0 0 .3-1.9 1.7 1.7 0 0 0-1.5-1H6.4v-2.6h.1a1.7 1.7 0 0 0 1.5-1 1.7 1.7 0 0 0-.3-1.9l-.1-.1 1.8-1.8.1.1a1.7 1.7 0 0 0 1.9.3 1.7 1.7 0 0 0 1-1.5V5h2.6v.1a1.7 1.7 0 0 0 1 1.5 1.7 1.7 0 0 0 1.9-.3l.1-.1 1.8 1.8-.1.1a1.7 1.7 0 0 0-.3 1.9 1.7 1.7 0 0 0 1.5 1h.1v2.6h-.1a1.7 1.7 0 0 0-1.5 1z" /></svg> Account Settings</span></a>
        <div class="dropdown-divider"></div>
        <button type="button" class="dropdown-item dropdown-logout-btn">Log Out Securely</button>
    `;

    const logoutButton = dropdown.querySelector('.dropdown-logout-btn');
    if (logoutButton) {
        logoutButton.addEventListener('click', function () {
            if (logoutId) logoutId.click();
        });
    }
}

function escapeDropdownText(value) {
    return String(value).replace(/[&<>"']/g, function (character) {
        return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[character];
    });
}

// ===== SMOOTH SCROLL FOR ALL ANCHOR LINKS =====
document.querySelectorAll('a[href^="#"]').forEach(link => {
    link.addEventListener('click', function (e) {
        const targetId = this.getAttribute('href');
        if (targetId && targetId.length > 1) {
            const targetEl = document.querySelector(targetId);
            if (targetEl) {
                e.preventDefault();
                targetEl.scrollIntoView({ behavior: 'smooth' });
            }
        }
    });
});

// ===== NAVBAR SHADOW ON SCROLL =====
const navbar = document.querySelector('.navbar');
window.addEventListener('scroll', function () {
    if (navbar) {
        if (window.scrollY > 10) {
            navbar.classList.add('navbar-scrolled');
        } else {
            navbar.classList.remove('navbar-scrolled');
        }
    }
});

// ===== SCROLL-REVEAL ANIMATION FOR SECTIONS =====
// Sections start hidden (reveal-hidden) and fade in (reveal-visible) once 15% of them is on screen.
// The CSS for these classes is in Landing.css. unobserve() makes each section animate only once.
const revealElements = document.querySelectorAll(
    '.how-it-works, .verified-companions, .why-us'
);

if (revealElements.length > 0) {
    const revealObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('reveal-visible');
                revealObserver.unobserve(entry.target);
            }
        });
    }, { threshold: 0.15 });

    revealElements.forEach(el => {
        el.classList.add('reveal-hidden');
        revealObserver.observe(el);
    });
}

// ===== TOAST NOTIFICATION FUNCTION =====
// Shows a small message at the bottom of the page for `duration` ms. Needs a <div id="toast"> in the page.
// Usage: showToast("Saved!"); or showToast("Saved!", 4000);
function showToast(message, duration = 2500) {
    const toast = document.getElementById('toast');
    if (!toast) return;

    toast.textContent = message;
    toast.classList.add('show');

    clearTimeout(toast.hideTimeout);
    toast.hideTimeout = setTimeout(() => {
        toast.classList.remove('show');
    }, duration);
}

// ===== ADMIN PROFILE DROPDOWN =====
const adminUserToggle = document.getElementById('adminUserToggle');
const adminDropdown = document.getElementById('adminDropdown');

if (adminUserToggle && adminDropdown) {
    adminUserToggle.addEventListener('click', function (e) {
        e.stopPropagation();
        adminDropdown.classList.toggle('show');
        adminUserToggle.classList.toggle('open');
    });

    document.addEventListener('click', function (e) {
        if (!adminDropdown.contains(e.target) && !adminUserToggle.contains(e.target)) {
            adminDropdown.classList.remove('show');
            adminUserToggle.classList.remove('open');
        }
    });

}

// ===== COMPANION ACCOUNT DROPDOWN =====
function toggleCompanionAccount(event) {
    if (event) event.stopPropagation();

    const menu = document.getElementById('companionAccountMenu');
    const toggle = menu ? menu.querySelector('.topbar-user') : null;
    const dropdown = document.getElementById('companionAccountDropdown');
    if (!menu || !dropdown) return;

    const open = menu.classList.toggle('open');
    dropdown.classList.toggle('show', open);
    dropdown.setAttribute('aria-hidden', open ? 'false' : 'true');
    if (toggle) toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
}

document.addEventListener('click', function (event) {
    const menu = document.getElementById('companionAccountMenu');
    const dropdown = document.getElementById('companionAccountDropdown');
    const toggle = menu ? menu.querySelector('.topbar-user') : null;
    if (!menu || !dropdown || menu.contains(event.target)) return;

    menu.classList.remove('open');
    dropdown.classList.remove('show');
    dropdown.setAttribute('aria-hidden', 'true');
    if (toggle) toggle.setAttribute('aria-expanded', 'false');
});

document.addEventListener('keydown', function (event) {
    const menu = document.getElementById('companionAccountMenu');
    const toggle = menu ? menu.querySelector('.topbar-user') : null;
    if (!menu || !toggle) return;

    if ((event.key === 'Enter' || event.key === ' ') && document.activeElement === toggle) {
        event.preventDefault();
        toggleCompanionAccount(event);
    } else if (event.key === 'Escape' && menu.classList.contains('open')) {
        menu.classList.remove('open');
        const dropdown = document.getElementById('companionAccountDropdown');
        dropdown.classList.remove('show');
        dropdown.setAttribute('aria-hidden', 'true');
        toggle.setAttribute('aria-expanded', 'false');
        toggle.focus();
    }
});

// ===== ADMIN PROFILE MODAL =====
function openAdminProfileModal() {
    const modal = document.getElementById('adminProfileModal');
    if (modal) modal.classList.add('show');
}

function closeAdminProfileModal() {
    const modal = document.getElementById('adminProfileModal');
    if (modal) modal.classList.remove('show');
}

// ===== VERIFICATION DETAIL MODAL =====
// Fills the popup on the admin dashboard with one companion application's details. The CompanionID is stored in
// the hidden field (hfSelectedCompanionId) so the server knows which application the Approve/Reject button is for.
function openVerificationModal(name, email, contact, activities, date, docPath, companionId) {
    const vModalName = document.getElementById('vModalName');
    const vModalEmail = document.getElementById('vModalEmail');
    const vModalContact = document.getElementById('vModalContact');
    const vModalActivities = document.getElementById('vModalActivities');
    const vModalDate = document.getElementById('vModalDate');
    const vModalAvatar = document.getElementById('vModalAvatar');
    const docLink = document.getElementById('vModalDoc');

    if (vModalName) vModalName.textContent = name;
    if (vModalEmail) vModalEmail.textContent = email;
    if (vModalContact) vModalContact.textContent = contact || 'N/A';
    if (vModalActivities) vModalActivities.textContent = activities || 'None specified';
    if (vModalDate) vModalDate.textContent = date;
    if (vModalAvatar) vModalAvatar.textContent = name ? name.substring(0, 2).toUpperCase() : 'OC';
    if (docLink) docLink.href = docPath || '#';

    const hiddenField = document.querySelector('input[id$="hfSelectedCompanionId"]');
    if (hiddenField) hiddenField.value = companionId;

    const verificationModal = document.getElementById('verificationModal');
    if (verificationModal) verificationModal.classList.add('show');
}

function closeVerificationModal() {
    const verificationModal = document.getElementById('verificationModal');
    if (verificationModal) verificationModal.classList.remove('show');
}

// Pangkalahatang click handler para isara ang mga modals kapag pinindot ang labas nito
document.addEventListener('click', function (e) {
    const adminModal = document.getElementById('adminProfileModal');
    if (adminModal && e.target === adminModal) {
        adminModal.classList.remove('show');
    }

    const verificationModal = document.getElementById('verificationModal');
    if (verificationModal && e.target === verificationModal) {
        verificationModal.classList.remove('show');
    }
});