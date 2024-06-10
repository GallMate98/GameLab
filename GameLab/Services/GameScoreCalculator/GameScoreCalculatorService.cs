using GameLab.Data;
using GameLab.Models;
using GameLab.Services.GameLobbyAssignment;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace GameLab.Services.GameScoreCalculator
{
    public class GameScoreCalculatorService:IGameScoreCalculatorService
    {
        
        private readonly UserManager<User> _userManager;
        private readonly DataContext _dataContext;
        private readonly IGameAssignmentService _gameAssignmentService;

        public GameScoreCalculatorService(DataContext dataContext, UserManager<User> userManager, IGameAssignmentService gameAssignmentService)
        {
            _dataContext = dataContext;
            _userManager = userManager;
            _gameAssignmentService = gameAssignmentService;
        }

      


        public List<Player> CalculateScores(Guid gameLobbyId, string winnerPlayerName, List<Player> players)
        {
            int loserPlayerScore;
            int winnerPlayerScore;
           List<Player> playersWithNewScores = new List<Player>();

            if (players[0].UserName == winnerPlayerName)
            {
                winnerPlayerScore =players[0].Score;
                loserPlayerScore = players[1].Score;
            }
            else
            {
                winnerPlayerScore =players[1].Score;
                loserPlayerScore = players[0].Score;
            }

            if (Math.Abs(winnerPlayerScore-loserPlayerScore) < 50)
            {
                winnerPlayerScore = CalaculatIfHaveLittel(winnerPlayerScore, true);
                loserPlayerScore = CalaculatIfHaveLittel(loserPlayerScore, false);
            }
            else if (Math.Abs(winnerPlayerScore-loserPlayerScore) > 50)
            {
                if (winnerPlayerScore < loserPlayerScore)
                {
                    winnerPlayerScore = WeakerWin(winnerPlayerScore, Math.Abs(winnerPlayerScore-loserPlayerScore));
                    loserPlayerScore = StrongestLose(loserPlayerScore, Math.Abs(winnerPlayerScore-loserPlayerScore));
                }
                else
                {
                    winnerPlayerScore = StrongestWin(winnerPlayerScore, Math.Abs(winnerPlayerScore-loserPlayerScore));
                    loserPlayerScore = WeakerLose(loserPlayerScore, Math.Abs(winnerPlayerScore-loserPlayerScore));
                }
            }



            if (winnerPlayerName == players[0].UserName)
                playersWithNewScores = _gameAssignmentService.ChangeLobbyPalyersScores(gameLobbyId, players[0].UserName, winnerPlayerScore, loserPlayerScore); 
            else
                playersWithNewScores = _gameAssignmentService.ChangeLobbyPalyersScores(gameLobbyId, players[1].UserName, winnerPlayerScore, loserPlayerScore);
                
            

            return playersWithNewScores;
        }

       




        private int CalaculatIfHaveLittel(int score, bool winner)
        {
            if (winner == true)
            {
                score += 5*score/100;
                return (int)score;
            }
            else
            {
                score -= 5*score/100;
                return (int)score;
            }
        }

        private int StrongestWin(int score, int differeceBetweenTwoPlayers)
        {

            int percent = (int)differeceBetweenTwoPlayers/50;
            int basicPercent = 7-percent;
            if (basicPercent<=1)
            {
                score += score/100;
                return score;
            }
            else
            {
                score += basicPercent*score/100;
                return score;
            }

        }

        private int WeakerWin(int score, int differeceBetweenTwoPlayers)
        {
            int percent = (int)differeceBetweenTwoPlayers/50;
            int basicPercent = 5+percent;
            score += basicPercent*score/100;
            return score;
        }

        private int StrongestLose(int score, int differeceBetweenTwoPlayers)
        {
            int percent = (int)differeceBetweenTwoPlayers/50;
            int basicPercent = 5+percent;
            score -= basicPercent*score/100;

            if (score <= 0)
            {
                score = 0;
                return score;
            }
            return score;
        }

        private int WeakerLose(int score, int differeceBetweenTwoPlayers)
        {
            int percent = (int)differeceBetweenTwoPlayers/50;
            int basicPercent = 7-percent;
            if (basicPercent<=1)
            {
                score -= score/100;
                return score;
            }
            else
            {
                score -= basicPercent*score/100;
                return score;
            }
        }
    }
}
