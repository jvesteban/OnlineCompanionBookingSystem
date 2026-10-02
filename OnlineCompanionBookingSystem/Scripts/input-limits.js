// input-limits.js: keeps number-like fields (contact number, rate, duration) to sensible values while the user types.
//
// Add these attributes to a text box and this script does the rest (the server still checks everything again):
//   data-digits-only   : only the digits 0-9 can be typed or pasted (letters, spaces, "+", "-", "e", "." are removed)
//   data-max-digits    : most digits allowed, e.g. 11 for a contact number
//   data-max           : largest number allowed, e.g. 50000 for a rate; a digit that would go over it is not accepted
(function () {
    'use strict';

    // Removes anything that is not a digit, then applies the digit limit and the largest allowed number
    function clean(box) {
        var text = box.value.replace(/\D/g, '');

        var maxDigits = parseInt(box.getAttribute('data-max-digits'), 10);
        if (maxDigits > 0 && text.length > maxDigits) text = text.substring(0, maxDigits);

        var max = parseInt(box.getAttribute('data-max'), 10);
        while (max > 0 && text.length > 0 && parseInt(text, 10) > max) text = text.substring(0, text.length - 1);

        if (text !== box.value) box.value = text;
    }

    // Number boxes: the browser lets "e", "+", "-" and "." through; stop them before they appear
    document.addEventListener('keydown', function (e) {
        var box = e.target;
        if (!box || !box.hasAttribute || !box.hasAttribute('data-digits-only')) return;
        if (e.ctrlKey || e.metaKey || e.altKey || e.key.length > 1) return;   // shortcuts and keys like Backspace or arrows
        if (!/^[0-9]$/.test(e.key)) e.preventDefault();
    });

    // Typing, pasting, or dropping text all end up here
    document.addEventListener('input', function (e) {
        var box = e.target;
        if (box && box.hasAttribute && box.hasAttribute('data-digits-only')) clean(box);
    });

    // Clean what is already in the boxes when the page opens (for example after a failed save)
    var boxes = document.querySelectorAll('[data-digits-only]');
    for (var i = 0; i < boxes.length; i++) clean(boxes[i]);
})();
