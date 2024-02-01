using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GameLab.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using GameLab.Data;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Win32;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Data;
using GameLab.Services.Token;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Net;

namespace GameLab.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;

        public UserController(IEmailService emailService, IConfiguration configuration, UserManager<User> userManager, ITokenService tokenService, RoleManager<IdentityRole> roleManager)
        {
           
            _emailService = emailService;
            _configuration = configuration;
            _userManager = userManager;
            _tokenService = tokenService;
            _roleManager = roleManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistration register)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid registration data");
            }

            if (await _userManager.Users.AnyAsync(u => u.Email == register.Email))
            {
                return Conflict("User with this email already exists");
            }

            if (await _userManager.Users.AnyAsync(u => u.UserName == register.UserName))
            {
                return Conflict("User with this Username already exists");
            }


            var newUser = new User
            {
                FirstName = register.FirstName,
                LastName = register.LastName,
                Email = register.Email,
                UserName = register.UserName,
            };

            newUser.VerificationToken = await _userManager.GeneratePasswordResetTokenAsync(newUser);
            var result = await _userManager.CreateAsync(newUser, register.Password);

            if(!result.Succeeded)
            {
                var errorDescription = result.Errors.ToList()[0].Description;
                return BadRequest(errorDescription);
            }

           
                await _userManager.AddToRoleAsync(newUser, "User");
             
             if (!result.Succeeded)
             {
                return BadRequest("Somthing went wrong");
             }

            EmailDto emailDto = new EmailDto
            {
                To = newUser.Email,
                Subject = "New Account",
                Body = "Your Verification Token is: <a href=\"http://localhost:3000/verify?token=" + WebUtility.UrlEncode(newUser.VerificationToken) + "\">Click here</a>",
            };

            _emailService.SendMail(emailDto);

            return Ok("Registration successful! Please login."+ newUser.VerificationToken);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLogin login)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid login data");
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == login.UserName);

            if (user == null)
            {
                return BadRequest("User not found");
            }

           var checkPasword = await _userManager.CheckPasswordAsync(user, login.Password);

            if (!checkPasword)
            {
                return BadRequest("Password is wrong!");
            }

            if (user.VerifiedAt == null)
            {
                return BadRequest("Not verified!");
            }

            DateTime now = DateTime.Now;
            if (user.AccountDateBan==DateTime.MinValue || user.AccountDateBan-now<=TimeSpan.Zero)
            {
                if(user.AccountDateBan != DateTime.MinValue)
                {
                    user.AccountDateBan= DateTime.MinValue;
                    await _userManager.UpdateAsync(user);
                }


                var roles = await _userManager.GetRolesAsync(user);

                if (roles == null)
                {
                    return BadRequest("Not found role!");
                }

                var jwtToken = _tokenService.CreateJWTToken(user, roles.ToList());

                string message = $"Welcome back, {user.UserName}! :) ";

                var response = new LoginResponse
                {
                    JwtToken = jwtToken,
                    Message = message
                };

                return Ok(response);
            }

            var banTimeLeft = user.AccountDateBan-DateTime.Now;
            return BadRequest("You are account was baned! Ban time left: "+ banTimeLeft);
        }

        [HttpGet("verify")]
        public async Task<IActionResult> Verify(string token)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);

            
            if (user == null)
            {
                return BadRequest(new { message = "Invalid token " });
            }

            if (user.VerifiedAt  != null)
            {
                return BadRequest(new { message = "Alredy verified " });
            }

            user.VerifiedAt = DateTime.Now;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "User verified! :)" });

        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return BadRequest("User with this email not found");
            }

            user.PasswordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            user.ResetTokenExpries = DateTime.Now.AddDays(1);
            await _userManager.UpdateAsync(user);

            EmailDto emailDto = new EmailDto
            {
                To = user.Email,
                Subject = "Reset Password",
                Body = "Your ResetVerification Token is : <a href=\"https://localhost:7267/api/User/reset-password?token="+WebUtility.UrlEncode(user.PasswordResetToken)+"\" > Click here</a>",
            };

            _emailService.SendMail(emailDto);

            return Ok("You may now reset your password."+user.PasswordResetToken);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPassword resetPassword, string token)
        {

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token);

            if (user == null || user.ResetTokenExpries < DateTime.Now)
            {
                return BadRequest("Invalid token");
            }


            if (string.IsNullOrEmpty(resetPassword.Password))
            {
                return BadRequest("New password is required");
            }

            var hashedNewPassword = _userManager.PasswordHasher.HashPassword(user, resetPassword.Password);

            user.PasswordHash = hashedNewPassword;

            user.PasswordResetToken = null;
            user.ResetTokenExpries = null;
            await _userManager.UpdateAsync(user);

            return Ok("Password successfully reset.");
        }
    }
}
