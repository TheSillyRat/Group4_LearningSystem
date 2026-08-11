using System.ComponentModel.DataAnnotations;

namespace LMS.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter your Email address.")]
        [EmailAddress(ErrorMessage = "Invalid Email address format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your Password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
