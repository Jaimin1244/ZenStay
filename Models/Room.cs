using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Learn_Auth.Models
{
    public class Room
    {
        public int RoomId { get; set; }
        public int HotelId { get; set; }
        public string RoomNumber { get; set; }
        public string RoomType { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public string Amenities { get; set; }
        public string RoomImage { get; set; }
        // New property for file upload (not mapped to the database)
        [NotMapped]
        public HttpPostedFileBase RoomImageFile { get; set; }

        public virtual Hotel Hotel { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
    }

}