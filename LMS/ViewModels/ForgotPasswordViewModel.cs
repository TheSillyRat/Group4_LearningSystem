using System.ComponentModel.DataAnnotations;

namespace LMS.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Please enter your Email address.")]
        [EmailAddress(ErrorMessage = "Invalid Email address format.")]
        public string Email { get; set; } = string.Empty;

        public string? Code { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "New Password must be at least 6 characters long.")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New Password confirmation does not match.")]
        public string? ConfirmNewPassword { get; set; }

        public int Step { get; set; } = 1;
    }
}
