using Learn_Auth.Models;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Learn_Auth.Attributes;
using Learn_Auth.Controllers;
using QRCoder;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace Learn_Auth.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AuthDbContext _context = new AuthDbContext();

        // GET: Payment
        // Lists all payments in descending order of PaymentDate
        [RoleAuthorize(1)]
        public ActionResult Index()
        {
            var payments = _context.Payments
                .OrderByDescending(p => p.PaymentDate)
                .ToList();
            return View(payments);
        }

        // GET: Payment/Details/5
        [RoleAuthorize(1,2,3)]
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Invalid payment id.";
                return RedirectToAction("Index");
            }

            var payment = _context.Payments.Find(id.Value);
            if (payment == null)
            {
                TempData["ErrorMessage"] = "Payment not found.";
                return RedirectToAction("Index");
            }

            return View(payment);
        }

        // GET: Payment/MakePayment/bookingId
        public ActionResult MakePayment(int? bookingId)
        {
            if (!bookingId.HasValue)
            {
                TempData["ErrorMessage"] = "Booking ID is required to process payment.";
                return RedirectToAction("Index", "Booking");
            }

            // Fetch the payment details
            var payment = _context.Payments
                .FirstOrDefault(p => p.BookingID == bookingId.Value && p.PaymentStatus == "Pending");

            if (payment == null)
            {
                TempData["ErrorMessage"] = "No pending payment found for this booking.";
                return RedirectToAction("Index", "Booking");
            }

            // Generate UPI QR Code
            string upiId = "jjpatel1890@oksbi"; // Replace with actual UPI ID
            string amount = payment.PaymentAmount.ToString("0.00");
            string note = $"Booking a {payment.Booking.Room.RoomType}-Room at {payment.Booking.Room.Hotel.HotelName}";

            byte[] qrCodeBytes = GenerateUPIQRCode(upiId, amount, note);
            if (qrCodeBytes == null)
            {
                TempData["ErrorMessage"] = "Failed to generate QR Code.";
                return RedirectToAction("UserBookings", "Booking");
            }

            ViewBag.QRCode = $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
            ViewBag.Payment = payment; // Send payment details to the view

            return View(payment);
        }


        // POST: Payment/MakePayment
        // Processes the actual payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MakePayment(Payment payment)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid payment data.";
                return View(payment);
            }

            // Get the existing payment entry
            var existingPayment = _context.Payments
                .FirstOrDefault(p => p.PaymentID == payment.PaymentID);

            if (existingPayment == null)
            {
                TempData["ErrorMessage"] = "Payment not found.";
                return RedirectToAction("UserBookings", "Booking");
            }

            // Update the payment details
            existingPayment.PaymentDate = DateTime.Now;
            existingPayment.PaymentMethod = payment.PaymentMethod ?? "Cash"; // Default to cash if not specified
            existingPayment.PaymentStatus = "Completed";  // Mark as completed

            var booking = _context.Bookings.FirstOrDefault(b => b.BookingID == existingPayment.BookingID);
            if(booking != null)
            {
                booking.BookingStatus = "Booked";
                _context.Entry(booking).State = System.Data.Entity.EntityState.Modified;
                _context.SaveChanges();
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Payment updated successfully.";
            return RedirectToAction("GenerateInvoice", "Invoice", new { bookingId = payment.BookingID });
        }

        // GET: Payment/PaymentStatus/5
        // Renders a view to update payment status
        public ActionResult PaymentStatus(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Invalid payment id.";
                return RedirectToAction("Index");
            }

            var payment = _context.Payments.Find(id.Value);
            if (payment == null)
            {
                TempData["ErrorMessage"] = "Payment not found.";
                return RedirectToAction("Index");
            }

            return View(payment);
        }

        // POST: Payment/PaymentStatus/5
        // Updates the payment status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PaymentStatus(int id, string newStatus)
        {
            var payment = _context.Payments.Find(id);
            if (payment == null)
            {
                TempData["ErrorMessage"] = "Payment not found.";
                return RedirectToAction("Index");
            }

            payment.PaymentStatus = newStatus;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Payment status updated successfully.";
            return RedirectToAction("Details", new { id = payment.PaymentID });
        }

        private byte[] GenerateUPIQRCode(string upiId, string amount, string note)
        {
            string upiUri = $"upi://pay?pa={upiId}&pn=Hotel Booking&am={amount}&cu=INR&tn={note}";

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(upiUri, QRCodeGenerator.ECCLevel.Q);
                using (QRCode qrCode = new QRCode(qrCodeData))
                {
                    using (Bitmap qrCodeImage = qrCode.GetGraphic(20))
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            qrCodeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                            return stream.ToArray();
                        }
                    }
                }
            }
        }

        //private byte[] GenerateUPIQRCode(string upiId, string amount, string note)
        //{
        //    string upiUri = $"upi://pay?pa={upiId}&pn=Hotel Booking&am={amount}&cu=INR&tn={note}";

        //    using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        //    {
        //        QRCodeData qrCodeData = qrGenerator.CreateQrCode(upiUri, QRCodeGenerator.ECCLevel.Q);
        //        using (QRCode qrCode = new QRCode(qrCodeData))
        //        {
        //            using (Bitmap qrCodeImage = qrCode.GetGraphic(20)) // Generate QR code with 20x scale
        //            {
        //                // Load the company logo
        //                string logoPath = HttpContext.Server.MapPath("~/Content/logo.png"); // Path to logo
        //                using (Bitmap logo = new Bitmap(logoPath))
        //                {
        //                    int logoSize = qrCodeImage.Width / 5; // Set logo size to 20% of the QR code
        //                    using (Bitmap resizedLogo = new Bitmap(logo, new Size(logoSize, logoSize)))
        //                    {
        //                        int centerX = (qrCodeImage.Width - logoSize) / 2;
        //                        int centerY = (qrCodeImage.Height - logoSize) / 2;

        //                        // Place logo in the center of the QR code
        //                        using (Graphics graphics = Graphics.FromImage(qrCodeImage))
        //                        {
        //                            graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
        //                            graphics.DrawImage(resizedLogo, new Point(centerX, centerY));
        //                        }

        //                        // Save the final QR code with logo
        //                        using (MemoryStream stream = new MemoryStream())
        //                        {
        //                            qrCodeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        //                            return stream.ToArray(); // Return as byte[]
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

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
