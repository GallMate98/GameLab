﻿using GameLab.Models;

namespace GameLab.Services.GameLobbyAssignment
{
    public class GameAssignmentService : IGameAssignmentService
    {

        private readonly List<GameLobby> _gameLobbies;

        public GameAssignmentService() 
        {
            _gameLobbies = new List<GameLobby>();
        }

        public GameLobby AssignPlayerToGame(Player player1,Player player2, Game game)
        {
            var gameLobby = CreateNewGame(game.Id, player1,player2);

            _gameLobbies.Add(gameLobby);
            return gameLobby;
        }

        //public Player RemovePlayer(Guid gameLobbyId, string userName)
        //{
        //    var gameLobby = _gameLobbies.FirstOrDefault(l => l.Id == gameLobbyId);

       

        //    if(userName == gameLobby.Player1.UserName || userName == gameLobby.Player2.UserName)
        //    {
        //        return 
        //    }


        //    //lobby.Players.Remove(playerToRemove);

        //    //return lobby.Players;
        //}


        public List<Player> GetLobbyPalyers(Guid gameLobbyId)
        {
            var gameLobby = _gameLobbies.FirstOrDefault(l => l.Id == gameLobbyId);

            if (gameLobby == null)
            {
                throw new Exception("Lobby error");
            }

            List<Player> players = new List<Player>();
            players.Add(gameLobby.Player1);
            players.Add(gameLobby.Player2);

            return players;
        }

        public Player AddStarterPlayer (List<Player> players)
        {
            Random rand = new Random();
           int startPlayerIndex = rand.Next(players.Count);

            return players[startPlayerIndex];
        }


        private GameLobby CreateNewGame(Guid gameId, Player player1, Player player2)
        {
            var newGameLobby = new GameLobby
            {
                Id = Guid.NewGuid(),
                Player1 = player1,
                Player2 = player2,
                GameId  = gameId,
                
            };

            _gameLobbies.Add(newGameLobby);

            return newGameLobby;
        }
    }
}
