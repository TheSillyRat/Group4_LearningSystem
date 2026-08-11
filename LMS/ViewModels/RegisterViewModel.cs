using System.ComponentModel.DataAnnotations;

namespace LMS.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please enter your Full Name.")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your Email address.")]
        [EmailAddress(ErrorMessage = "Invalid Email address format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your Primary Password.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your Primary Password.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password confirmation does not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
