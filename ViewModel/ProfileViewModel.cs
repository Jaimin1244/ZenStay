using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Learn_Auth.ViewModel
{
    public class ProfileViewModel
    {
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Display(Name = "Email Address")]
        [EmailAddress]
        public string Email { get; set; }
        public bool IsVerified { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Contact Number")]
        [Phone]
        public string ContactNo { get; set; }

        [Display(Name = "Upload profile Picture")]
        public HttpPostedFileBase ProfilePicture { get; set; }

        // For displaying the current image.
        public string ProfilePictureUrl { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }
    }
}