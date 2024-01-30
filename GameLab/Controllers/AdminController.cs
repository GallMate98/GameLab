using GameLab.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GameLab.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase

    {
        private readonly UserManager<User> _userManager;

        public AdminController(UserManager<User> userManager) 
        {
            _userManager = userManager;
        }

        [HttpPut("addAdminRole")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> MakeAdminRole(UpdatePermission update)
        {
            var user = await _userManager.FindByNameAsync(update.UserName);

            if (user == null)
            {
                return BadRequest("Invalid User name!");
            }

            var userIsAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (userIsAdmin)
            {
                return BadRequest("User is already an Admin!");
            }

            var newRole = await _userManager.AddToRoleAsync(user, "Admin");

            if(newRole.Succeeded)
            {
                return Ok("User is now Admin");
            }

            return BadRequest("Somthing is wrong!");
        }

        [HttpPut("removeAdminRole")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveAdminRole(UpdatePermission update)
        {
            var user = await _userManager.FindByNameAsync(update.UserName);

            if (user == null)
            {
                return BadRequest("Invalid User name!");
            }

            var userIsAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (!userIsAdmin)
            {
                return BadRequest("User is not an Admin!");
            }

            var newRole = await _userManager.RemoveFromRoleAsync(user, "Admin");

            if (newRole.Succeeded)
            {
                return Ok("Admin role removed successfully");
            }

            return BadRequest("Somthing is wrong!");
        }


        [HttpPut("addModeratorRole")]
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> MakeModeratorRole(UpdatePermission update)
        {
            var user = await _userManager.FindByNameAsync(update.UserName);

            if (user == null)
            {
                return BadRequest("Invalid User name!");
            }

            var userIsModerator = await _userManager.IsInRoleAsync(user, "Moderator");

            if (userIsModerator)
            {
                return BadRequest("User is already an Moderator!");
            }

            var newRole = await _userManager.AddToRoleAsync(user, "Moderator");

            if (newRole.Succeeded)
            {
                return Ok("User is now Moderator");
            }

            return BadRequest("Somthing is wrong!");
        }

        [HttpPut("removeModeratorRole")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveModeratorRole(UpdatePermission update)
        {
            var user = await _userManager.FindByNameAsync(update.UserName);

            if (user == null)
            {
                return BadRequest("Invalid User name!");
            }

            var userIsModerator = await _userManager.IsInRoleAsync(user, "Moderator");

            if (!userIsModerator)
            {
                return BadRequest("User is not an Moderator!");
            }

            var newRole = await _userManager.RemoveFromRoleAsync(user, "Moderator");

            if (newRole.Succeeded)
            {
                return Ok("Moderator role removed successfully");
            }

            return BadRequest("Somthing is wrong!");
        }

        [HttpPut("acount-ban")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AccountBan(UpdatePermission update)
        {
            var user = await _userManager.FindByNameAsync(update.UserName);

            if (user == null)
            {
                return BadRequest("User name not found!");
            }

            user.AccountDateBan = DateTime.Now.AddMinutes(1);
            await _userManager.UpdateAsync(user);

            return Ok("Account was banned!");
        }

    }
}
