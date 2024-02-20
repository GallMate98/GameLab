using GameLab.Models;

namespace GameLab.Services.GameLobbyAssignment
{
    public interface IGameAssignmentService
    {


        public GameLobby AssignPlayerToGame(Player player1, Player player2, Game game);
        public List<Player> GetLobbyPalyers(Guid gameLobbyId);
        public Player AddStarterPlayer(List<Player> players);
    }
}
