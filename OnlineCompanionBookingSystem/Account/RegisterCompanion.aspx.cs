using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;

namespace OnlineCompanionBookingSystem.Account
{
    // Registration form ng Companion (may ID upload, activities, at hourly rate).
    // Gaya ng Customer, hindi pa gumagawa ng account dito: ini-save sa Session at dinadala sa Payment.aspx.
    // Pagkatapos ng bayad, "Pending" ang verification hanggang ma-approve ng admin.
    public partial class RegisterCompanion : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnRegisterCompanion_Click(object sender, EventArgs e)
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

            // Rate per hour: a whole number of pesos inside the allowed range. The page already checks this
            // (RangeValidator), but the server checks again because browser checks can be bypassed.
            int hourlyRateWhole;
            if (!int.TryParse(txtRate.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out hourlyRateWhole) ||
                hourlyRateWhole < RegistrationService.MinHourlyRate || hourlyRateWhole > RegistrationService.MaxHourlyRate)
            {
                ShowSweetAlert("Invalid Rate", "Rate per hour must be a whole number from 50 to 10,000 pesos.", "warning");
                return;
            }
            decimal hourlyRate = hourlyRateWhole;

            // "About You" is optional, but limited in length (the box also stops typing at the limit)
            string bioError = ProfileValidator.ValidateBio(txtBio.Text);
            if (bioError != null)
            {
                ShowSweetAlert("About You Too Long", bioError, "warning");
                return;
            }

            // Kunin ang mga napiling activities
            var selectedActivities = cblActivities.Items.Cast<ListItem>()
                                        .Where(item => item.Selected)
                                        .Select(item => item.Value)
                                        .ToList();

            // File Upload Check
            if (!fuVerificationDoc.HasFile)
            {
                ShowSweetAlert("Missing Document", "Please upload a verification document (Valid ID).", "warning");
                return;
            }

            // The file type, size, and real image content were already checked by the cvDocFile validator
            // (JPG/PNG images only), so only the extension is needed here for the saved file name.
            string fileName = Path.GetFileName(fuVerificationDoc.PostedFile.FileName);
            string fileExtension = Path.GetExtension(fileName).ToLower();

            string savedDocumentPath = null;

            try
            {
                // Tignan kung nakaregister na ang email
                if (RegistrationService.EmailExists(email))
                {
                    ShowSweetAlert("Email Already Exists", "This email address is already registered. Please use a different email or log in.", "warning");
                    return;
                }

                // I-save ang uploaded verification file sa ~/Uploads/ (mabubura kung hindi matuloy ang payment)
                string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                string uploadFolder = Server.MapPath("~/Uploads/");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                string filePath = Path.Combine(uploadFolder, uniqueFileName);
                fuVerificationDoc.SaveAs(filePath);
                savedDocumentPath = filePath;

                // Wala pang account na ginagawa: hawak muna sa Session hanggang matapos ang payment
                Session["PendingReg"] = new PendingRegistration
                {
                    Role = "Companion",
                    FullName = fullName,
                    Email = email,
                    Contact = contact,
                    PasswordHash = PasswordHelper.HashPassword(password),
                    HourlyRate = hourlyRate,
                    Bio = ProfileValidator.NormalizeBio(txtBio.Text),
                    Activities = selectedActivities,
                    DocPath = "~/Uploads/" + uniqueFileName,
                    DocPhysicalPath = filePath,
                    ReferenceNo = RegistrationService.NewReferenceNo()
                };

                Response.Redirect("~/Account/Payment.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                // Burahin ang na-upload na ID kung pumalya ang registration, para walang naiiwang file
                if (!string.IsNullOrEmpty(savedDocumentPath) && File.Exists(savedDocumentPath))
                {
                    File.Delete(savedDocumentPath);
                }

                System.Diagnostics.Debug.WriteLine("Companion registration error: " + ex);
                ShowSweetAlert("Registration Error", "We could not continue your registration. Please try again.", "error");
            }
        }

        // Nagpapakita ng SweetAlert popup sa pamamagitan ng startup script
        private void ShowSweetAlert(string title, string message, string icon)
        {
            string script = $"Swal.fire({{ title: '{title}', text: '{message}', icon: '{icon}', confirmButtonColor: '#007bff' }});";
            ClientScript.RegisterStartupScript(this.GetType(), "SweetAlertNotif", script, true);
        }

        // Valid ID must be a real JPG or PNG image (checks the extension, the 5 MB limit, and the file's first bytes,
        // so a renamed PDF or program is rejected). An empty file box is left to the "required" validator.
        protected void cvDocFile_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (!fuVerificationDoc.HasFile)
            {
                args.IsValid = true;
                return;
            }

            string error = ProfileValidator.ValidateImageFile(fuVerificationDoc.PostedFile, ProfileValidator.MaxIdImageBytes, "ID image");
            args.IsValid = error == null;
            if (error != null) cvDocFile.ErrorMessage = error;
        }

        // Kailangang may kahit isang activity na napili
        protected void cvActivities_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = cblActivities.Items.Cast<ListItem>().Any(item => item.Selected);
        }

        protected void cvAgreeFee_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = chkAgreeFee.Checked;
        }
    }
}
