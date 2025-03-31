using System.Collections.Generic;

namespace Learn_Auth.Models
{
    public class DashboardViewModel
    {
        public int TotalHotels { get; set; }
        public int TotalBookings { get; set; }
        public int AvailableRooms { get; set; }
        public int PendingCheckIns { get; set; }
        public int PendingCheckOuts { get; set; }
        public decimal RevenueToday { get; set; }

        public List<Hotel> MyHotels { get; set; }
        public List<Room> MyRooms { get; set; }
        public List<Booking> TodaysBookings { get; set; }
    }
}
