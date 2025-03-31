using Learn_Auth.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Learn_Auth.Attributes;

namespace Learn_Auth.Controllers
{
    public class RoomController : Controller
    {
        private readonly AuthDbContext _context = new AuthDbContext();

        // GET: Room
        public ActionResult Index(int hotelId = 0)
        {
            var hotels = _context.Hotels.Include(h => h.Rooms).ToList();

            if (hotelId == 0 && hotels.Any())
                hotelId = hotels.First().HotelId; // Default to first hotel

            var rooms = _context.Rooms
                         .Where(r => r.Hotel.HotelId == hotelId)
                         .OrderBy(r => r.RoomNumber)
                         .ToList();

            var roomIds = rooms.Select(r => r.RoomId).ToList();
            var bookings = _context.Bookings
                           .Where(b => roomIds.Contains(b.RoomID))
                           .ToList();

            ViewBag.Bookings = bookings;

            ViewBag.Hotels = new SelectList(hotels, "HotelId", "HotelName", hotelId);
            ViewBag.SelectedHotelId = hotelId;

            return View(rooms);
        }

        // GET: Room/Create - Returns PartialView for modal
        [RoleAuthorize(2)]
        public ActionResult Create()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var currentUser = _context.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "Unable to retrieve user. Please ensure you're logged in.";
                return RedirectToAction("Index", "Home");
            }
            if (!currentUser.IsVerified)
            {
                TempData["ErrorMessage"] = "Please verify your email first.";
                return RedirectToAction("Profile", "Account");
            }
            ViewBag.HotelId = new SelectList(_context.Hotels.Where(h => h.UserId == currentUser.Id), "HotelId", "HotelName");
            return PartialView("_Create");
        }

        // POST: Room/Create
        [HttpPost]
        [RoleAuthorize(2)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Room room)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var currentUser = _context.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (currentUser == null || !currentUser.IsVerified)
            {
                TempData["ErrorMessage"] = "You are not authorized to perform this action.";
                return RedirectToAction("Index", "Home");
            }
            // Verify that the hotel belongs to the current user
            var hotel = _context.Hotels.FirstOrDefault(h => h.HotelId == room.HotelId);
            if (hotel == null || hotel.UserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "You are not the owner of the selected hotel.";
                return RedirectToAction("Dashboard", "Owner");
            }
            if (ModelState.IsValid)
            {
                var existingRoom = _context.Rooms.FirstOrDefault(r => r.RoomNumber == room.RoomNumber && r.HotelId == room.HotelId);
                if (existingRoom != null)
                {
                    TempData["ErrorMessage"] = "A room with this number already exists for the selected hotel.";
                    return RedirectToAction("Index", "Hotel");
                }
                // Process file upload if a file was provided
                if (room.RoomImageFile != null && room.RoomImageFile.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(room.RoomImageFile.FileName);
                    var uploadDir = "~/Content/images/Rooms";
                    var imagePath = Path.Combine(Server.MapPath(uploadDir), fileName);

                    room.RoomImageFile.SaveAs(imagePath);
                    room.RoomImage = Path.Combine(uploadDir, fileName);
                }

                _context.Rooms.Add(room);
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Room #{room.RoomNumber} created successfully.";
                return RedirectToAction("Dashboard", "Owner");
            }

            TempData["ErrorMessage"] = "Error while creating room. Please check the provided data.";
            ViewBag.HotelId = new SelectList(_context.Hotels.Where(h => h.UserId == currentUser.Id), "HotelId", "HotelName");
            return PartialView("_Create", room);
        }

        // GET: Room/Details/5 - Returns PartialView for modal
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Room room = _context.Rooms.Find(id);
            if (room == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Details", room);
        }

        // GET: Room/Edit/5 - Returns PartialView for modal
        [RoleAuthorize(2)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Your request is invalid.";
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var currentUser = _context.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "Unable to retrieve user. Please ensure you're logged in.";
                return RedirectToAction("Index", "Home");
            }
            Room room = _context.Rooms.Find(id);
            if (room == null)
            {
                TempData["ErrorMessage"] = "Room not found.";
                return HttpNotFound();
            }
            // Verify that the room's hotel belongs to the current user
            var hotel = _context.Hotels.FirstOrDefault(h => h.HotelId == room.HotelId);
            if (hotel == null || hotel.UserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "You are not the owner of this room's hotel.";
                return RedirectToAction("Dashboard", "Owner");
            }
            ViewBag.HotelId = new SelectList(_context.Hotels.Where(h => h.UserId == currentUser.Id), "HotelId", "HotelName", room.HotelId);
            return PartialView("_Edit", room);
        }

        // POST: Room/Edit/5
        [HttpPost]
        [RoleAuthorize(2)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Room room)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var currentUser = _context.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "Unable to retrieve user. Please ensure you're logged in.";
                return RedirectToAction("Index", "Home");
            }
            // Verify that the room's hotel belongs to the current user
            var hotel = _context.Hotels.FirstOrDefault(h => h.HotelId == room.HotelId);
            if (hotel == null || hotel.UserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "You are not the owner of this room's hotel.";
                return RedirectToAction("Dashboard", "Owner");
            }
            if (ModelState.IsValid)
            {
                // Process file upload if a new file is provided
                if (room.RoomImageFile != null && room.RoomImageFile.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(room.RoomImageFile.FileName);
                    var uploadDir = "~/Content/images/Rooms";
                    var imagePath = Path.Combine(Server.MapPath(uploadDir), fileName);

                    room.RoomImageFile.SaveAs(imagePath);
                    room.RoomImage = Path.Combine(uploadDir, fileName);
                }

                _context.Entry(room).State = EntityState.Modified;
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Room #{room.RoomNumber} updated successfully.";
                return RedirectToAction("Dashboard", "Owner");
            }
            ViewBag.HotelId = new SelectList(_context.Hotels.Where(h => h.UserId == currentUser.Id), "HotelId", "HotelName", room.HotelId);
            return PartialView("_Edit", room);
        }

        // GET: Room/Delete/5 - Returns PartialView for modal
        [RoleAuthorize(2)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Your request is invalid.";
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var currentUser = _context.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "Unable to retrieve user. Please ensure you're logged in.";
                return RedirectToAction("Index", "Home");
            }
            Room room = _context.Rooms.Find(id);
            if (room == null)
            {
                TempData["ErrorMessage"] = "Room not found.";
                return HttpNotFound();
            }
            // Verify that the room's hotel belongs to the current user
            var hotel = _context.Hotels.FirstOrDefault(h => h.HotelId == room.HotelId);
            if (hotel == null || hotel.UserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "You are not the owner of this room's hotel.";
                return RedirectToAction("Dashboard", "Owner");
            }
            return PartialView("_Delete", room);
        }

        // POST: Room/Delete/5
        [HttpPost, ActionName("Delete")]
        [RoleAuthorize(2)]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var currentUser = _context.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "Unable to retrieve user. Please ensure you're logged in.";
                return RedirectToAction("Index", "Home");
            }
            Room room = _context.Rooms.Find(id);
            if (room == null)
            {
                TempData["ErrorMessage"] = "Room not found.";
                return HttpNotFound();
            }
            // Verify that the room's hotel belongs to the current user
            var hotel = _context.Hotels.FirstOrDefault(h => h.HotelId == room.HotelId);
            if (hotel == null || hotel.UserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "You are not the owner of this room's hotel.";
                return RedirectToAction("Dashboard", "Owner");
            }

            // Optional: Remove associated image from server if it exists
            if (!string.IsNullOrEmpty(room.RoomImage))
            {
                string imagePath = Server.MapPath(room.RoomImage);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Rooms.Remove(room);
            _context.SaveChanges();
            TempData["SuccessMessage"] = $"Room #{room.RoomNumber} has been deleted successfully.";
            return RedirectToAction("Dashboard", "Owner");
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
