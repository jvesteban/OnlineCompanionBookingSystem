using System;
using System.Web.UI.WebControls;

namespace OnlineCompanionBookingSystem.Account
{
    // Registration form ng Customer. Hindi pa gumagawa ng account dito: ini-save muna ang datos sa Session
    // at dinadala sa Payment.aspx; doon ginagawa ang account pagkatapos ng bayad.
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            string firstName = txtFirstName.Text.Trim();
            string middleName = txtMiddleName.Text.Trim();
            string lastName = txtLastName.Text.Trim();

            // Pagdugtungin ang pangalan para sa Users table FullName column
            string fullName = string.IsNullOrWhiteSpace(middleName)
                ? $"{firstName} {lastName}"
                : $"{firstName} {middleName} {lastName}";

            string email = txtEmail.Text.Trim();
            string contact = txtContact.Text.Trim();
            string password = txtPassword.Text.Trim();

            try
            {
                // Tignan kung may kaparehong email na nakatala
                if (RegistrationService.EmailExists(email))
                {
                    ShowSweetAlert("Email Already Exists", "This email address is already registered. Please use another email or log in.", "warning");
                    return;
                }

                // Wala pang account na ginagawa: hawak muna sa Session hanggang matapos ang payment
                Session["PendingReg"] = new PendingRegistration
                {
                    Role = "Customer",
                    FullName = fullName,
                    Email = email,
                    Contact = contact,
                    PasswordHash = PasswordHelper.HashPassword(password),
                    ReferenceNo = RegistrationService.NewReferenceNo()
                };

                Response.Redirect("~/Account/Payment.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Customer registration error: " + ex);
                ShowSweetAlert("Registration Error", "We could not continue your registration. Please try again.", "error");
            }
        }

        // Nagpapakita ng SweetAlert popup (library na naka-load sa .aspx) sa pamamagitan ng startup script
        private void ShowSweetAlert(string title, string message, string icon)
        {
            string script = $"Swal.fire({{ title: '{title}', text: '{message}', icon: '{icon}', confirmButtonColor: '#007bff' }});";
            ClientScript.RegisterStartupScript(this.GetType(), "SweetAlertNotif", script, true);
        }

        // Server-side validator: kailangang naka-check ang "I agree to pay the registration fee"
        protected void cvAgreeFee_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = chkAgreeFee.Checked;
        }
    }
}
