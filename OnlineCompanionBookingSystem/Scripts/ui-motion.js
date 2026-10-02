// ui-motion.js: small animations (for the Companion and Customer dashboards) that CSS alone cannot do.
//   - Count-up: the numbers in the statistic cards (Total Bookings, Average Rating, ...) count up from 0
//     to their value when the Dashboard opens.
// Everything else (page fade-in, hover effects, ...) is in CSS/CompanionDashboard.css.
(function () {
    'use strict';

    // People who asked their device to reduce motion just see the final numbers.
    if (window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;

    var numbers = document.querySelectorAll('.stat-card h3');
    var DURATION = 900;       // milliseconds
    var START_DELAY = 250;    // wait for the card's own fade-in to begin

    Array.prototype.forEach.call(numbers, function (el) {
        var text = el.textContent.trim();
        if (!/^\d+(\.\d+)?$/.test(text)) return;   // only plain numbers (e.g. "12" or "4.5")

        var target = parseFloat(text);
        if (target === 0) return;                  // nothing to count
        var decimals = text.indexOf('.') >= 0 ? text.split('.')[1].length : 0;

        el.textContent = (0).toFixed(decimals);
        var begin = null;

        function step(now) {
            if (begin === null) begin = now;
            var progress = Math.min((now - begin) / DURATION, 1);
            var eased = 1 - Math.pow(1 - progress, 3);   // starts fast, slows down near the end
            el.textContent = (target * eased).toFixed(decimals);
            if (progress < 1) requestAnimationFrame(step);
            else el.textContent = text;                  // always end on the exact original value
        }

        setTimeout(function () { requestAnimationFrame(step); }, START_DELAY);
    });
})();
