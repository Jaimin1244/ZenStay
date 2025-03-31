using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Learn_Auth.ViewModel
{
    public class InvoiceTemplateViewModel
    {
        public int BookingID { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string BookingStatus { get; set; }

        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }

        public int InvoiceID { get; set; }
        public string HotelName { get; set; }
        public string RoomNumber { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime InvoiceDate { get; set; }
    }

}