using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace OnlineCompanionBookingSystem.Account
{
    // Simulated checkout para sa registration fee (walang totoong singil at walang card details).
    // Steps: pumili ng method -> authorize -> resibo. Ang account ay ginagawa lang sa Authorize,
    // gamit ang data na hawak ng Session["PendingReg"] mula sa registration page.
    public partial class Payment : Page
    {
        // Mga control (walang designer file para sa page na ito)
        protected HtmlGenericControl stepPay, stepDone;
        protected Panel pnlSummary, pnlSelect, pnlAuthorize, pnlSuccess;
        protected Literal litRole, litName, litEmail, litFee, litTotal, litMethods;
        protected Literal litProvider, litAuthAmount, litRef, litAuthMethod, litAuthAccountLabel, litAuthAccount;
        protected Literal litSuccessLead, litRcRef, litRcMethod, litRcDate, litRcAmount, litRcEmail;
        protected Label lblError;
        protected HiddenField hfMethod;
        protected Button btnContinue, btnCancel, btnAuthorize, btnChangeMethod;

        // Registration na naghihintay ng bayad; null kapag nag-expire ang session o wala pang nagre-register
        private PendingRegistration Pending => Session["PendingReg"] as PendingRegistration;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Tapos na ang bayad: ipakita ang resibo (Post-Redirect-Get para hindi maulit ang bayad kapag nag-refresh)
            var receipt = Session["PaymentReceipt"] as PaymentReceipt;
            if (Request.QueryString["done"] == "1" && receipt != null)
            {
                ShowSuccess(receipt);
                return;
            }

            if (Pending == null)
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                if (string.IsNullOrEmpty(Pending.ReferenceNo))
                    Pending.ReferenceNo = RegistrationService.NewReferenceNo();

                litRole.Text = Pending.Role;
                litName.Text = HttpUtility.HtmlEncode(Pending.FullName);
                litEmail.Text = HttpUtility.HtmlEncode(Pending.Email);
                litFee.Text = Pending.Fee.ToString("N2");
                litTotal.Text = Pending.Fee.ToString("N2");
                litMethods.Text = BuildMethodsHtml(null);
            }
        }

        // Gumagawa ng radio cards ng mga payment method. Plain HTML ang gamit (hindi asp:RadioButton)
        // kaya binabasa ang pinili gamit ang Request.Form["payMethod"]; "selected" ang naka-check na.
        private static string BuildMethodsHtml(string selected)
        {
            var descriptions = new[]
            {
                "Pay with your GCash wallet",
                "Pay with your Maya wallet",
                "Visa or Mastercard",
                "Pay through your bank's online banking"
            };

            var sb = new StringBuilder();
            for (int i = 0; i < RegistrationService.PaymentMethods.Length; i++)
            {
                string m = RegistrationService.PaymentMethods[i];
                string id = "pm" + i;
                bool isSelected = string.Equals(m, selected, StringComparison.Ordinal);
                sb.Append("<label class='method" + (isSelected ? " selected" : "") + "' for='" + id + "'>")
                  .Append("<input type='radio' name='payMethod' id='" + id + "' value='" + HttpUtility.HtmlAttributeEncode(m) + "'" + (isSelected ? " checked" : "") + " />")
                  .Append("<span class='method-name'>" + HttpUtility.HtmlEncode(m) + "</span>")
                  .Append("<span class='method-desc'>" + descriptions[i] + "</span>")
                  .Append("</label>");
            }
            return sb.ToString();
        }

        // Server-side na pag-check na galing sa listahan ang method (hindi mapapalitan ng user ang value sa browser)
        private static bool IsValidMethod(string method)
        {
            return Array.IndexOf(RegistrationService.PaymentMethods, method) >= 0;
        }

        private void ShowError(string message)
        {
            lblError.Text = HttpUtility.HtmlEncode(message);
            lblError.Visible = true;
        }

        // Step 1 -> 2: i-validate ang napiling method at ipakita ang authorize screen
        protected void btnContinue_Click(object sender, EventArgs e)
        {
            string method = Request.Form["payMethod"];
            if (!IsValidMethod(method))
            {
                litMethods.Text = BuildMethodsHtml(null);
                ShowError("Please select a payment method to continue.");
                return;
            }

            hfMethod.Value = method;
            var p = Pending;

            litProvider.Text = HttpUtility.HtmlEncode(method);
            litAuthAmount.Text = p.Fee.ToString("N2");
            litRef.Text = HttpUtility.HtmlEncode(p.ReferenceNo);
            litAuthMethod.Text = HttpUtility.HtmlEncode(method);

            if (method == "GCash" || method == "Maya")
            {
                litAuthAccountLabel.Text = "<span>Mobile number</span>";
                litAuthAccount.Text = MaskMobile(p.Contact);
            }
            else if (method == "Credit / Debit Card")
            {
                litAuthAccountLabel.Text = "<span>Card</span>";
                litAuthAccount.Text = "Test card (no details required)";
            }
            else
            {
                litAuthAccountLabel.Text = "<span>Account</span>";
                litAuthAccount.Text = "Test bank account";
            }

            pnlSummary.Visible = false;
            pnlSelect.Visible = false;
            pnlAuthorize.Visible = true;
        }

        protected void btnChangeMethod_Click(object sender, EventArgs e)
        {
            litMethods.Text = BuildMethodsHtml(hfMethod.Value);
            pnlSummary.Visible = true;
            pnlAuthorize.Visible = false;
            pnlSelect.Visible = true;
        }

        // Step 2 -> 3: "bayaran" ang fee. Dito ginagawa ang account at ang Payments record (sa iisang transaction),
        // tapos nagpapadala ng mga email at nagre-redirect sa resibo.
        protected void btnAuthorize_Click(object sender, EventArgs e)
        {
            var p = Pending;
            string method = hfMethod.Value;
            if (p == null || !IsValidMethod(method))
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            // Maikling paghihintay para maramdaman na may "pinoproseso"
            Thread.Sleep(1500);

            try
            {
                RegistrationService.CompleteRegistration(p, method);
            }
            catch (InvalidOperationException ex) when (ex.Message == "EmailExists")
            {
                ShowFailed("This email address was registered while you were paying. Please log in or use another email.");
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Payment completion error: " + ex);
                ShowFailed("We could not complete your registration. You have not been charged. Please try again.");
                return;
            }

            var receipt = new PaymentReceipt
            {
                Role = p.Role,
                FullName = p.FullName,
                Email = p.Email,
                ReferenceNo = p.ReferenceNo,
                Method = method,
                Amount = p.Fee,
                DatePaid = DateTime.Now
            };

            // Mga email: hindi pinipigilan ang registration kapag pumalya ang pagpapadala
            if (p.Role == "Companion")
                EmailHelper.SendCompanionApplicationReceived(p.Email, p.FullName);
            else
                EmailHelper.SendCustomerWelcome(p.Email, p.FullName);
            EmailHelper.SendPaymentReceipt(receipt);

            Session["PaymentReceipt"] = receipt;
            Session.Remove("PendingReg");
            Response.Redirect("~/Account/Payment.aspx?done=1", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        // Kapag pumalya ang pagbabayad: ipakita ang error at ibalik sa pagpili ng method (walang account na nagawa)
        private void ShowFailed(string message)
        {
            ShowError(message);
            pnlSummary.Visible = true;
            litMethods.Text = BuildMethodsHtml(hfMethod.Value);
            pnlAuthorize.Visible = false;
            pnlSelect.Visible = true;
        }

        // Kanselahin ang registration: linisin ang Session at bumalik sa tamang registration page ayon sa role
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            var p = Pending;
            string back = "~/Account/RegisterCustomer.aspx";

            if (p != null)
            {
                if (p.Role == "Companion")
                {
                    back = "~/Account/RegisterCompanion.aspx";
                    // Burahin ang na-upload na ID dahil hindi natuloy ang registration
                    if (!string.IsNullOrEmpty(p.DocPhysicalPath) && File.Exists(p.DocPhysicalPath))
                    {
                        try { File.Delete(p.DocPhysicalPath); } catch { }
                    }
                }
                Session.Remove("PendingReg");
            }

            Response.Redirect(back);
        }

        // Step 3: itago ang ibang panel at ipakita ang resibo
        private void ShowSuccess(PaymentReceipt r)
        {
            stepPay.Attributes["class"] = "done";
            stepDone.Attributes["class"] = "active";

            pnlSummary.Visible = false;
            pnlSelect.Visible = false;
            pnlAuthorize.Visible = false;
            pnlSuccess.Visible = true;

            litSuccessLead.Text = r.Role == "Companion"
                ? "Your companion application has been submitted and is now pending admin verification. You will be able to log in once it has been approved."
                : "Your customer account is ready. You can now log in and start booking verified companions.";
            litRcRef.Text = HttpUtility.HtmlEncode(r.ReferenceNo);
            litRcMethod.Text = HttpUtility.HtmlEncode(r.Method);
            litRcDate.Text = r.DatePaid.ToString("MMM dd, yyyy h:mm tt");
            litRcAmount.Text = r.Amount.ToString("N2");
            litRcEmail.Text = HttpUtility.HtmlEncode(r.Email);
        }

        // 09171234567 -> 0917 *** 4567
        private static string MaskMobile(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile) || mobile.Length < 8) return "Registered mobile number";
            return mobile.Substring(0, 4) + " *** " + mobile.Substring(mobile.Length - 4);
        }
    }
}
