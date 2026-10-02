// register-companion.js: browser-side helpers for Account/RegisterCompanion.aspx
//   1. Show / Hide button for the password fields
//   2. "Standard Rate per Hour": digits only, at most 5 digits (the allowed range is checked by the page's RangeValidator)
//   3. Valid ID upload: JPG/PNG images only, up to 5 MB (used by the CustomValidator "cvDocFile")
// These checks make the form friendlier; the server repeats every one of them, so they cannot be bypassed.
(function () {
    'use strict';

    // ---- 1. Show / Hide password ----
    // A button with data-target="<input id>" switches that input between hidden (password) and visible (text).
    var toggles = document.querySelectorAll('.pw-toggle');
    for (var i = 0; i < toggles.length; i++) {
        toggles[i].addEventListener('click', function () {
            var button = this;
            var input = document.getElementById(button.getAttribute('data-target'));
            if (!input) return;

            var reveal = input.type === 'password';
            input.type = reveal ? 'text' : 'password';
            button.textContent = reveal ? 'Hide' : 'Show';
            button.setAttribute('aria-pressed', reveal ? 'true' : 'false');
            button.setAttribute('aria-label', reveal ? 'Hide password' : 'Show password');
            input.focus();
        });
    }

    // ---- 2. Rate per hour: digits only, max 5 digits ----
    var rate = document.getElementById('txtRate');
    if (rate) {
        var MAX_DIGITS = 5; // the highest allowed rate (10000) has 5 digits

        // A number box normally also accepts "e", "+", "-", "." and ","; none of them make sense for a whole-peso rate.
        rate.addEventListener('keydown', function (event) {
            if (['e', 'E', '+', '-', '.', ','].indexOf(event.key) !== -1) event.preventDefault();
        });

        // Typing, pasting, or dropping: strip anything that is not a digit and cut it to the maximum length.
        rate.addEventListener('input', function () {
            var cleaned = rate.value.replace(/\D/g, '').slice(0, MAX_DIGITS);
            if (rate.value !== cleaned) rate.value = cleaned;
        });

        // Scrolling the mouse wheel over a focused number box would silently change the amount.
        rate.addEventListener('wheel', function () { rate.blur(); });
    }

    // ---- "About You": live character counter (the box itself is limited to 500 characters) ----
    var bio = document.getElementById('txtBio');
    var bioCount = document.getElementById('bioCount');
    if (bio && bioCount) {
        var updateBioCount = function () { bioCount.textContent = bio.value.length; };
        bio.addEventListener('input', updateBioCount);
        updateBioCount();
    }

    // ---- 3. Valid ID file: JPG/PNG only, up to 5 MB ----
    var MAX_ID_BYTES = 5 * 1024 * 1024;
    var idInput = document.getElementById('fuVerificationDoc');

    // Called by ASP.NET's CustomValidator (ClientValidationFunction="validateIdFile").
    // An empty file box is handled by the "required" validator, so it is treated as valid here.
    window.validateIdFile = function (source, args) {
        var file = idInput && idInput.files && idInput.files[0];
        if (!file) { args.IsValid = true; return; }

        var message = '';
        if (!/\.(jpe?g|png)$/i.test(file.name)) {
            message = 'Invalid file format. Please upload a JPG or PNG image.';
        } else if (file.size > MAX_ID_BYTES) {
            message = 'The ID image is too large. Please choose an image under 5 MB.';
        }

        args.IsValid = message === '';
        if (message) {
            // The text on screen is the validator's own content, so update it as well as errormessage
            source.errormessage = message;
            source.innerHTML = message;
        }
    };

    // Check the file as soon as it is chosen, instead of waiting for the Register button.
    if (idInput) {
        idInput.addEventListener('change', function () {
            if (typeof window.ValidatorValidate !== 'function') return;
            var validators = ['rfvDoc', 'cvDocFile'];
            for (var j = 0; j < validators.length; j++) {
                var validator = document.getElementById(validators[j]);
                if (validator) window.ValidatorValidate(validator);
            }
        });
    }
})();
