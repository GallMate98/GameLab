using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GameLab.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using GameLab.Data;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Win32;

namespace GameLab.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IEmailService _emailService;

        public UserController(DataContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;   
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistration register)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid registration data");
            }

            if (await _context.Users.AnyAsync(u => u.Email == register.Email))
            {
                return Conflict("User with this email already exists");
            }

            if (await _context.Users.AnyAsync(u => u.Username == register.Username))
            {
                return Conflict("User with this Username already exists");
            }

            CreatePassswordHash (register.Password, out byte[]
                paswordHash, out byte[] passwordSalt);


            var newUser = new User
            {
                FirstName = register.FirstName,
                LastName = register.LastName,
                Email = register.Email,
                Username = register.Username,
                PasswordHash = paswordHash,
                PasswordSalt = passwordSalt,
                VerificationToken = CreateRandomToken(),
              
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            EmailDto emailDto = new EmailDto
            {
                To = newUser.Email,
                Subject = "New Account",
                Body = "Your Verification Token is: <a href=\"https://localhost:7267/api/User/Verify?token=" + newUser.VerificationToken + "\">Click here</a>" ,
            };

            _emailService.SendMail(emailDto);


            return Ok("Registration successful");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLogin login)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid login data");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == login.Username);

            if (user == null)
            {
                return BadRequest("User not found");
            }

            if (!VerifyPassswordHash(login.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Password is wrong!");
            }

            if (user.VerifiedAt==null)
            {
                return BadRequest("Not verified!");
            }

            return Ok($"Welcome back, {user.Username}! :)");
        }


        [HttpGet("verify")]
        public async Task<IActionResult> Verify(string token)
        {

            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);

            if (user == null)
            {
                return BadRequest("Invalid token");
            }

            if(user.VerifiedAt  != null)
            {
                return BadRequest("Alredy verified");
            }

            user.VerifiedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok("User verified! :)");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return BadRequest("Invalid token");
            }

            

            user.PasswordResetToken = CreateRandomToken();
            user.ResetTokenExpries = DateTime.Now.AddDays(1);
            await _context.SaveChangesAsync();

            return Ok("You may now reset your password.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPassword resetPassword)
        {

            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == resetPassword.Token);

            if (user == null || user.ResetTokenExpries < DateTime.Now)
            {
                return BadRequest("Invalid token");
            }


            CreatePassswordHash(resetPassword.Password, out byte[]
                paswordHash, out byte[] passwordSalt);

            user.PasswordHash = paswordHash;
            user.PasswordSalt = passwordSalt;
            user.PasswordResetToken = null;
            user.ResetTokenExpries = null;

            await _context.SaveChangesAsync();

            return Ok("Password successfully reset.");
        }

        private void CreatePassswordHash (string password, out byte[]
                passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPassswordHash(string password,  byte[]
                passwordHash,  byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
               var computedHash  = hmac
               .ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
               return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }
    }
}
