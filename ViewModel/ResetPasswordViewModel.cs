using System.ComponentModel.DataAnnotations;

namespace Learn_Auth.ViewModel
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } 

        [Required, DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}