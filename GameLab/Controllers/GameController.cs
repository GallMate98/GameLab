using GameLab.Data;
using GameLab.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GameLab.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly DataContext _dataContext;
        public GameController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet("getGames")]
        [Authorize(Roles = "User")]
        public ActionResult<IEnumerable<Game>> GetGames()
        {
            var games = _dataContext.Games.ToList();

            return Ok(games);
        }
    }
}
