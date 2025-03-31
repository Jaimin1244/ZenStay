using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Learn_Auth.Models
{
    public class Booking
    {
        public int BookingID { get; set; }
        public int UserID { get; set; }
        public int RoomID { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string BookingStatus { get; set; }

        public virtual User User { get; set; }
        public virtual Room Room { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
    }

}