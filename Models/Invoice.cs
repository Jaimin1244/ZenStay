using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Learn_Auth.Models
{
    public class Invoice
    {
        public int InvoiceID { get; set; }
        public int BookingID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string HotelName { get; set; }
        public string RoomNo { get; set; }
        public decimal Price { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Tax { get; set; }
        public decimal Discounts { get; set; }
        public decimal FinalAmount { get; set; }

        public virtual Booking Booking { get; set; }
    }
}