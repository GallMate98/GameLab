using GameLab.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameLab.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserInfoController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public UserInfoController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("get-user")]
        [Authorize (Roles = "User,Moderator,Admin")]
        public async Task<IActionResult> GetUserByUsernameAsync(UpdatePermission update)
        {

            var user = await _userManager.FindByNameAsync(update.UserName);

            if (user == null)
            {
                return BadRequest("User not found");
            }
            return Ok(user);
        }

        [HttpGet("get-users")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            if (users.Any())
            {
                return Ok(users);
            }

            return BadRequest("Database empty!"); 
        }

        [HttpDelete("delete-user")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(UpdatePermission update)
        {
            var user = await _userManager.FindByNameAsync(update.UserName);

            if (user == null)
            {
                return BadRequest("User not found or already deleted");
            }

            await _userManager.DeleteAsync(user);

            return Ok("User deleted successfully");
        }

        [HttpDelete("delete-users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            if (users == null)
            {
                return BadRequest("Users not found or all users already deleted");
            }

            foreach (var user in users)
            {
                await _userManager.DeleteAsync(user);
            }

            return Ok("User deleted successfully");
        }
    }
}
