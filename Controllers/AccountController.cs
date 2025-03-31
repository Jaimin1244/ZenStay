    using Learn_Auth.Models;
using Learn_Auth.ViewModel;
using Learn_Auth.Attributes;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using System.IO;
using System.Security.Cryptography;
using System.Web;

namespace Learn_Auth.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthDbContext _context = new AuthDbContext();

        //GET : Register User. " public ActionResult Register() => View();"
        public ActionResult Register()
        {
            return View();
        }

        //POST : Register User.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Register(RegisterViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if(_context.Users.Any(u => u.Email == model.Email || u.UserName == model.UserName))
                    {
                        return Json(new { success = false, message = "User with this email or username already exists." });
                    }

                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
                    var roleId = 3;
                    if(model.isOwner == true)
                    {
                        roleId = 2;
                    }

                    var user = new User
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        PasswordHash = hashedPassword,
                        RoleId = roleId,
                        IsVerified = false
                    };

                    _context.Users.Add(user);
                    _context.SaveChanges();

                    return Json(new { success = true, message = "Registration successful! Please verify your email from your profile page." });
                }

                return Json(new { success = false, message = "Please correct the errors in the form." });
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return Json(new { success = false, message = "An unexpected error occurred. Please try again." });
            }
        }
        //GET : Login User
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //POST : Login User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Login(LoginViewModel model, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                    if (user == null)
                    {
                        return Json(new { success = false, message = "User not found." });
                    }

                    // Verify password
                    bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
                    if (!isPasswordCorrect)
                    {
                        return Json(new { success = false, message = "Incorrect password." });
                    }

                    // Set Authentication Cookie
                    FormsAuthentication.SetAuthCookie(user.Email, model.RememberMe);

                    Session["UserId"] = user.Id;
                    Session["UserName"] = user.UserName;
                    Session["Email"] = user.Email;
                    Session["RoleId"] = user.RoleId;
                    Session["ProfileImage"] = user.ProfilePicture;

                    if (model.RememberMe)
                    {
                        HttpCookie userCookie = new HttpCookie("UserProfile")
                        {
                            ["UserName"] = user.UserName,
                            ["ProfileImage"] = user.ProfilePicture,
                            ["Email"] = user.Email,
                            Expires = DateTime.Now.AddDays(30)
                        };
                        Response.Cookies.Add(userCookie);
                    }

                    string redirectUrl = !string.IsNullOrEmpty(returnUrl) ? returnUrl : Url.Action("Index", "Home");

                        return Json(new { success = true, message = "Login successful! Redirecting...",
                            redirectUrl = redirectUrl,
                            user = new
                        {
                            id = user.Id,
                            userName = user.UserName,
                            email = user.Email,
                            profilePicture = user.ProfilePicture
                        }
                    });
                }

                return Json(new { success = false, message = "Please fill in all required fields." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Json(new { success = false, message = "An unexpected error occurred. Please try again." });
            }
        }

        //GET: Logout user.
        public ActionResult Logout()
        {
            // Clear authentication
            FormsAuthentication.SignOut();

            // Clear all session variables
            Session.Clear();
            Session.Abandon();

            // Remove authentication cookies        
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                authCookie.Expires = DateTime.Now.AddDays(-1); // Expire the cookie
                Response.Cookies.Add(authCookie);
            }

            // Remove session cookies
            HttpCookie sessionCookie = Request.Cookies["ASP.NET_SessionId"];
            if (sessionCookie != null)
            {
                sessionCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(sessionCookie);
            }

            return RedirectToAction("Index", "Home");
        }


        public ActionResult AccessDenied()
        {
            return View();
        }

        //GET : Profile.
        public ActionResult Profile()
        {
            try
            {
                var email = User.Identity.Name;
                var user = _context.Users.FirstOrDefault(u => u.Email == email);

                if(user == null)
                {
                    return RedirectToAction("Login");
                }

                //var role = _context.Roles.FirstOrDefault(r => r.Id == user.RoleId)?.Name ?? "User";

                var model = new ProfileViewModel
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ContactNo = user.ContactNo,
                    ProfilePictureUrl = !string.IsNullOrEmpty(user.ProfilePicture) ? Url.Content(user.ProfilePicture) : "",
                    Address = user.Address,
                    IsVerified = user.IsVerified
                };

                return View(model);
            }
            catch
            {
                TempData["Error"] = "Server error..!";
                return View();
            }
        }

        // GET: Account/EditProfile
        [HttpGet]
        public ActionResult EditProfile()
        {
            var email = User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new ProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ContactNo = user.ContactNo,
                ProfilePictureUrl = user.ProfilePicture,
                Address = user.Address
            };

            return View(model);
        }

        //POST : EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(ProfileViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                    if (user == null)
                    {
                        return Json(new { success = false, message = "User not found." });
                    }

                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.UserName = model.UserName;
                    user.ContactNo = model.ContactNo;
                    user.Address = model.Address;

                    if(model.ProfilePicture != null && model.ProfilePicture.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(model.ProfilePicture.FileName);
                        string uploadDir = "~/ProfilePictures";
                        string imagePath = Path.Combine(Server.MapPath(uploadDir), fileName);
                        model.ProfilePicture.SaveAs(imagePath);

                        user.ProfilePicture = Path.Combine(uploadDir, fileName);
                    }

                    _context.SaveChanges();
                    TempData["Success"] = "Profile updated successfully!";
                    return RedirectToAction("Profile");

                }
                return View(model);
            }
            catch
            {
                TempData["Error"] = "Server error..!";
                return View();
            }
        }

        //GET : Forgot Password.
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //POST : Forgot Password.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                    if (user == null)
                    {
                        return Json(new { success = false, message = "Email not found!" });
                    }


                    // Generate unique token.
                    string token = Guid.NewGuid().ToString();
                    user.ResetToken = token;
                    user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
                    _context.SaveChanges();

                    //Generate reset Url
                    var resetLink = Url.Action("ResetPassword", "Account", new { Token = token }, Request.Url.Scheme);

                    var tempPath = Server.MapPath("~/Views/EmailTemplates/ForgotPasswordTemplate.html");
                    string emailBody = System.IO.File.ReadAllText(tempPath);

                    emailBody = emailBody.Replace("{{ResetLink}}", resetLink);

                    string subject = "Password Reset Request";
                    EmailService.SendEmail(user.Email, subject, emailBody);

                    return Json(new { success = true, message = "Password reset link has been sent!" });
                }
                return Json(new { success = false, message = "Invalid request!" });


            }
            catch
            {
                TempData["Error"] = "Server error..!";
                return View();
            }
        }

        //GET : ResetPassword
        public ActionResult ResetPassword(string token)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.UtcNow);
                if (user == null)
                {
                    return Content("Invalid or expired token.");
                }

                var model = new ResetPasswordViewModel { Token = token };
                return View(model);

            }
            catch
            {
                TempData["Error"] = "Server error..!";
                return View();
            }
        }

        // POST : ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = _context.Users.FirstOrDefault(u => u.ResetToken == model.Token && u.ResetTokenExpiry > DateTime.UtcNow);
                    if (user == null)
                    {
                        return Json(new { success = false, message = "Invalid or expired token." });
                    }

                    //Update password
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                    user.ResetToken = null;
                    user.ResetTokenExpiry = null;
                    _context.SaveChanges();

                    return Json(new { success = true, message = "Password has been reset! You can now login." });

                }

                return Json(new { success = false, message = "Please correct the errors in the form." });
            }
            catch
            {
                TempData["Error"] = "Server error..!";
                return View();
            }
        }

        //Generate OTP.
        public string GenerateOTP()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[4];
                rng.GetBytes(data);
                int seed = BitConverter.ToInt32(data, 0);
                int otpValue = new Random(seed).Next(100000, 1000000);

                return otpValue.ToString();
            }
        }

        // GET: SendVerificationEmail
        public ActionResult SendVerificationEmail()
        {
            var email = User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return RedirectToAction("Login");
            }


            // Generate OTP and update the user
            string otp = GenerateOTP();
            user.OTP = otp;
            user.OTPExpiry = DateTime.UtcNow.AddMinutes(2.5);
            _context.SaveChanges();

            var templatePath = Server.MapPath("~/Views/EmailTemplates/VerifyEmail.html");
            string emailBody = System.IO.File.ReadAllText(templatePath);
            emailBody = emailBody.Replace("{{OTP}}", otp).Replace("{{UserName}}", user.UserName);

            string subject = "Email Verification - ZenStay";
            EmailService.SendEmail(user.Email, subject, emailBody);

            TempData["Message"] = "A verification email has been sent to your email address.";
            TempData["UserEmail"] = user.Email;
            return RedirectToAction("VerifyEmail");
        }

        //GET : Otp Verification.
        public ActionResult VerifyEmail()
        {
            var email = User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new VerifyOTPViewModel
            {
                Email = user.Email,
                OTPExpiry = user.OTPExpiry
            };
            return View(model);
        }
        // POST: OTP Verification.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult VerifyEmail(VerifyOTPViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid data provided." });
                }

                // Optional: Use a case-insensitive comparison if needed
                var user = _context.Users.FirstOrDefault(u => u.Email.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }
                if (user.IsVerified)
                {
                    return Json(new { success = false, message = "Email is already verified. You can log in." });
                }

                // Ensure OTP is provided
                if (string.IsNullOrWhiteSpace(model.OTP))
                {
                    return Json(new { success = false, message = "OTP is required." });
                }

                if (user.OTP == model.OTP && user.OTPExpiry > DateTime.UtcNow)
                {
                    user.IsVerified = true;
                    user.OTP = null;
                    user.OTPExpiry = null;
                    _context.SaveChanges();

                    return Json(new { success = true, message = "Email verified successfully! You can now log in." });
                }
                return Json(new { success = false, message = "Invalid or expired OTP." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Json(new { success = false, message = "Server error. Please try again." });
            }
        }
    }
}