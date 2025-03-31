using Learn_Auth.Models;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Learn_Auth.Attributes;
using System.Collections.Generic;
using System.Data.Entity;

namespace Learn_Auth.Controllers
{
    public class BookingController : Controller
    {
        private readonly AuthDbContext _context = new AuthDbContext();

        // GET: Booking
        [RoleAuthorize(1)]
        public ActionResult Index()
        {
            // Lists all bookings (e.g., for admin or a general listing)
            var bookings = _context.Bookings
                .OrderByDescending(b => b.BookingDate)
                .ToList();
            return View(bookings);
        }

        // GET: Booking/Details/5
        [RoleAuthorize(1,2,3)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Invalid booking id.";
                return RedirectToAction("Index");
            }

            var booking = _context.Bookings
                            .Include(b => b.Room.Hotel)
                            .Include(b => b.User)
                            .Include(b => b.Payments)
                            .Include(b => b.Invoices)
                            .FirstOrDefault(b => b.BookingID == id);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("Index");
            }
            return View(booking);
        }

        // GET: Booking/Create
        [RoleAuthorize(3)]
        public ActionResult Create(int? roomId)
        {
            if (roomId == null)
            {
                return RedirectToAction("SelectHotelRoom");
            }

            var room = _context.Rooms.Find(roomId);
            if (room == null)
            {
                TempData["ErrorMessage"] = "Room not found.";
                return RedirectToAction("Index", "Room");
            }

            //var bookedRoom = _context.Rooms.FirstOrDefault(r => r.RoomId == roomId && r.IsAvailable);
            //if (bookedRoom== null)
            //{
            //    TempData["ErrorMessage"] = "Selected room is not available.";
            //    return RedirectToAction("Details", "Hotel", new { id = room.HotelId });
            //}

            // Calculate total amount based on 1-day default booking
            var defaultStayDuration = 1;
            var totalAmount = room.Price * defaultStayDuration;

            // Prepopulate a new booking with room id and default dates.
            var model = new Booking
            {
                RoomID = room.RoomId,
                Room = room,
                CheckInDate = DateTime.Now,
                CheckOutDate = DateTime.Now.AddDays(1),
                BookingDate = DateTime.Now,
                BookingStatus = "Pending",

                Payments = new List<Payment>
                {
                    new Payment
                    {
                        PaymentAmount = totalAmount
                    }
                }
            };
            ViewBag.ActivePage = "Booking";
            return View(model);
        }

        // GET: Booking/SelectHotelRoom
        [RoleAuthorize(3)]
        public ActionResult SelectHotelRoom()
        {
            var hotels = _context.Hotels.ToList();
            ViewBag.Hotels = new SelectList(hotels, "HotelId", "HotelName");
            return View();
        }

        // GET: Booking/GetRoomsByHotelId
        [HttpGet]
        public JsonResult GetRoomsByHotelId(int hotelId)
        {
            var rooms = _context.Rooms
                .Where(r => r.HotelId == hotelId)
                .Select(r => new
                {
                    r.RoomId,
                    DisplayText = r.RoomNumber + " - " + r.RoomType
                })
                .ToList();
            return Json(rooms, JsonRequestBehavior.AllowGet);
        }

        // POST: Booking/RedirectToBooking
        [HttpPost]
        [RoleAuthorize(3)]
        [ValidateAntiForgeryToken]
        public ActionResult RedirectToBooking(int SelectedRoomId)
        {
            if (SelectedRoomId == 0)
            {
                TempData["ErrorMessage"] = "No room selected.";
                return RedirectToAction("Create");
            }

            // Redirect to the Booking/Create action with the selected RoomId.
            return RedirectToAction("Create", new { roomId = SelectedRoomId });
        }


        // POST: Booking/Create
        [HttpPost]
        [RoleAuthorize(3)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Booking booking)
        {
            if (ModelState.IsValid)
            {
                // Set the current user as the booking owner.
                var currentUserEmail = User.Identity.Name;
                var user = _context.Users.FirstOrDefault(u => u.Email == currentUserEmail);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Login", "Account");
                }
                booking.UserID = user.Id;
                booking.BookingDate = DateTime.Now;
                booking.BookingStatus = "Pending Payment";

                // ✅ Final Check in Create Method (before saving)
                bool isRoomAvailable = !_context.Bookings.Any(b =>
                    b.RoomID == booking.RoomID &&
                    b.BookingStatus != "Cancelled" && // Skip cancelled bookings
                    (b.CheckInDate <= booking.CheckOutDate && b.CheckOutDate >= booking.CheckInDate)
                );

                if (!isRoomAvailable)
                {
                    TempData["ErrorMessage"] = "Room is no longer available for the selected dates.";
                    return RedirectToAction("Create", new { roomId = booking.RoomID });
                }


                _context.Bookings.Add(booking);
                _context.SaveChanges();

                var room = _context.Rooms.FirstOrDefault(r => r.RoomId == booking.RoomID);
                if(room != null)
                {
                    room.IsAvailable = false;
                    _context.Entry(room).State = System.Data.Entity.EntityState.Modified;
                    _context.SaveChanges();
                };

                // Calculate number of nights
                int numberOfNights = (booking.CheckOutDate - booking.CheckInDate).Days;
                if (numberOfNights <= 0)
                {
                    numberOfNights = 1;
                }

                // Get the price per night
                decimal pricePerNight = _context.Rooms
                                        .Where(r => r.RoomId == booking.RoomID)
                                        .Select(r => r.Price)
                                        .FirstOrDefault();

                // Calculate total payment amount
                decimal totalAmount = numberOfNights * pricePerNight;

                // Create payment object
                var payment = new Payment
                {
                    BookingID = booking.BookingID,
                    PaymentStatus = "Pending",
                    PaymentAmount = totalAmount,
                    PaymentDate = DateTime.Now
                };


                _context.Payments.Add(payment);
                _context.SaveChanges();

                ModelState.Clear();
                Session.Remove("BookingData");


                TempData["SuccessMessage"] = "Booking created successfully.";
                return RedirectToAction("MakePayment","Payment", new { bookingId = booking.BookingID });
            }
            TempData["ErrorMessage"] = "Invalid booking data.";
            return View(booking);
        }

        // GET: Booking/Edit/5
        [RoleAuthorize(3)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Invalid booking id.";
                return RedirectToAction("UserBookings");
            }
            var booking = _context.Bookings.Find(id);
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("UserBookings");
            }
            if(booking.BookingStatus == "Booked")
            {
                TempData["ErrorMessage"] = "Payment for this Booking is done.";
                return RedirectToAction("UserBookings");
            }
            // Allow editing only if the current user owns the booking.
            var currentUserEmail = User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.Email == currentUserEmail);
            if (user == null || booking.UserID != user.Id)
            {
                TempData["ErrorMessage"] = "You are not authorized to edit this booking.";
                return RedirectToAction("UserBookings");
            }
            return View(booking);
        }

        // POST: Booking/Edit/5
        [HttpPost]
        [RoleAuthorize(3)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Booking booking)
        {
            if (ModelState.IsValid)
            {
                var existingBooking = _context.Bookings.Find(booking.BookingID);
                if (existingBooking == null)
                {
                    TempData["ErrorMessage"] = "Booking not found.";
                    return RedirectToAction("Index");
                }
                // Verify the current user owns the booking.
                var currentUserEmail = User.Identity.Name;
                var user = _context.Users.FirstOrDefault(u => u.Email == currentUserEmail);
                if (user == null || existingBooking.UserID != user.Id)
                {
                    TempData["ErrorMessage"] = "You are not authorized to edit this booking.";
                    return RedirectToAction("UserBookings");
                }
                // Update booking details. You can add more fields as necessary.
                existingBooking.CheckInDate = booking.CheckInDate;
                existingBooking.CheckOutDate = booking.CheckOutDate;
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Booking updated successfully.";
                return RedirectToAction("Details", new { id = booking.BookingID });
            }
            TempData["ErrorMessage"] = "Invalid booking data.";
            return View(booking);
        }

        // GET: Booking/Cancel/5
        [RoleAuthorize(3)]
        public ActionResult Cancel(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Invalid booking id.";
                return RedirectToAction("Index");
            }
            var booking = _context.Bookings.Find(id);
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("Index");
            }
            // Verify ownership.
            var currentUserEmail = User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.Email == currentUserEmail);
            if (user == null || booking.UserID != user.Id)
            {
                TempData["ErrorMessage"] = "You are not authorized to cancel this booking.";
                return RedirectToAction("UserBookings");
            }
            return View(booking);
        }

        // POST: Booking/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [RoleAuthorize(3)]
        [ValidateAntiForgeryToken]
        public ActionResult CancelConfirmed(int id)
        {
            var booking = _context.Bookings.Find(id);
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("Index");
            }
            // Verify ownership.
            var currentUserEmail = User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.Email == currentUserEmail);
            if (user == null || booking.UserID != user.Id)
            {
                TempData["ErrorMessage"] = "You are not authorized to cancel this booking.";
                return RedirectToAction("UserBookings");
            }
            var room = _context.Rooms.FirstOrDefault(r => r.RoomId == booking.RoomID);
            if (room != null)
            {
                room.IsAvailable = true;
                _context.Entry(room).State = System.Data.Entity.EntityState.Modified;
                _context.SaveChanges();
            };
            // Remove the booking.
            _context.Bookings.Remove(booking);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Booking cancelled successfully.";
            return RedirectToAction("UserBookings");
        }

        // GET: Booking/UserBookings
        [RoleAuthorize(3)]
        public ActionResult UserBookings()
        {
            var currentUserEmail = User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.Email == currentUserEmail);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login", "Account");
            }
            var bookings = _context.Bookings
                .Where(b => b.UserID == user.Id)
                .OrderByDescending(b => b.BookingDate)
                .ToList();
            return View(bookings);
        }

        [HttpGet]
        public JsonResult CheckRoomAvailability(int roomId, DateTime desiredCheckIn, DateTime desiredCheckOut)
        {
            bool isAvailable = !_context.Bookings.Any(b =>
                b.RoomID == roomId &&
                (b.CheckInDate <= desiredCheckOut && b.CheckOutDate >= desiredCheckIn)
            );

            // Return JSON so the client-side can parse it easily.
            return Json(isAvailable, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetBookedDates(int roomId)
        {
            var bookedDates = _context.Bookings
                .Where(b => b.RoomID == roomId)
                .Select(b => new
                {
                    StartDate = b.CheckInDate,
                    EndDate = b.CheckOutDate
                })
                .ToList();

            return Json(bookedDates, JsonRequestBehavior.AllowGet);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
