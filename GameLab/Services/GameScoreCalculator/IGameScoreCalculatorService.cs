using GameLab.Models;

namespace GameLab.Services.GameScoreCalculator
{
    public interface IGameScoreCalculatorService
    {
        public List<Player> CalculateScores(Guid gameLobbyId, string winnerPlayerName, List<Player> players);
    }
}
