using Learn_Auth.Models;
using Learn_Auth.ViewModel;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using SelectPdf;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Learn_Auth.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly AuthDbContext _context = new AuthDbContext();

        // GET: Invoice
        // Lists all invoices.
        public ActionResult Index()
        {
            var invoices = _context.Invoices
                .OrderByDescending(i => i.InvoiceDate)
                .ToList();
            return View(invoices);
        }

        // GET: Invoice/Details/5
        // Shows invoice details.
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Invalid invoice id.";
                return RedirectToAction("Index");
            }

            var invoice = _context.Invoices.Find(id);
            if (invoice == null)
            {
                TempData["ErrorMessage"] = "Invoice not found.";
                return RedirectToAction("Index");
            }

            return View(invoice);
        }

        // GET: Invoice/GenerateInvoice/bookingId
        // Generates an invoice for a given booking.
        public ActionResult GenerateInvoice(int? bookingId)
        {
            if (bookingId == null)
            {
                TempData["ErrorMessage"] = "Booking id is required.";
                return RedirectToAction("UserBookings", "Booking");
            }

            var booking = _context.Bookings
                .Include(b => b.User)              // Include User details
                .Include(b => b.Room.Hotel)        // Include Room and Hotel details
                .FirstOrDefault(b => b.BookingID == bookingId.Value);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("UserBookings", "Booking");
            }

            // Calculate number of nights
            int numberOfNights = (booking.CheckOutDate - booking.CheckInDate).Days;
            if (numberOfNights <= 0)
            {
                numberOfNights = 1; // Default to 1 night if dates are invalid or the same
            }

            decimal pricePerNight = booking.Room.Price;
            decimal totalAmount = numberOfNights * pricePerNight;

            // Calculate Discounts and Taxes
            decimal discount = 0.00m;  // Apply discount logic if needed
            decimal tax = 0.00m;       // Apply tax logic if needed

            decimal finalAmount = totalAmount + tax - discount; // Final amount calculation

            // Create a new invoice
            var invoice = new Invoice
            {
                BookingID = bookingId.Value,
                InvoiceDate = DateTime.Now,
                CustomerName = booking.User.FirstName + " " + booking.User.LastName,
                CustomerAddress = booking.User.Address,
                HotelName = booking.Room.Hotel.HotelName,
                RoomNo = booking.Room.RoomNumber,
                Price = pricePerNight,
                TotalAmount = totalAmount,
                Tax = tax,
                Discounts = discount,
                FinalAmount = finalAmount
            };

            _context.Invoices.Add(invoice);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Invoice generated successfully.";
            return RedirectToAction("Details", new { id = invoice.InvoiceID });
        }


        // GET: Invoice/DownloadInvoice/5
        // Reads the invoice template, replaces placeholders with invoice data, and returns the PDF.

        public ActionResult DownloadInvoice(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Invalid invoice ID.";
                return RedirectToAction("UserBooking", "Booking");
            }

            // Fetch the invoice with related data
            var invoice = _context.Invoices
                .Include(i => i.Booking)
                .Include(b => b.Booking.User)
                .Include(i => i.Booking.Room)
                .Include(r => r.Booking.Room.Hotel)
                .FirstOrDefault(i => i.InvoiceID == id);

            // Handle null or invalid invoice case
            if (invoice == null || invoice.Booking == null || invoice.Booking.Room == null || invoice.Booking.User == null)
            {
                TempData["ErrorMessage"] = "Invoice not found or data is incomplete.";
                return RedirectToAction("UserBooking", "Booking");
            }

            // Prepare invoice model for InvoiceTemplate
            var invoiceModel = new InvoiceTemplateViewModel
            {
                InvoiceID = invoice.InvoiceID,
                HotelName = invoice.Booking.Room.Hotel.HotelName,
                RoomNumber = invoice.Booking.Room.RoomNumber,
                Price = invoice.Price,
                TotalAmount = invoice.TotalAmount,
                InvoiceDate = invoice.InvoiceDate,
                BookingID = invoice.Booking.BookingID,
                CheckInDate = invoice.Booking.CheckInDate,
                CheckOutDate = invoice.Booking.CheckOutDate,
                BookingStatus = invoice.Booking.BookingStatus,
                CustomerName = $"{invoice.Booking.User.FirstName} {invoice.Booking.User.LastName}",
                CustomerEmail = invoice.Booking.User.Email,
                CustomerPhone = invoice.Booking.User.ContactNo,
                CustomerAddress = invoice.Booking.User.Address
            };

            // Render the invoice template to HTML
            string html = RenderRazorViewToString("InvoiceTemplate", invoiceModel);

            // Initialize HTML to PDF converter
            HtmlToPdf converter = new HtmlToPdf();

            try
            {
                // Generate PDF from HTML
                SelectPdf.PdfDocument doc = converter.ConvertHtmlString(html);

                // Save PDF to memory stream
                byte[] pdf = doc.Save();
                doc.Close();

                // Return PDF as a downloadable file
                return File(pdf, "application/pdf", $"Invoice_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating invoice: {ex.Message}";
                return RedirectToAction("UserBooking", "Booking");
            }
        }


        // Helper method to render Razor view to string
        private string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindView(ControllerContext, viewName, null);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                return sw.GetStringBuilder().ToString();
            }
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
