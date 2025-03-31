using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Learn_Auth.Models
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public int BookingID { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string RefundStatus { get; set; }

        public virtual Booking Booking { get; set; }
    }

}