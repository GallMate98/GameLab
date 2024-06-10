using GameLab.Data;
using GameLab.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameLab.Services.LobbyAssignment
{
    public class LobbyAssignmentService:ILobbyAssignmentService
    {
        private readonly List<Lobby> _lobbies;

        public LobbyAssignmentService()
        {
            _lobbies = new List<Lobby>();
        }

        public Lobby AssignPlayerToLobby(Player player, Game game)
        {
            int steps = 150;
            int lowerBound = (player.Score / steps) * steps;

            List<Lobby> potentialLobbies = _lobbies.Where(lobby => lobby.LobbyScore == lowerBound).ToList();

            foreach (Lobby myLobby in potentialLobbies) 
            {
                foreach(Player myPlayer in myLobby.Players)
                {
                    if(myPlayer.UserName == player.UserName)
                    {
                        return myLobby;
                    }
                }
            }

            var availableLobbies = _lobbies
                .Where(lobby => lobby.GameId == game.Id && lobby.LobbyScore == lowerBound && lobby.Players.Count < 10)
                .ToList();

            Lobby selectedLobby = availableLobbies.FirstOrDefault();

            if (selectedLobby == null)
            {
                selectedLobby = CreateNewLobby(game.Id, lowerBound);
                _lobbies.Add(selectedLobby);
            }

            selectedLobby.Players.Add(player); 
            return selectedLobby;

        
        }

        public List<Player> RemovePlayer(Guid lobbyId, string userName)
        {
            var lobby = _lobbies.FirstOrDefault(l => l.Id == lobbyId);

            Player playerToRemove = lobby.Players.FirstOrDefault(p=> p.UserName == userName);

            lobby.Players.Remove(playerToRemove);

            return lobby.Players;
        }


        public List<Player> GetLobbyPalyers(Guid lobbyId)
        {
            var lobby = _lobbies.FirstOrDefault(l=> l.Id == lobbyId);

            if (lobby == null)
            {
                throw new Exception("Lobby error"); 
            }

            return lobby.Players;
        }


        private Lobby CreateNewLobby(Guid gameId, int lobbyScore)
        {
            var newLobby = new Lobby
            {
                Id = Guid.NewGuid(), 
                GameId  = gameId,
                LobbyScore = lobbyScore,
                Players = new List<Player>()
            };

            _lobbies.Add(newLobby);

            return newLobby;
        }
    }
}
