using GameLab.Data;
using GameLab.Models;
using GameLab.Repositories;
using GameLab.Services.DataService;
using GameLab.Services.GameLobbyAssignment;
using GameLab.Services.GameScoreCalculator;
using GameLab.Services.LobbyAssignment;
using GameLab.Services.NineMensMorrisService.cs;
using GameLab.Services.TicTacToe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Claims;

namespace GameLab.Hubs
{
    [Authorize]
    public class GameHub:Hub 
    {
        private readonly IGameAssignmentService _gameAssignmentService;
        private readonly SharedDb _sharedDb;
        private readonly TicTacToeService _ticTacToeService;
        private readonly IGameScoreCalculatorService _gameScoreCalculatorService;
        private readonly GameScoreRepository _gameScoreRepository;


        public GameHub(SharedDb sharedDb,  IGameAssignmentService gameAssignmentService, TicTacToeService ticTacToeService,IGameScoreCalculatorService gameScoreCalculatorService, GameScoreRepository gameScoreRepository )
        {
        
            _sharedDb = sharedDb;
            _gameAssignmentService = gameAssignmentService;
            _ticTacToeService = ticTacToeService;
            _gameScoreCalculatorService = gameScoreCalculatorService;
            _gameScoreRepository = gameScoreRepository;

        }

        public async Task JoinGameLobby(string gameLobbyId, string playerUserName)
        {

            if(_ticTacToeService.GetGameInPogres() == true && _ticTacToeService.GetCurrentPlayer != null)
            {
                _ticTacToeService.SetPlayerJoined(true);
                bool leftTheGame = _sharedDb.LeftGameTime.ContainsKey(playerUserName);
              
                if (leftTheGame == true)
                {
                    List<Player> gamelobbyData = GetLobbyData(gameLobbyId);
                    foreach (Player player in gamelobbyData)
                    {
                        if (playerUserName == player.UserName)
                        {
                            DateTime now = DateTime.Now;
                            DateTime liftTheGameTime = _sharedDb.LeftGameTime[playerUserName];

                            if (now - liftTheGameTime <= TimeSpan.FromMinutes(1))
                            {
                                string opponentConnectionId = AddPlayerConnectionId(gameLobbyId, playerUserName, false);
                                await Clients.Client(opponentConnectionId).SendAsync("StopTimerOpponent");

                            }

                        }
                      
                    }

                }
               

                string userName = Context.User.FindFirstValue(ClaimTypes.Name);

                var connectionId = Context.ConnectionId;

                _sharedDb.Playerconn[gameLobbyId][userName] = connectionId;
                await Groups.AddToGroupAsync(Context.ConnectionId, gameLobbyId);
               

                if(leftTheGame == true)
                {
                    string oppPlayerConnectionId = AddPlayerConnectionId(gameLobbyId, playerUserName, false);
                    
                   
                    GetCurrentBoard(gameLobbyId, playerUserName);

                }
                else
                {
                    GetCurrentBoard(gameLobbyId, playerUserName);
                }
           

            }
            else
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



                    if (!_sharedDb.MoveTime.ContainsKey(gameLobbyId))
                    {
                        _sharedDb.MoveTime.TryAdd(gameLobbyId, new ConcurrentDictionary<string, DateTime>());
                    }
                    await Clients.Group(gameLobbyId).SendAsync("StartGame", starterPlayerUserName);
                    _sharedDb.MoveTime[gameLobbyId][starterPlayerUserName] = DateTime.Now;
                    string playerConnectionId = AddPlayerConnectionId(gameLobbyId, starterPlayerUserName, true);
                    await Clients.Client(playerConnectionId).SendAsync("TimerStart");
                    _ticTacToeService.SetPlayerInCountDown(starterPlayerUserName);
                    _ticTacToeService.SetGameInPogres(true);
                    if (!_sharedDb.X0game.ContainsKey(gameLobbyId))
                    {
                        _sharedDb.X0game.TryAdd(gameLobbyId, new BoardX0());
                    }
                }

          
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

        private string  AddPlayerConnectionId(string gameLobbyId, string playerName, bool player)
        {
            string PlayerConnectionId = "";

            List<Player> myPlayers = GetLobbyData(gameLobbyId);

            foreach (Player myPlayer in myPlayers)
            {
                if(player == true)
                {
                    if (myPlayer.UserName == playerName)
                    {
                        PlayerConnectionId = _sharedDb.Playerconn[gameLobbyId][myPlayer.UserName];
                        return PlayerConnectionId;
                    }
                }
                else
                {
                    if (myPlayer.UserName != playerName)
                    {
                        PlayerConnectionId = _sharedDb.Playerconn[gameLobbyId][myPlayer.UserName];
                        return PlayerConnectionId;
                    }

                }
               
            }

            return "No  player.";
        }


  
        public async Task SendMessage(string lobbyId, string userName, string message)
        {
           
            await Clients.Group(lobbyId).SendAsync("ReceiveMessage", userName, message);
            if (!_sharedDb.Messages.ContainsKey(lobbyId))
            {
                _sharedDb.Messages.TryAdd(lobbyId, new List<Message>());
            }

            var newMessage = new Message { userName = userName, message = message };
            _sharedDb.Messages[lobbyId].Add(newMessage);

        }


        private List<Player> GetLobbyData(string gameLobbyId)
        {
            return _gameAssignmentService.GetLobbyPalyers(Guid.Parse(gameLobbyId));
        }


        public async Task LeaveLobby(string gameLobbyId, string userName, int remainsecondes)
        {
            _ticTacToeService.SetPlayerJoined(false);
            await Task.Delay(2000); 

            if (!_ticTacToeService.GetPlayerJoined())
            {
                int userCount = -1;

                if (_ticTacToeService.GetWinnerExist() == false)
                {
                    _sharedDb.Remainsecondes[userName] = remainsecondes;
                }

                if (!string.IsNullOrEmpty(userName))
                {


                    if (_sharedDb.Userconn.TryGetValue(userName, out string? userId))
                    {
                        _sharedDb.Userconn.Remove(userName, out _);
                    }

                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameLobbyId);


                    if (_sharedDb.Playerconn.TryGetValue(gameLobbyId, out var players))
                    {
                        players.TryRemove(userName, out _);
                        userCount = players.Count;
                    }


                    if (userCount == 0)
                    {
                        _ticTacToeService.Reset();
                        _sharedDb.Playerconn.Remove(gameLobbyId, out _);
                    }

                    string opponentConectionId = AddPlayerConnectionId(gameLobbyId, userName, false);
                    if (userCount !=0 && _ticTacToeService.GetWinnerExist()==false)
                    {
                        await Clients.Client(opponentConectionId).SendAsync("TimerOpponent", 60, userName);
                        _sharedDb.LeftGameTime[userName] = DateTime.Now;
                    }
                }
            }

          
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
                string myConnectionId = AddPlayerConnectionId(gameLobbyId, playerName,true);
                await Clients.Client(myConnectionId).SendAsync("StopTimer");
                _sharedDb.MoveTime[gameLobbyId][newPlayer]= DateTime.Now;
                string opponentConnectionId = AddPlayerConnectionId(gameLobbyId, newPlayer, true);
                await Clients.Client(opponentConnectionId).SendAsync("TimerStart");
                _ticTacToeService.SetPlayerInCountDown(newPlayer);

                string winner = _ticTacToeService.CheckWin(board);

                if (winner != null)
                {
                    _ticTacToeService.SetWinnerExist(true);
                    _ticTacToeService.SetWinnerPlayer(winner);
                   List<Player> gameLobbyData = GetLobbyData(gameLobbyId);
                   List<Player> newScores =  _gameScoreCalculatorService.CalculateScores(Guid.Parse(gameLobbyId), winner, gameLobbyData);
                    await Clients.Group(gameLobbyId).SendAsync("Winner", winner);
                    await Clients.Group(gameLobbyId).SendAsync("StopTimer");
                    await Clients.Group(gameLobbyId).SendAsync("UpdateLobby", newScores);
                    await _gameScoreRepository.ChangeScoreInDatabase(newScores, "Tic-tac-toe");
                }

                bool isDraw = _ticTacToeService.CheckDraw(board);
                if (isDraw && winner == null)
                {
                    
                    await Clients.Group(gameLobbyId).SendAsync("Draw", true);
                    await Clients.Group(gameLobbyId).SendAsync("StopTimer");
                }

            }
        }

        public async Task TimeOut(string gameLobbyId, string playerName)
        {
            DateTime lastTime = DateTime.MinValue;
            DateTime currentTime = DateTime.Now;


            if (_sharedDb.MoveTime.ContainsKey(gameLobbyId))
            {
              lastTime = _sharedDb.MoveTime[gameLobbyId][playerName];

            
            }

    
            if (currentTime - lastTime >= TimeSpan.FromMinutes(1))
            {
                string winner = "";
                if(playerName != _ticTacToeService.GetPlayerName1())
                {
                     winner = _ticTacToeService.GetPlayerName1();
                }
                else
                {
                     winner = _ticTacToeService.GetPlayerName2();
                }

                _ticTacToeService.SetWinnerExist(true);
                _ticTacToeService.SetWinnerPlayer(winner);
                List<Player> gameLobbyData =  GetLobbyData(gameLobbyId);
                List<Player> newScores = _gameScoreCalculatorService.CalculateScores(Guid.Parse(gameLobbyId), winner, gameLobbyData);
                await Clients.Group(gameLobbyId).SendAsync("Winner", winner);
                await Clients.Group(gameLobbyId).SendAsync("UpdateLobby", newScores);
                await _gameScoreRepository.ChangeScoreInDatabase(newScores, "Tic-tac-toe");
            }





        }

        

        public async Task GetCurrentBoard(string gameLobbyId, string userName)
        {
            BoardX0 board = _sharedDb.X0game[gameLobbyId];
            List<Message> lobbyMessages = new List<Message>();
            ConcurrentDictionary<string, string> myboardState = new ConcurrentDictionary<string, string>();
            for (int i = 0; i<3; i++)
            {
                for (int j = 0; j<3; j++)
                {
                    if(board.Board[i,j] =='X' || board.Board[i, j] =='0')
                    {
                        string key = i.ToString()+j.ToString();
                        string value = board.Board[i, j].ToString();
                        myboardState.TryAdd(key, value);
                    }
                }
            }
            string currentPlayer = _ticTacToeService.GetCurrentPlayer();
          
            List<Player> gamelobbyData = GetLobbyData(gameLobbyId);

            if (_sharedDb.Messages.ContainsKey(gameLobbyId))
            {
                lobbyMessages = _sharedDb.Messages[gameLobbyId];
                
            }


            if(_ticTacToeService.GetWinnerExist() == true)
            {
                string winnerPlayer = _ticTacToeService.GetWinnerPlayer();
                await Clients.Caller.SendAsync("Refresh", myboardState, winnerPlayer, gamelobbyData,true);
                await Clients.Caller.SendAsync("RefreshMessages", lobbyMessages);
    
            }
            else
            {
                await Clients.Caller.SendAsync("Refresh", myboardState, currentPlayer, gamelobbyData, false);
                await Clients.Caller.SendAsync("RefreshMessages", lobbyMessages);


                if (_sharedDb.LeftGameTime.ContainsKey(userName))
                {
                    DateTime currentTime = DateTime.Now;
                    DateTime leftTheGameTime = _sharedDb.LeftGameTime[userName];
                    TimeSpan gameLeftDuration = currentTime - leftTheGameTime;

                    double leftSeconds = gameLeftDuration.TotalSeconds;

                    if ((int)leftSeconds>0)
                    {
                        int remainSeconds = _sharedDb.Remainsecondes[userName];
                        await Clients.Caller.SendAsync("RefreshCountDown", remainSeconds);
                        _sharedDb.LeftGameTime.TryRemove(userName, out _);
                        _sharedDb.Remainsecondes.TryRemove(userName, out _);
                    }
                }
                else
                {
                    if (userName == _ticTacToeService.GetPlayerInCountDown())
                    {
                        DateTime lastTime = _sharedDb.MoveTime[gameLobbyId][userName];
                        DateTime now = DateTime.Now;
                        TimeSpan duration = lastTime.AddMinutes(1) - now;

                        double seconds = duration.TotalSeconds;




                        await Clients.Caller.SendAsync("RefreshCountDown", (int)seconds);

                        bool empty = _sharedDb.LeftGameTime.IsEmpty;

                        if (!empty)
                        {
                            var opponent = _sharedDb.LeftGameTime.FirstOrDefault();
                            string opponentName = opponent.Key;
                            DateTime remainTime = opponent.Value;
                            DateTime currentTime = DateTime.Now;

                            TimeSpan durationOppIneterval = remainTime.AddMinutes(1) - currentTime;

                            double oppseconds = durationOppIneterval.TotalSeconds;

                            await Clients.Caller.SendAsync("TimerOpponent", (int)oppseconds, opponentName);
                        }

                    }

                }
              
            }
           
            
        }

    }
}
