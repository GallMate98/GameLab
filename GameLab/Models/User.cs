using System.ComponentModel.DataAnnotations;

namespace GameLab.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(256)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MaxLength(100000000)] 
        public byte[] PasswordHash { get; set; } = new byte[32];

        [Required]
        [MaxLength(100000000)]
        public byte[] PasswordSalt { get; set; } = new byte[32];

        public string? VerificationToken { get; set; }

        public DateTime? VerifiedAt { get; set; }
 
        public string? PasswordResetToken { get; set; }

        public DateTime? ResetTokenExpries { get; set; }
    }
}
