using System.ComponentModel.DataAnnotations;

namespace GameLab.Models
{
    public class UpdateUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [MaxLength(50)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [MaxLength(256)]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        public string PhoneNumber { get; set; } 
    }
}
