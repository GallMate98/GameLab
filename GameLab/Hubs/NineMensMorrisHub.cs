using GameLab.Models;
using GameLab.Services.DataService;
using GameLab.Services.GameLobbyAssignment;
using GameLab.Services.NineMensMorrisService.cs;
using GameLab.Services.TicTacToe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace GameLab.Hubs
{
    [Authorize]
    public class NineMensMorrisHub : Hub
    {
        private readonly IGameAssignmentService _gameAssignmentService;
        private readonly SharedDb _sharedDb;
        private readonly NineMensMorrisService _nineMensMorrisService;

        public NineMensMorrisHub(SharedDb sharedDb, IGameAssignmentService gameAssignmentService, NineMensMorrisService nineMensMorrisService)
        {

            _sharedDb = sharedDb;
            _gameAssignmentService = gameAssignmentService;
            _nineMensMorrisService = nineMensMorrisService;
           

        }

        public async Task HelloMessage()
        {
            await Console.Out.WriteLineAsync("hi");
        }

        public async Task JoinNineMensMorrisGameLobby(string gameLobbyId, string playerUserName)
        {

            if (string.IsNullOrEmpty(gameLobbyId) || string.IsNullOrEmpty(playerUserName))
            {

                throw new ArgumentException("Empty Data");
            }

            string userName = Context.User.FindFirstValue(ClaimTypes.Name);

            var connectionId = Context.ConnectionId;

            if (!_sharedDb.Playerconn.ContainsKey(gameLobbyId))
            {
                _sharedDb.Playerconn.TryAdd(gameLobbyId, new ConcurrentDictionary<string, string>());
            }


            List<Player> gamelobbyData = GetLobbyData(gameLobbyId);

            if (gamelobbyData.Count > 0)
            {
                string firstPlayerUserName = gamelobbyData[0].UserName;
                string secondPlayerUserName = gamelobbyData[1].UserName;

                if (firstPlayerUserName == playerUserName || secondPlayerUserName == playerUserName)
                {


                    _sharedDb.Playerconn[gameLobbyId][userName] = connectionId;
                    await Groups.AddToGroupAsync(Context.ConnectionId, gameLobbyId);

                    await Clients.Group(gameLobbyId).SendAsync("UpdateLobby", gamelobbyData);
                }
            }

            if (_sharedDb.Playerconn[gameLobbyId].Count == 2)
            {
                Player starterPlayer = _gameAssignmentService.AddStarterPlayer(GetLobbyData(gameLobbyId));
                string starterPlayerUserName = starterPlayer.UserName;
                _nineMensMorrisService.SetPlayers(starterPlayerUserName);
                if (gamelobbyData[0].UserName != starterPlayerUserName)
                {
                    _nineMensMorrisService.SetPlayers(gamelobbyData[0].UserName);
                }
                else
                {
                    _nineMensMorrisService.SetPlayers(gamelobbyData[1].UserName);
                }
                await Clients.Group(gameLobbyId).SendAsync("StartGame", starterPlayerUserName);
            }

        }


        public async Task SendMessage(string lobbyId, string userName, string message)
        {

            await Clients.Group(lobbyId).SendAsync("ReceiveMessage", userName, message);

        }

        private List<Player> GetLobbyData(string gameLobbyId)
        {
            return _gameAssignmentService.GetLobbyPalyers(Guid.Parse(gameLobbyId));
        }


        public async Task AddPiece(string gameLobbyId, string playerName, string position)
        {
          
           int row = int.Parse(position[0].ToString());
           int col = int.Parse(position[1].ToString());


            if (!_sharedDb.ninemensgame.ContainsKey(gameLobbyId))
            {
                _sharedDb.ninemensgame.TryAdd(gameLobbyId, new BoardNineMens());
            }

            BoardNineMens myboard = _sharedDb.ninemensgame[gameLobbyId];

         bool isOk = _nineMensMorrisService.CheckPosition(gameLobbyId, playerName, row, col, myboard);

            Console.WriteLine(myboard); 

            if(isOk)
            {
              
            }
        }
    }
    }
