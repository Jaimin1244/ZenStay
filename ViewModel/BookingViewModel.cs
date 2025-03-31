using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Learn_Auth.ViewModel
{
    public class BookingViewModel
    {
        // Guest details (for whom the booking is being made)
        [Required]
        public string GuestName { get; set; }

        [Required, EmailAddress]
        public string GuestEmail { get; set; }

        [Required]
        public string GuestPhone { get; set; }
        public string GuestAddress { get; set; }

        // Hotel selection
        [Required(ErrorMessage = "Please select a hotel.")]
        public int HotelId { get; set; }

        // Single room selection
        [Required(ErrorMessage = "Please select a room.")]
        public int RoomId { get; set; }

        // Booking dates
        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        // Calculated total price
        public decimal TotalPrice { get; set; }

        // Dropdown lists for Hotels & Rooms
        public List<SelectListItem> Hotels { get; set; }
        public List<SelectListItem> Rooms { get; set; }
    }
}
