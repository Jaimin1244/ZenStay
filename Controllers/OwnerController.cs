using Learn_Auth.Attributes;
using Learn_Auth.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Learn_Auth.Controllers
{
    [Authorize]
    public class OwnerController : Controller
    {
        private readonly AuthDbContext _context = new AuthDbContext();

        //GET : Dashboard

        [RoleAuthorize(2)]
        public ActionResult Dashboard()
        {
            // Get the logged-in hotel owner’s ID (stored in Session)
            int hotelOwnerId = Convert.ToInt32(Session["UserId"]);

            if (hotelOwnerId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the list of hotel IDs owned by the current user
            var hotelIds = _context.Hotels
                                   .Where(h => h.UserId == hotelOwnerId)
                                   .Select(h => h.HotelId)
                                   .ToList();

            if (!hotelIds.Any())
            {
                TempData["WarnMessage"] = "You do not have any hotel right now.";
                return View(new DashboardViewModel());
            }

            var myHotels = _context.Hotels
       .Where(h => h.UserId == hotelOwnerId)
       .Include(h => h.Rooms)
       .ToList();

            var myRooms = _context.Rooms
        .Where(r => r.Hotel.UserId == hotelOwnerId)
        .Include(r => r.Bookings) 
        .ToList();

            var todayBookings = _context.Bookings
    .Where(b => DbFunctions.TruncateTime(b.CheckInDate) == DbFunctions.TruncateTime(DateTime.Today))
    .Include(b => b.Room)
    .ToList();


            // Filter the bookings through the Room's HotelId
            int totalHotels = hotelIds.Count();

            int totalBookings = _context.Bookings
                .Count(b => hotelIds.Contains(b.Room.HotelId));

            int availableRooms = _context.Rooms
                .Count(r => hotelIds.Contains(r.HotelId) && r.IsAvailable);

            int pendingCheckIns = _context.Bookings
                .Count(b => hotelIds.Contains(b.Room.HotelId) &&
                    DbFunctions.TruncateTime(b.CheckInDate) == DbFunctions.TruncateTime(DateTime.Today) &&
                    b.BookingStatus == "Pending");

            int pendingCheckOuts = _context.Bookings
                .Count(b => hotelIds.Contains(b.Room.HotelId) &&
                    DbFunctions.TruncateTime(b.CheckOutDate) == DbFunctions.TruncateTime(DateTime.Today) &&
                    b.BookingStatus == "Checked-In");

            decimal revenueToday = _context.Payments
                .Where(p => hotelIds.Contains(p.Booking.Room.HotelId) &&
                    DbFunctions.TruncateTime(p.PaymentDate) == DbFunctions.TruncateTime(DateTime.Today))
                .Sum(p => (decimal?)p.PaymentAmount) ?? 0;

            var dashboardData = new DashboardViewModel
            {
                TotalHotels = totalHotels,
                TotalBookings = totalBookings,
                AvailableRooms = availableRooms,
                PendingCheckIns = pendingCheckIns,
                PendingCheckOuts = pendingCheckOuts,
                RevenueToday = revenueToday,
                MyHotels = myHotels,
                MyRooms = myRooms,
                TodaysBookings = todayBookings

            };

            return View(dashboardData);
        }

        public ActionResult GetRooms(int page = 1, int pageSize = 5)
        {
            int hotelOwnerId = Convert.ToInt32(Session["UserId"]);

            if (hotelOwnerId == 0)
            {
                return Json(new { success = false, message = "Unauthorized access." }, JsonRequestBehavior.AllowGet);
            }

            var query = _context.Rooms
                .Where(r => r.Hotel.UserId == hotelOwnerId)
                .Include(r => r.Hotel)
                .OrderBy(r => r.RoomId);

            int totalRecords = query.Count();
            var rooms = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Json(new
            {
                success = true,
                data = rooms.Select(r => new
                {
                    RoomId = r.RoomId,
                    RoomNumber = r.RoomNumber,
                    HotelName = r.Hotel.HotelName,
                    RoomType = r.RoomType,
                    Price = r.Price.ToString("C"),
                    IsAvailable = r.IsAvailable ? "Available" : "Booked"
                }),
                totalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                currentPage = page
            }, JsonRequestBehavior.AllowGet);
        }


    }
}