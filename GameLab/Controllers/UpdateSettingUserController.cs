using GameLab.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Security.Claims;

namespace GameLab.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateSettingUserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public UpdateSettingUserController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("my-data")]
        [Authorize]
        public async Task<IActionResult> MyData()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (userEmail == null)
            {
                return BadRequest("User not found");
            }

            var user = await _userManager.FindByEmailAsync(userEmail);

            return Ok(user);
        }


        [HttpPut("update-mydata")]
        [Authorize]
        public async Task<IActionResult> UpdateMyAccount(UpdateUser updateuser)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(userEmail);


            user.Email = updateuser.Email;
            user.FirstName = updateuser.FirstName;
            user.LastName = updateuser.LastName;
            user.UserName = updateuser.UserName;
            user.PhoneNumber = updateuser.PhoneNumber;

            if (await _userManager.Users.AnyAsync(u => u.Email == updateuser.Email && u.Id != user.Id))
            {
                return Conflict("User with this email already exists");
            }

            if (await _userManager.Users.AnyAsync(u => u.UserName == updateuser.UserName && u.Id != user.Id))
            {
                return Conflict("User with this Username already exists");
            }
           
            await _userManager.UpdateAsync(user);

            return Ok("User update succesfully");
        }

        
        [HttpDelete("delete_myaccount")]
        [Authorize]
        public async Task<IActionResult> DeleteMyAccount()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(userEmail); 

            if (user == null)
            {
                return NotFound("User not found or already deleted"); 
            }

           await  _userManager.DeleteAsync(user);

            return Ok("Succesful deleted you are account!"); 
        }
    }
}

