using System.ComponentModel.DataAnnotations;

namespace Learn_Auth.ViewModel
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}