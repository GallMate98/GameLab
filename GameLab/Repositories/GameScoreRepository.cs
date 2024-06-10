using GameLab.Data;
using GameLab.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameLab.Repositories
{
    public class GameScoreRepository
    {
        private readonly DataContext _dataContext;

        public GameScoreRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IActionResult> ChangeScoreInDatabase(List<Player> players, string gameType)
        {
            var game = await _dataContext.Games.FirstOrDefaultAsync(g => g.Name == gameType);
            if (game == null)
            {
                return new NotFoundObjectResult($"Game '{gameType}' not found");
            }

            foreach (var player in players)
            {
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.UserName == player.UserName);
                if (user == null)
                {
                    return new NotFoundObjectResult($"User '{player.UserName}' not found");
                }

                var userScore = await _dataContext.GameScores.FirstOrDefaultAsync(gs => gs.GameId == game.Id && gs.UserId == user.Id);
                if (userScore != null)
                {
                    userScore.Score = player.Score;
                    _dataContext.GameScores.Update(userScore);
                }
                else
                {
                    return new NotFoundObjectResult($"Score for user '{player.UserName}' not found");
                }
            }

            await _dataContext.SaveChangesAsync();
            return new OkObjectResult("Scores updated successfully");
        }
    }
}
