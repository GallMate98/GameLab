﻿using GameLab.Data;
using GameLab.Models;
using GameLab.Services.DataService;
using GameLab.Services.GameLobbyAssignment;
using GameLab.Services.LobbyAssignment;
using GameLab.Services.TicTacToe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace GameLab.Hubs
{
    [Authorize]
    public class GameHub:Hub 
    {
        private readonly IGameAssignmentService _gameAssignmentService;
        private readonly SharedDb _sharedDb;
        private readonly TicTacToeService _ticTacToeService;

        public GameHub(SharedDb sharedDb,  IGameAssignmentService gameAssignmentService, TicTacToeService ticTacToeService)
        {
        
            _sharedDb = sharedDb;
            _gameAssignmentService = gameAssignmentService;
            _ticTacToeService = ticTacToeService;

        }

        public async Task JoinGameLobby(string gameLobbyId, string playerUserName)
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
               _ticTacToeService.SetPlayers(starterPlayerUserName);
                if (gamelobbyData[0].UserName != starterPlayerUserName)
                {
                    _ticTacToeService.SetPlayers(gamelobbyData[0].UserName);
                }
                else
                {
                    _ticTacToeService.SetPlayers(gamelobbyData[1].UserName);
                }
               await Clients.Group(gameLobbyId).SendAsync("StartGame", starterPlayerUserName);
            }

          
            

          //  Frissítsd a lobby-t a klienseknek
            //if (_sharedDb.Messages.ContainsKey(lobbyId))
            //{
            //    var messages = _sharedDb.Messages[lobbyId];
            //    await Clients.Group(lobbyId).SendAsync("UpdateLobby", lobbyData, messages);
            //}
            //else
            //{
            //   // Ha a lobby üzenetei nem elérhetők, akkor csak a lobby adatokat küldjük
            //   await Clients.Group(lobbyId).SendAsync("UpdateLobby", lobbyData, null);
            //}
        }


        public async Task SendMessage(string lobbyId, string userName, string message)
        {

            await Clients.Group(lobbyId).SendAsync("ReceiveMessage", userName, message);

        }


        private List<Player> GetLobbyData(string gameLobbyId)
        {
            return _gameAssignmentService.GetLobbyPalyers(Guid.Parse(gameLobbyId));
        }

        public async Task GamePlay(string gameLobbyId, string playerName, int row, int col)
        {
           
            if (!_sharedDb.X0game.ContainsKey(gameLobbyId))
            {
                _sharedDb.X0game.TryAdd(gameLobbyId, new BoardX0());
            }

         bool isOk =  _ticTacToeService.CheckPosition(gameLobbyId,playerName,row,col);

            if (isOk)
            {
                BoardX0 board = _sharedDb.X0game[gameLobbyId];
                char value = board.Board[row,col];

                string newPlayer = _ticTacToeService.GetCurrentPlayer();

                await Clients.Group(gameLobbyId).SendAsync("NewPosition", value, newPlayer, row, col);

                string winner = _ticTacToeService.CheckWin(board);

                if (winner != null)
                {
                    await Clients.Group(gameLobbyId).SendAsync("Winner", winner);
                }

                bool isDraw = _ticTacToeService.CheckDraw(board);
                if (isDraw && winner == null)
                {
                    
                    await Clients.Group(gameLobbyId).SendAsync("Draw", true);
                }

            }



        }

    }
}
