using System;
using System.Collections.Generic;

namespace OnlineCompanionBookingSystem
{
    // Hawak ng Session habang hindi pa tapos ang payment; wala pang account sa database sa yugtong ito.
    [Serializable]
    public class PendingRegistration
    {
        public string Role { get; set; }              // "Customer" o "Companion"
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string PasswordHash { get; set; }
        public string ReferenceNo { get; set; }
        public decimal Fee { get; set; } = RegistrationService.RegistrationFee;

        // Para sa Companion lang
        public decimal HourlyRate { get; set; }
        public string Bio { get; set; }               // optional "About You" text (null when empty)
        public string Gender { get; set; }            // "Female" (the only gender accepted for companions); checked by the admin against the ID
        public List<string> Activities { get; set; } = new List<string>();
        public string DocPath { get; set; }           // "~/Uploads/xxxx.jpg"
        public string DocPhysicalPath { get; set; }   // para mabura kung ika-cancel
    }

    // Datos ng resibo na ipinapakita sa success screen at ipinapadala sa email pagkatapos ng bayad
    [Serializable]
    public class PaymentReceipt
    {
        public string Role { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ReferenceNo { get; set; }
        public string Method { get; set; }
        public decimal Amount { get; set; }
        public DateTime DatePaid { get; set; }
    }
}
