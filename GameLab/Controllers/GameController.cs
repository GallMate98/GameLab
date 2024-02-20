﻿using GameLab.Data;
using GameLab.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Data;
using static Azure.Core.HttpHeader;
using System.Security.Claims;
using GameLab.Services.LobbyAssignment;

namespace GameLab.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly ILobbyAssignmentService _lobbyAssigment;
        private readonly UserManager<User> _userManager;
        public GameController(DataContext dataContext, ILobbyAssignmentService lobbyAssignment, UserManager<User> userManager)
        {
            _dataContext = dataContext;
            _lobbyAssigment = lobbyAssignment;
            _userManager = userManager; 
        }

        [HttpGet("getGames")]
        [Authorize(Roles = "User")]
        public ActionResult<IEnumerable<Game>> GetGames()
        {
            var games = _dataContext.Games.ToList();

            return Ok(games);
        }

        [HttpGet("get-lobby/{gameUrl}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> JoinLobby(string gameUrl)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

           var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if(user == null)
            {
                return NotFound("User not found");
            }

            Player player = new Player()
            {
                UserName = user.UserName,
                Score = 200
            };

            var game = await _dataContext.Games.FirstOrDefaultAsync(g => g.Url == gameUrl);


            if (game == null)
            {
                return NotFound("Game not found");
            }

            Lobby lobby =  _lobbyAssigment.AssignPlayerToLobby(player, game);


            return Ok(lobby.Id);
        }

 
    }
}
