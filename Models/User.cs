using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learn_Auth.Models
{
    public class User
    {
        // For Login and Register
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; }     
        
        [Required, EmailAddress]
        public string Email { get; set; }     
        
        [Required]
        public string PasswordHash { get; set; }

        [NotMapped]
        public string Password { get; set; }

        public int RoleId { get; set; }
        public virtual Role Role { get; set; }

        // For MyProfile.
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Phone]
        public string ContactNo { get; set; }

        public string ProfilePicture { get; set; }
        public string Address { get; set; }

        // For Otp Verification
        public string OTP { get; set; }
        public DateTime? OTPExpiry { get; set; }
        public bool IsVerified { get; set; } = false;

        // For Password reset.
        public string ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
        public virtual ICollection<Hotel> Hotels { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
    }
}