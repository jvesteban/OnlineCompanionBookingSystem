// booking-status.js: switches a booking's status badge by itself when its time arrives, with no page refresh.
//
// The server already shows the right status when the page loads (see Helpers/BookingStatus.cs). If the page stays open,
// a "Confirmed" booking must become "On-going" at its start time, and later "Awaiting Completion". Each badge carries
// the times it needs: data-booking-start, data-booking-end, and data-now (the server's time when the page was built).
// The server's clock is used (not the visitor's), so a wrong clock on the visitor's computer changes nothing.
(function () {
    'use strict';

    var badges = [];
    var all = document.querySelectorAll('.status-badge[data-booking-start]');
    for (var i = 0; i < all.length; i++) {
        if (all[i].getAttribute('data-booking-start')) badges.push(all[i]);   // only Confirmed bookings have times
    }
    if (!badges.length) return;

    var loadedAt = Date.now();
    var CLASSES = ['status-confirmed', 'status-ongoing', 'status-awaiting'];
    var LABELS = { confirmed: 'Confirmed', ongoing: 'On-going', awaiting: 'Awaiting Completion' };

    // "2026-10-03T15:00:00" has no time zone, so it is read the same way everywhere and can be compared directly.
    function ms(text) { return new Date(text).getTime(); }

    function stateOf(badge, now) {
        if (now < ms(badge.getAttribute('data-booking-start'))) return 'confirmed';
        if (now < ms(badge.getAttribute('data-booking-end'))) return 'ongoing';
        return 'awaiting';
    }

    function refresh() {
        for (var j = 0; j < badges.length; j++) {
            var badge = badges[j];
            var now = ms(badge.getAttribute('data-now')) + (Date.now() - loadedAt);   // the server's time, moving forward
            var state = stateOf(badge, now);
            if (badge.getAttribute('data-state') === state) continue;
            badge.setAttribute('data-state', state);

            for (var k = 0; k < CLASSES.length; k++) badge.classList.remove(CLASSES[k]);
            badge.classList.add('status-' + state);
            badge.textContent = LABELS[state];

            // The small line under the badge ("Starts today at 3:00 PM" -> "Until 5:00 PM" -> "Ended 5:00 PM ...")
            var note = document.getElementById(badge.getAttribute('data-note-id') || '');
            if (note) {
                if (state === 'ongoing') note.textContent = badge.getAttribute('data-ongoing-note') || '';
                else if (state === 'awaiting') note.textContent = badge.getAttribute('data-awaiting-note') || '';
            }
        }
    }

    // The page is already correct when it loads, so the first check only records the starting state
    for (var m = 0; m < badges.length; m++) {
        var firstNow = ms(badges[m].getAttribute('data-now'));
        badges[m].setAttribute('data-state', stateOf(badges[m], firstNow));
    }

    setInterval(refresh, 15000);
    document.addEventListener('visibilitychange', function () { if (!document.hidden) refresh(); });
})();
