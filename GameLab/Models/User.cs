using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GameLab.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string? VerificationToken { get; set; }

        public DateTime? VerifiedAt { get; set; }
 
        public string? PasswordResetToken { get; set; }

        public DateTime? ResetTokenExpries { get; set; }

        public DateTime AccountDateBan { get; set; } = DateTime.MinValue;
    }
}
