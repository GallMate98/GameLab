using System.ComponentModel.DataAnnotations;

namespace GameLab.Models
{
    public class ResetPassword
    {
        [Required(ErrorMessage = "Password is required")]
        [MaxLength(100, ErrorMessage = "Password must be at most 100 characters long")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}
