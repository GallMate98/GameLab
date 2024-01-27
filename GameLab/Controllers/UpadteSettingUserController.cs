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
    public class UpadteSettingUserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public UpadteSettingUserController(UserManager<User> userManager)
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


        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateUser(UpdateUser updateuser)
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

        // DELETE: api/user
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteCurrentUser()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email); // Azonosító lekérése a JWT tokentől

            var user = await _userManager.FindByEmailAsync(userEmail); // Felhasználó lekérése az azonosító alapján

            if (user == null)
            {
                return NotFound("User not found or already deleted"); // Nem található felhasználó
            }

           await  _userManager.DeleteAsync(user);

            return Ok("Succesful deleted you are account!"); 
        }
    }
}

