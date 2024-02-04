using GameLab.Models;

namespace GameLab.Services.LobbyAssignment
{
    public interface ILobbyAssignmentService
    {
        public Lobby AssignPlayerToLobby(Player player, Game game);
        public List<Player> GetLobbyPalyers(Guid lobbyId);

        public List<Player> RemovePlayer(Guid lobbyId, string userName);

    }
}
