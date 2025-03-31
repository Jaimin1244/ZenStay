using System.ComponentModel.DataAnnotations;

namespace Learn_Auth.ViewModel
{
    public class RegisterViewModel
    {
        [Required]
        public string UserName { get; set; }  
        
        [Required, EmailAddress]
        public string Email { get; set; }  
        
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required]
        public bool isOwner { get; set; }
    }
}