using GameLab.Data;
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
        private readonly DataContext _dataContext;

        public UserInfoController(DataContext dataContext, UserManager<User> userManager)
        {
            _userManager = userManager;
            _dataContext = dataContext;
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

        [HttpGet("get-searchUsers")]
        [Authorize(Roles = "User,Moderator,Admin")]
        public async Task<IActionResult> SearchUsersAsync(string userNameStart)
        {
            if (string.IsNullOrEmpty(userNameStart))
            {
                return BadRequest("Username start cannot be empty");
            }
            var users = await _userManager.Users.Where(u => u.UserName.StartsWith(userNameStart)).ToListAsync();

            if (!users.Any())
            {
                return NotFound("No users found with the given start.");
            }

            var userWithScores = new List<object>();
            foreach (var user in users)
            {
                var gameUserGameScores = await _dataContext.GameScores.Where(gs => gs.UserId == user.Id).ToListAsync();
                if (!gameUserGameScores.Any())
                {
                    userWithScores.Add(new
                    {
                        UserName = user.UserName,
                        GameName = "No games played yet",
                        Score = 0
                    });
                }
                else
                {
                    foreach (var gameScore in gameUserGameScores)
                    {
                        var gameName = await _dataContext.Games.Where(g => g.Id == gameScore.GameId).Select(g => g.Name).FirstOrDefaultAsync();

                        userWithScores.Add(new
                        {
                            UserName = user.UserName,
                            GameName = gameName,
                            Score = gameScore.Score
                        });
                    }
                }
            }

            return Ok(userWithScores);


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
