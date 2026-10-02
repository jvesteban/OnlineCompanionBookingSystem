// nav-counts.js: shows the red "new" counters next to some sidebar items (like Facebook's notification badges).
//
// How it works:
//   1. Any element with data-nav-count="key" in the page is a badge (e.g. <span data-nav-count="notifications">).
//   2. This script asks Handlers/NavCounts.ashx for the number of NEW items and fills the badges.
//   3. It asks again every 20 seconds (and when the tab becomes visible again), so a new booking request or
//      notification shows up by itself without refreshing the page.
// A counter goes away once the user opens that page (the page tells the server it has been seen).
(function () {
    'use strict';

    var badges = document.querySelectorAll('[data-nav-count]');
    if (!badges.length) return;

    // Find the site's root from this script's own URL, so it works in any folder or virtual directory.
    var script = document.currentScript;
    if (!script) return;
    var root = script.src.replace(/Scripts\/nav-counts\.js.*$/i, '');
    var endpoint = root + 'Handlers/NavCounts.ashx';
    var REFRESH_MS = 20000;

    // Shows one badge. A number of 0 hides it. Counts above 99 are shown as "99+".
    function render(badge, info, animate) {
        var n = info ? info.n : 0;
        var previous = parseInt(badge.getAttribute('data-n') || '0', 10);

        // The page the user is already on needs no counter (opening it is what clears the counter).
        if (badge.closest('.sidebar-link.active, .admin-sidebar-link.active')) n = 0;

        if (n > 0) {
            badge.textContent = n > 99 ? '99+' : String(n);
            badge.setAttribute('aria-label', n + ' new');
            badge.hidden = false;

            // Little "pop" when the number goes up (a new booking, a new notification...)
            if (animate && n > previous) {
                badge.classList.remove('pop');
                void badge.offsetWidth; // restart the animation
                badge.classList.add('pop');
            }
        } else {
            badge.textContent = '';
            badge.hidden = true;
        }
        badge.setAttribute('data-n', String(n));
    }

    function apply(data, animate) {
        for (var i = 0; i < badges.length; i++) {
            render(badges[i], data[badges[i].getAttribute('data-nav-count')], animate);
        }
    }

    function refresh() {
        if (document.hidden) return; // don't ask the server while the tab is in the background
        fetch(endpoint, { credentials: 'same-origin', cache: 'no-store' })
            .then(function (response) { return response.ok ? response.json() : null; })
            .then(function (data) {
                if (!data) return; // logged out or a temporary server error: keep what is shown
                apply(data, true);
            })
            .catch(function () { /* offline: try again on the next tick */ });
    }

    // (The numbers are deliberately not cached in the browser: a cached value could briefly show the
    // previous user's counts if someone else logs in on the same tab.)
    refresh();
    setInterval(refresh, REFRESH_MS);
    document.addEventListener('visibilitychange', refresh);

    // Pages can call this after an action to update the numbers right away.
    window.refreshNavCounts = refresh;
})();
