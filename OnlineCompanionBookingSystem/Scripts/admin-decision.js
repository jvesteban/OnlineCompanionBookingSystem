// admin-decision.js: asks the administrator for a reason before approving, rejecting, or revoking a companion.
//
// A button opts in with data-decision="Approve" | "Reject" | "Revoke" (and optionally data-name="Maria Clara", or
// data-name-from="elementId" to read the name from an element). Clicking it opens a SweetAlert2 dialog with a list of
// reasons and an optional note. The chosen reason is put into the hidden field #decisionReason, then the button's
// normal action carries on. The server reads the reason from that field, checks it again, and puts it in the email
// and the in-app notification that the companion receives. Needs SweetAlert2 on the page.
(function () {
    'use strict';

    var OTHER = 'Other (please specify)';

    var REASONS = {
        Approve: [
            'Valid ID reviewed and details match the registration',
            'All verification requirements were fulfilled',
            OTHER
        ],
        Reject: [
            'The ID image is unclear or unreadable',
            'The ID appears to be invalid, expired, or altered',
            'The name on the ID does not match the registration',
            'The ID does not meet the eligibility requirements (female applicants only)',
            'Registration details are incomplete or incorrect',
            OTHER
        ],
        Revoke: [
            'The verification document was found to be invalid',
            'Complaints were received about the account',
            'A violation of the platform\'s terms of use',
            'The verification needs to be re-checked',
            OTHER
        ]
    };

    var TEXT = {
        Approve: { title: 'Approve this application?', button: 'Approve', color: '#16a34a', hint: 'Shown to the companion in the approval email.' },
        Reject: { title: 'Reject this application?', button: 'Reject', color: '#dc2626', hint: 'Shown to the companion in the rejection email. The registration fee will be refunded.' },
        Revoke: { title: 'Revoke this verification?', button: 'Revoke', color: '#d97706', hint: 'Shown to the companion in the email. The account goes back to Pending.' }
    };

    function escapeHtml(text) {
        return String(text).replace(/[&<>"']/g, function (c) {
            return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
        });
    }

    function nameOf(button) {
        var name = button.getAttribute('data-name');
        var from = button.getAttribute('data-name-from');
        if (!name && from) {
            var el = document.getElementById(from);
            if (el) name = el.textContent;
        }
        name = (name || '').trim();
        return name && name !== '—' ? name : 'this applicant';
    }

    // Carries on with what the button normally does (a postback link, or a submit button)
    function proceed(button) {
        var href = button.getAttribute('href');
        if (button.tagName === 'A' && href && href.indexOf('javascript:') === 0) {
            window.location.href = href;
        } else {
            button.setAttribute('data-confirmed', '1');
            button.click();
        }
    }

    document.addEventListener('click', function (e) {
        var button = e.target.closest ? e.target.closest('[data-decision]') : null;
        if (!button || button.getAttribute('data-confirmed') === '1') return;

        e.preventDefault();
        e.stopImmediatePropagation();

        if (typeof Swal === 'undefined') return;   // without the dialog nothing is sent, so a reason is never skipped

        var action = button.getAttribute('data-decision');
        var text = TEXT[action];
        var options = REASONS[action];
        if (!text || !options) return;

        var optionHtml = options.map(function (o) { return '<option>' + escapeHtml(o) + '</option>'; }).join('');

        Swal.fire({
            title: text.title,
            html:
                '<div style="text-align:left;font-size:14px">' +
                '<p style="margin:0 0 12px;color:#64748b">' + escapeHtml(nameOf(button)) + '</p>' +
                '<label for="swalReason" style="font-weight:600">Reason</label>' +
                '<select id="swalReason" class="swal2-select" style="width:100%;margin:6px 0 14px;display:flex">' + optionHtml + '</select>' +
                '<label for="swalNote" style="font-weight:600">Additional note <span style="font-weight:400;color:#94a3b8">(optional, required for "Other")</span></label>' +
                '<textarea id="swalNote" class="swal2-textarea" maxlength="200" rows="3" style="width:100%;margin:6px 0 4px" placeholder="Add details the companion should know"></textarea>' +
                '<p style="margin:6px 0 0;font-size:12px;color:#94a3b8">' + escapeHtml(text.hint) + '</p>' +
                '</div>',
            showCancelButton: true,
            confirmButtonText: text.button,
            confirmButtonColor: text.color,
            focusConfirm: false,
            preConfirm: function () {
                var reason = document.getElementById('swalReason').value;
                var note = document.getElementById('swalNote').value.trim();
                if (reason === OTHER) {
                    if (!note) {
                        Swal.showValidationMessage('Please describe the reason in the note.');
                        return false;
                    }
                    return note;
                }
                return note ? reason + '. ' + note : reason;
            }
        }).then(function (result) {
            if (!result.isConfirmed) return;
            var field = document.getElementById('decisionReason');
            if (field) field.value = result.value;
            proceed(button);
        });
    }, true);
})();
