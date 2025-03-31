using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Learn_Auth.ViewModel
{
    public class VerifyOTPViewModel
    {
        public string Email { get; set; }
        public string OTP { get; set; }
        public DateTime? OTPExpiry { get; set; }
    }
}