using Learn_Auth.Models;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Learn_Auth.Attributes;
using System.Data.Entity;
using Microsoft.Extensions.Logging;

namespace Learn_Auth.Controllers
{
    public class HotelController : Controller
    {
        private readonly AuthDbContext _context = new AuthDbContext();
        private readonly ILogger<HomeController> _logger;

        // GET: Hotel
        public ActionResult Index(string search, DateTime? checkIn, DateTime? checkOut)
        {
            var hotelsQuery = _context.Hotels.Include(h => h.Rooms).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                hotelsQuery = hotelsQuery.Where(h => h.Country.Contains(search) || h.City.Contains(search) || h.State.Contains(search) || h.Address.Contains(search));
            }

            var hotels = hotelsQuery.ToList();

            DateTime desiredCheckIn = checkIn ?? DateTime.Today;
            DateTime desiredCheckOut = checkOut ?? DateTime.Today.AddDays(1);

            hotels = hotels.Where(h => h.Rooms.Any(r => IsRoomAvailable(r.RoomId, desiredCheckIn, desiredCheckOut))).ToList();

            TempData["Search"] = search; 
            TempData["CheckIn"] = desiredCheckIn.ToString("yyyy-MM-dd");
            TempData["CheckOut"] = desiredCheckOut.ToString("yyyy-MM-dd");

            return View(hotels);
        }

        // GET: Hotel/Details/5
        public ActionResult Details(int? id,DateTime? checkIn, DateTime? checkOut)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Your request is invalid.";
                return RedirectToAction("Index", "Hotel");
            }

            var hotel = _context.Hotels.Include(r => r.Rooms).FirstOrDefault(h => h.HotelId == id);
            if (hotel == null)
            {
                TempData["ErrorMessage"] = "Hotel with this Id not found.";
                return RedirectToAction("Index", "Hotel");
            }

            // If the user hasn't provided dates, you can set defaults or handle it differently
            DateTime desiredCheckIn = checkIn ?? DateTime.Today;
            DateTime desiredCheckOut = checkOut ?? DateTime.Today.AddDays(1);

            // Evaluate each room’s availability for the desired date range
            foreach (var room in hotel.Rooms)
            {
                // Instead of a simple property, call the helper method:
                bool available = IsRoomAvailable(room.RoomId, desiredCheckIn, desiredCheckOut);
                room.IsAvailable = available;
            }

            // Example: populating other dropdown data
            var hotels = _context.Hotels.ToList();
            ViewBag.AllHotels = hotels;
            ViewBag.Location = hotels.Select(h => h.City).Distinct().ToList();
            ViewBag.Countries = hotels.Select(h => h.Country).Distinct().ToList();
            ViewBag.CitiesByCountry = hotels
                .GroupBy(h => h.Country)
                .ToDictionary(g => g.Key, g => g.Select(h => h.City).Distinct().ToList());

            // If AJAX request, return partial view
            if (Request.IsAjaxRequest())
            {
                return PartialView("_HotelDetailsPartial", hotel);
            }

            return View(hotel);
        }


        // GET: Hotel/Create
        [RoleAuthorize(2)]
        public ActionResult Create()
        {
            // Check if the user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            // Attempt to get the email from Session
            string email = Session["Email"] as string;

            // If session doesn't have it, try the custom cookie
            if (string.IsNullOrEmpty(email) && Request.Cookies["UserProfile"] != null)
            {
                email = Request.Cookies["UserProfile"]["Email"];
            }

            // Fallback to User.Identity.Name (set by FormsAuthentication) if needed
            if (string.IsNullOrEmpty(email))
            {
                email = User.Identity.Name;
            }

            // Retrieve user from database
            var currentUser = _context.Users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "Unable to retrieve user. Please ensure you're logged in.";
                return RedirectToAction("Login", "Account");
            }

            // Check if the user is verified
            if (!currentUser.IsVerified)
            {
                TempData["ErrorMessage"] = "Please verify your email first.";
                return RedirectToAction("Profile", "Account");
            }

            return View();
        }


        // POST: Hotel/Create
        [HttpPost]
        [RoleAuthorize(2)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Hotel model, HttpPostedFileBase HotelImage)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var currentUser = _context.Users.FirstOrDefault(u => u.Email.Equals(User.Identity.Name));
                    if (currentUser == null)
                    {
                        TempData["ErrorMessage"] = "Unable to retrieve user. Please ensure you're logged in.";
                        return RedirectToAction("Login", "Account");
                    }
                    model.UserId = currentUser.Id;

                    //checks for duplicate hotels
                    var duplicateHotel = _context.Hotels.FirstOrDefault(h =>
                        (h.HotelName.ToLower() == model.HotelName.ToLower() && h.UserId == currentUser.Id) ||
                        (h.HotelName.ToLower() == model.HotelName.ToLower() &&
                         h.Address.ToLower() == model.Address.ToLower() &&
                         h.UserId != currentUser.Id));

                    if (duplicateHotel != null)
                    {
                        if (duplicateHotel.UserId == currentUser.Id)
                        {
                            TempData["ErrorMessage"] = "You have already registered a hotel with this name.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "This hotel is already registered by another user.";
                        }
                        return View(model);
                    }

                    // Check if an image file is uploaded
                    if (HotelImage != null && HotelImage.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(HotelImage.FileName);
                        string uniqueFileName = Guid.NewGuid() + "_" + fileName;
                        string uploadDir = "~/HotelImages";
                        string uploadPath = Server.MapPath(uploadDir);

                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        string filePath = Path.Combine(uploadPath, uniqueFileName);
                        HotelImage.SaveAs(filePath);

                        // Save relative path in the model
                        model.HotelImages = Path.Combine(uploadDir, uniqueFileName);
                    }

                    _context.Hotels.Add(model);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Hotel created successfully.";
                    return RedirectToAction("Details", "Hotel", new { id = model.HotelId});
                }

                TempData["ErrorMessage"] = "Invalid data provided.";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the hotel.");
                TempData["ErrorMessage"] = "An error occurred while creating the hotel.";
                return View(model);
            }
        }

        // GET: Hotel/Edit/5
        [RoleAuthorize(2)]
        public ActionResult Edit(int? id,string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (id == null)
            {
                TempData["ErrorMessage"] = "Your request is invalid.";
                return RedirectToAction("Index", "Hotel");
            }

            var CurrentUser = _context.Users.FirstOrDefault(u => u.Email.Equals(User.Identity.Name));
            if(CurrentUser == null)
            {
                TempData["ErrorMessage"] = "Unable to retrieve user. Please ensure you're logged in.";
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Login", "Account");
            }
            var hotel = _context.Hotels.FirstOrDefault(h => h.HotelId == id);
            if(hotel == null)
            {
                TempData["ErrorMessage"] = "The Hotel you are looking for was not found.";
                return RedirectToAction("Dashboard", "Owner");
            }
            if(hotel.UserId != CurrentUser.Id)
            {
                TempData["ErrorMessage"] = "You are not the owner of this hotel.";
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Dashboard", "Owner");
            }
            return View(hotel);
        }

        // POST: Hotel/Edit/5
        [HttpPost]
        [RoleAuthorize(2)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Hotel model, HttpPostedFileBase HotelImage, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var hotelInDb = _context.Hotels.FirstOrDefault(h => h.HotelId == model.HotelId);
                    if (hotelInDb == null)
                    {
                        TempData["ErrorMessage"] = "The Hotel you are looking for not found.";
                        if (!string.IsNullOrEmpty(returnUrl))
                            return Redirect(returnUrl);

                        return RedirectToAction("Dashboard", "Owner");
                    }

                    // Update properties
                    hotelInDb.HotelName = model.HotelName;
                    hotelInDb.Address = model.Address;
                    hotelInDb.City = model.City;
                    hotelInDb.State = model.State;
                    hotelInDb.Country = model.Country;
                    hotelInDb.ZipCode = model.ZipCode;
                    hotelInDb.Description = model.Description;
                    hotelInDb.ContactInfo = model.ContactInfo;

                    // Handle Image Upload
                    if (HotelImage != null && HotelImage.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(HotelImage.FileName);
                        string uniqueFileName = Guid.NewGuid() + "_" + fileName;
                        string uploadDir = "~/HotelImages";
                        string uploadPath = Server.MapPath(uploadDir);

                        string newFilePath = Path.Combine(uploadPath, uniqueFileName);

                        // Delete the old image if it exists
                        if (!string.IsNullOrEmpty(hotelInDb.HotelImages))
                        {
                            string oldFilePath = Server.MapPath(hotelInDb.HotelImages);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        HotelImage.SaveAs(newFilePath);
                        hotelInDb.HotelImages = Path.Combine(uploadDir, uniqueFileName);

                    }

                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Hotel updated successfully.";
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Dashboard", "Owner");

                }
                TempData["ErrorMessage"] = "Invalid data provided";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the hotel.");
                TempData["ErrorMessage"] = "An error occurred while updating the hotel.";
                return RedirectToAction("Dashboard", "Owner");
            }
        }

        // GET: Hotel/Delete/5
        [RoleAuthorize(2)]
        public ActionResult Delete(int? id, string returnUrl)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Your request is invalid.";
                return RedirectToAction("Index", "Hotel");
            }

            var CurrentUser = _context.Users.FirstOrDefault(u => u.Email.Equals(User.Identity.Name));
            if (CurrentUser == null)
            {
                TempData["ErrorMessage"] = "Unable to retrieve user. Please ensure you're logged in.";
                return RedirectToAction("Index", "Hotel");
            }

            var hotel = _context.Hotels.FirstOrDefault(h => h.HotelId == id);
            if (hotel == null)
            {
                TempData["ErrorMessage"] = "Hotel You are looking for does not exist.";
                return RedirectToAction("Index", "Hotel");
            }
            if(hotel.UserId != CurrentUser.Id)
            {
                TempData["ErrorMessage"] = "You are not the owner of this hotel.";
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Hotel");
            }
            return View(hotel);
        }

        // POST: Hotel/Delete/5
        [HttpPost, ActionName("Delete")]
        [RoleAuthorize(2)]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id,string returnUrl)
        {
            try
            {
                var CurrentUser = _context.Users.FirstOrDefault(u => u.Email.Equals(User.Identity.Name));
                if (CurrentUser == null)
                {
                    TempData["ErrorMessage"] = "Unable to retrieve user. Please ensure you're logged in.";
                    return RedirectToAction("Index", "Hotel");
                }

                var hotel = _context.Hotels.FirstOrDefault(h => h.HotelId == id);
                if (hotel == null)
                {
                    TempData["ErrorMessage"] = "Hotel You are looking for does not exist.";
                    return RedirectToAction("Index", "Hotel");
                }

                if (hotel.UserId != CurrentUser.Id)
                {
                    TempData["ErrorMessage"] = "You are not the owner of this hotel.";
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Hotel");
                }
                _context.Hotels.Remove(hotel);  
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Hotel deleted successfully.";
                return RedirectToAction("Dashboard", "Owner");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the hotel.");
                TempData["ErrorMessage"] = "An error occurred while deleting the hotel.";
                return RedirectToAction("Dashboard", "Owner");
            }
        }

        public bool IsRoomAvailable(int roomId, DateTime desiredCheckIn, DateTime desiredCheckOut)
        {
               return !_context.Bookings.Any(b =>
              b.RoomID == roomId &&
              ((b.CheckInDate <= desiredCheckOut && b.CheckOutDate >= desiredCheckIn)));
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
