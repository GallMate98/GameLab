using GameLab.Models;
using GameLab.Repositories;
using GameLab.Services.DataService;
using GameLab.Services.GameLobbyAssignment;
using GameLab.Services.GameScoreCalculator;
using GameLab.Services.NineMensMorrisService.cs;
using GameLab.Services.TicTacToe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Claims;

namespace GameLab.Hubs
{
    [Authorize]
    public class NineMensMorrisHub : Hub
    {
        private readonly IGameAssignmentService _gameAssignmentService;
        private readonly SharedDb _sharedDb;
        private readonly NineMensMorrisService _nineMensMorrisService;
        private readonly GameScoreRepository _gameScoreRepository;
        private readonly IGameScoreCalculatorService _gameScoreCalculatorService;
        private  List<Player> ordergamelobbyData = new List<Player>();
       


        public NineMensMorrisHub(SharedDb sharedDb, IGameAssignmentService gameAssignmentService, NineMensMorrisService nineMensMorrisService, GameScoreRepository gameScoreRepository, IGameScoreCalculatorService gameScoreCalculatorService)
        {

            _sharedDb = sharedDb;
            _gameAssignmentService = gameAssignmentService;
            _nineMensMorrisService = nineMensMorrisService;
            _gameScoreRepository = gameScoreRepository;
            _gameScoreCalculatorService = gameScoreCalculatorService;
        }

        public async Task JoinNineMensMorrisGameLobby(string gameLobbyId, string playerUserName)
 
        {
            bool pogress = _nineMensMorrisService.GetGameInPogres();
            if (_nineMensMorrisService.GetGameInPogres() == true && _nineMensMorrisService.GetCurrentPlayer != null)
            {
                _nineMensMorrisService.SetPlayerJoined(true);
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
                                string opponentConnectionId = AddOpponentPlayerConnectionId(gameLobbyId, playerUserName);
                                await Clients.Client(opponentConnectionId).SendAsync("StopTimerOpponent");

                            }
                        }
                    }
                }
                string userName = Context.User.FindFirstValue(ClaimTypes.Name);

                var connectionId = Context.ConnectionId;

                _sharedDb.Playerconn[gameLobbyId][userName] = connectionId;
                await Groups.AddToGroupAsync(Context.ConnectionId, gameLobbyId);
                GetCurrentBoard(gameLobbyId, playerUserName);

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

                bool addedFirstPlayer = false;
                Player secondPlayer = null;

                if (gamelobbyData.Count > 0)
                {
                    string firstPlayerUserName = gamelobbyData[0].UserName;
                    string secondPlayerUserName = gamelobbyData[1].UserName;

                    if (firstPlayerUserName == playerUserName || secondPlayerUserName == playerUserName)
                    {


                        _sharedDb.Playerconn[gameLobbyId][userName] = connectionId;
                        await Groups.AddToGroupAsync(Context.ConnectionId, gameLobbyId);


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
                    foreach (Player player in gamelobbyData)
                    {
                        if (addedFirstPlayer == false)
                        {
                            if (player.UserName == starterPlayerUserName)
                            {
                                ordergamelobbyData.Add(player);
                                addedFirstPlayer = true;
                            }
                            else
                                secondPlayer = player;

                        }
                        else
                            secondPlayer = player;

                        if (secondPlayer != null && addedFirstPlayer == true)
                        {
                            ordergamelobbyData.Add(secondPlayer);
                        }

                    }
                    gamelobbyData = ordergamelobbyData;



                    await Clients.Group(gameLobbyId).SendAsync("UpdateLobby", gamelobbyData);
                    if (!_sharedDb.MoveTime.ContainsKey(gameLobbyId))
                    {
                        _sharedDb.MoveTime.TryAdd(gameLobbyId, new ConcurrentDictionary<string, DateTime>());
                    }
                    await Clients.Group(gameLobbyId).SendAsync("StartGame", starterPlayerUserName);
                    _sharedDb.MoveTime[gameLobbyId][starterPlayerUserName] = DateTime.Now;
                    string playerConnectionId = AddPlayerConnectionId(gameLobbyId, starterPlayerUserName);
                    await Clients.Client(playerConnectionId).SendAsync("TimerStart");
                    _nineMensMorrisService.SetGameInPogres(true);
                    _nineMensMorrisService.SetPlayerInCountDown(starterPlayerUserName);

                }
            }

          


        }

        public async Task SendMessage(string lobbyId, string userName, string message)
        {

            await Clients.Group(lobbyId).SendAsync("ReceiveMessage", userName, message);

            if (!_sharedDb.Messages.ContainsKey(lobbyId))
            {
                _sharedDb.Messages.TryAdd(lobbyId, new List<Message>());
            }

            var newMessage = new Message { userName = userName,  message = message };
            _sharedDb.Messages[lobbyId].Add(newMessage);

        }

        private List<Player> GetLobbyData(string gameLobbyId)
        {
            return _gameAssignmentService.GetLobbyPalyers(Guid.Parse(gameLobbyId));
        }

        private string AddOpponentPlayerConnectionId (string gameLobbyId, string playerName)
        {
            string opponentPlayerConnectionId="";

            List<Player> myPlayers = GetLobbyData(gameLobbyId);

            foreach (Player myPlayer in myPlayers)
            {
                if (myPlayer.UserName != playerName)
                {
                    opponentPlayerConnectionId = _sharedDb.Playerconn[gameLobbyId][myPlayer.UserName];
                    return opponentPlayerConnectionId;
                }
            }

            return "No opponent player.";
        }

        private string AddPlayerConnectionId(string gameLobbyId, string playerName)
        {
            string opponentPlayerConnectionId = "";

            List<Player> myPlayers = GetLobbyData(gameLobbyId);

            foreach (Player myPlayer in myPlayers)
            {
                if (myPlayer.UserName == playerName)
                {
                    opponentPlayerConnectionId = _sharedDb.Playerconn[gameLobbyId][myPlayer.UserName];
                    return opponentPlayerConnectionId;
                }
            }

            return "No  player.";
        }

        private int AddOpponentPlayerPieceCount(int myPieceType, BoardNineMens myboard)
        {
            int oppenentPieceCountInBoard;
            if (myPieceType < 10)
            {
               oppenentPieceCountInBoard = _nineMensMorrisService.PieceCount('0', myboard);
                return oppenentPieceCountInBoard;
            }
            else
            {
                oppenentPieceCountInBoard = _nineMensMorrisService.PieceCount('1', myboard);
                return oppenentPieceCountInBoard;
            }
        }


        public async Task LeaveLobby(string gameLobbyId, string userName, int remainsecondes)
        {
            _nineMensMorrisService.SetPlayerJoined(false);
            await Task.Delay(2000);

            if (!_nineMensMorrisService.GetPlayerJoined())
            {
                int userCount = -1;

                if (_nineMensMorrisService.GetWinnerExist() == false)
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
                        _nineMensMorrisService.Reset();
                        _sharedDb.Playerconn.Remove(gameLobbyId, out _);
                    }

                    string opponentConectionId = AddOpponentPlayerConnectionId(gameLobbyId, userName);
                    if (userCount !=0 && _nineMensMorrisService.GetWinnerExist()==false)
                    {
                        await Clients.Client(opponentConectionId).SendAsync("TimerOpponent", 60, userName);
                        _sharedDb.LeftGameTime[userName] = DateTime.Now;
                    }
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
                if (playerName == _nineMensMorrisService.GetPlayer1Name())
                {
                    winner = _nineMensMorrisService.GetPlayer1Name();
                }
                else
                {
                    winner = _nineMensMorrisService.GetPlayer2Name();
                }

                List<Player> gameLobbyData = GetLobbyData(gameLobbyId);
                List<Player> newScores = _gameScoreCalculatorService.CalculateScores(Guid.Parse(gameLobbyId), playerName, gameLobbyData);
                _nineMensMorrisService.SetWinnerExist(true);
                _nineMensMorrisService.SetWinnerPlayer(winner);
                await Clients.Group(gameLobbyId).SendAsync("Winner", winner);
                await Clients.Group(gameLobbyId).SendAsync("UpdateLobby", newScores);
                await _gameScoreRepository.ChangeScoreInDatabase(newScores, "Nine Men's Morris");
            }
        }


        public async Task<bool> AddPiece(string gameLobbyId, string playerName, string position, string id, string previousPosition)
        {
            int row = int.Parse(position[0].ToString());
            int col = int.Parse(position[1].ToString());
            int myPieceId = int.Parse(id);
            List<string> selectedPostions;
            int redPieceInBoard;
            int greenPieceInBoard;
            string newPlayer = "";



            string myColorIs;
            int pieceCount;
            if (myPieceId<10)
            {
                myColorIs = "red";
            }
            else
            {
                myColorIs = "green";
            }

            if (previousPosition !="3" && previousPosition != "-1")
                return false;

            if (!_sharedDb.ninemensgame.ContainsKey(gameLobbyId))
            {
                _sharedDb.ninemensgame.TryAdd(gameLobbyId, new BoardNineMens());
            }
            
            BoardNineMens myboard = _sharedDb.ninemensgame[gameLobbyId];

            bool isOk = _nineMensMorrisService.CheckPosition(playerName, row, col, myboard, myColorIs);

            if (!isOk)
            {
                return false;
            }


            if(myColorIs == "red")
               pieceCount = _nineMensMorrisService.GetPlayer1PieceCount();
            else 
                pieceCount = _nineMensMorrisService.GetPlayer2PieceCount();

            _sharedDb.PiecePositionList[id] = position;

          string  myConnectionId = _sharedDb.Playerconn[gameLobbyId][playerName];

             newPlayer = _nineMensMorrisService.ChangeCurrentPlayer(playerName);

            bool isMill = _nineMensMorrisService.CheckIsMill(myboard, row, col);
            if (isMill)
            {
                _nineMensMorrisService.SetHaveMillsBeforeRefresh(true);
                if(myColorIs == "red")
                {
                    selectedPostions = _nineMensMorrisService.PositionsForSelection('1', myboard);
                }
                else
                {
                   selectedPostions = _nineMensMorrisService.PositionsForSelection('0', myboard);

                }
                _nineMensMorrisService.SetInRefreshSelectedPostion(selectedPostions);
                await Clients.Group(gameLobbyId).SendAsync("NewPosition", newPlayer, id, position, pieceCount, false);
               
               
                await Clients.Client(myConnectionId).SendAsync("SelectedPostions",selectedPostions);
                await Clients.Caller.SendAsync("StopTimer");
                _sharedDb.MoveTime[gameLobbyId][playerName]= DateTime.Now;
                await Clients.Caller.SendAsync("TimerStart");

            }
            else
            {
                await Clients.Group(gameLobbyId).SendAsync("NewPosition", newPlayer, id, position, pieceCount,true);

                await Clients.Client(myConnectionId).SendAsync("StopTimer");
                _sharedDb.MoveTime[gameLobbyId][newPlayer]= DateTime.Now;
                string opponentConnectionId = AddOpponentPlayerConnectionId(gameLobbyId, playerName);
              await Clients.Client(opponentConnectionId).SendAsync("TimerStart");
                _nineMensMorrisService.SetPlayerInCountDown(newPlayer);

            }

            if(_nineMensMorrisService.GetPlayer2PieceCount() == 0)
            {
                redPieceInBoard = _nineMensMorrisService.PieceCount('0', myboard);
                greenPieceInBoard = _nineMensMorrisService.PieceCount('1', myboard);

               int redplayerPhase = _nineMensMorrisService.CheckGamePhase(redPieceInBoard);
               int greenplayerPhase = _nineMensMorrisService.CheckGamePhase(greenPieceInBoard);

                if (redplayerPhase == greenplayerPhase)
                    await Clients.Group(gameLobbyId).SendAsync("ChangePhase", redplayerPhase);
                else
                {
                    await Clients.Client(myConnectionId).SendAsync("ChangePhase", greenplayerPhase);
                    string opponentPlayerConnectionId = AddOpponentPlayerConnectionId(gameLobbyId, playerName);

                    if(opponentPlayerConnectionId != "No opponent player.")
                        await Clients.Client(opponentPlayerConnectionId).SendAsync("ChangePhase", redplayerPhase);
                }

                if(redplayerPhase == greenplayerPhase && greenplayerPhase == 2)
                {
                  
                      bool existPieceMutableRed = _nineMensMorrisService.NotHaveMutablePiece(myboard, '0');

                    if (existPieceMutableRed == false )
                    {
                        _nineMensMorrisService.SetWinnerExist(true);
                        _nineMensMorrisService.SetWinnerPlayer(playerName);
                        List<Player> gameLobbyData = GetLobbyData(gameLobbyId);
                        List<Player> newScores = _gameScoreCalculatorService.CalculateScores(Guid.Parse(gameLobbyId), playerName, gameLobbyData);
                        await Clients.Group(gameLobbyId).SendAsync("AddWinnerPlayer", playerName);
                        await Clients.Group(gameLobbyId).SendAsync("StopTimer");
                        await Clients.Group(gameLobbyId).SendAsync("UpdateLobby", newScores);
                        await _gameScoreRepository.ChangeScoreInDatabase(newScores, "Nine Men's Morris");

                    }
                }
            }

            return true;
     }

        public bool VerifyPieceForDelete(string position, List<string> selectablePositions)
        {
            foreach (string selectablePosition in selectablePositions)
            {
                if (selectablePosition == position)
                    return true;
            }

            return false;

        }

        public bool ChangePiecePostionInList(ConcurrentDictionary<string, string> postionList, string id, string postion)
        {
            foreach (var myPostion in postionList)
            {

                if (myPostion.Key == id)
                {
                    postionList[myPostion.Key] = postion;
                    return true;
                }
            }
            return false;
        }



        public bool RemovePiecePostionInList(ConcurrentDictionary<string, string> postionList, string id)
        {
          if(postionList.TryRemove(id, out var myPostion))
            {
                return true;
            }
            return false;
        }


        public async Task<bool> RemovePiece(string gameLobbyId, string playerName ,string position, string id, int myGamePhase, List<string> selectablePositions)
        {
            int row = int.Parse(position[0].ToString());
            int col = int.Parse(position[1].ToString());
            int oppenentPieceCountInBoard;
            bool existPieceMutable;

            bool checkMyPostion = VerifyPieceForDelete(position, selectablePositions);

            if (checkMyPostion == false)
                return false;

            BoardNineMens myboard = _sharedDb.ninemensgame[gameLobbyId];
        
            if (myboard.Board[row,col] == '-')
            {
                return false;
               
            }

            myboard.Board[row, col] = '-';

            RemovePiecePostionInList(_sharedDb.PiecePositionList, id);
            string newPlayer = _nineMensMorrisService.ChangeCurrentPlayer(playerName);
            await Clients.Group(gameLobbyId).SendAsync("DeletePosition", id, newPlayer);
            await Clients.Caller.SendAsync("StopTimer"); //veszely lehet
            _sharedDb.MoveTime[gameLobbyId][newPlayer]= DateTime.Now;
            string opponentConnectionId = AddOpponentPlayerConnectionId(gameLobbyId, playerName);
            await Clients.Client(opponentConnectionId).SendAsync("TimerStart");
            _nineMensMorrisService.SetPlayerInCountDown(newPlayer);
            _nineMensMorrisService.SetHaveMillsBeforeRefresh(false);
            _nineMensMorrisService.ClearInRefreshSelectedPostion();

            if(myGamePhase == 2 || myGamePhase == 3)
            {
                if(myGamePhase == 2)
                {
                    if (int.Parse(id.ToString())<10)
                        existPieceMutable = _nineMensMorrisService.NotHaveMutablePiece(myboard, '0');
                    else
                        existPieceMutable = _nineMensMorrisService.NotHaveMutablePiece(myboard, '1');

                    if(existPieceMutable == false)
                    {
                        List<Player> gameLobbyData = GetLobbyData(gameLobbyId);
                        List<Player> newScores = _gameScoreCalculatorService.CalculateScores(Guid.Parse(gameLobbyId), playerName, gameLobbyData);
                        await Clients.Group(gameLobbyId).SendAsync("AddWinnerPlayer", playerName);
                        _nineMensMorrisService.SetWinnerPlayer(playerName);
                        await Clients.Group(gameLobbyId).SendAsync("UpdateLobby", newScores);
                        await _gameScoreRepository.ChangeScoreInDatabase(newScores, "Nine Men's Morris");


                    }

                }

                oppenentPieceCountInBoard = AddOpponentPlayerPieceCount(int.Parse(id.ToString()), myboard);
               
                 
                if (oppenentPieceCountInBoard == 3)
                {
                   int newGamePhaseForOpponentPlayer = _nineMensMorrisService.CheckGamePhase(oppenentPieceCountInBoard);
                   string opponentPlayerConnectionId = AddOpponentPlayerConnectionId(gameLobbyId, playerName);
                    if (opponentPlayerConnectionId != "No opponent player.")
                        await Clients.Client(opponentPlayerConnectionId).SendAsync("ChangePhase", newGamePhaseForOpponentPlayer);
                }
                if (oppenentPieceCountInBoard < 3)
                {
                    _nineMensMorrisService.SetWinnerExist(true);
                    _nineMensMorrisService.SetWinnerPlayer(playerName);
                    List<Player> gameLobbyData = GetLobbyData(gameLobbyId);
                    List<Player> newScores = _gameScoreCalculatorService.CalculateScores(Guid.Parse(gameLobbyId), playerName, gameLobbyData);
                    await Clients.Group(gameLobbyId).SendAsync("AddWinnerPlayer", playerName);
                    await Clients.Group(gameLobbyId).SendAsync("StopTimer");
                    await Clients.Group(gameLobbyId).SendAsync("UpdateLobby", newScores);
                    await _gameScoreRepository.ChangeScoreInDatabase(newScores, "Nine Men's Morris");
                }

            }
           
            return true;
        }

        public async Task<bool> MovePiece(string gameLobbyId, string playerName, string position, string id, string previousPosition)
        {
            int prevPositionRow = int.Parse(previousPosition[0].ToString());
            int prevPositionCol = int.Parse(previousPosition[1].ToString());
            int nextPositionRow = int.Parse(position[0].ToString());
            int nextPositionCol = int.Parse(position[1].ToString());
            bool existMutablePieceForOpponent;
            int myPieceId = int.Parse(id);
            string myColorIs;
            List<string> selectedPostions;
            if (myPieceId<10)
            {
                myColorIs = "red";
            }
            else
            {
                myColorIs = "green";
            }

           List<string> neighbourPostions = _nineMensMorrisService.NieghbourPostions(prevPositionRow, prevPositionCol);
           BoardNineMens myboard = _sharedDb.ninemensgame[gameLobbyId];
           List<string> checkedNeighbourPostions = _nineMensMorrisService.CheckedNieghbourPostions(myboard, neighbourPostions);
           bool checkMove = _nineMensMorrisService.ValidMove(checkedNeighbourPostions, position);

            if(!checkMove)
            {
                return false;
            }

            ChangePiecePostionInList(_sharedDb.PiecePositionList, id, position);

            if (myColorIs == "red")
                _nineMensMorrisService.SetPiece(nextPositionRow, nextPositionCol, prevPositionRow, prevPositionCol, myboard, '0');
            else
                _nineMensMorrisService.SetPiece(nextPositionRow, nextPositionCol, prevPositionRow, prevPositionCol, myboard, '1');



            string newPlayer = _nineMensMorrisService.GetCurrentPlayer();
            string myConnectionId = _sharedDb.Playerconn[gameLobbyId][playerName];

            bool isMill = _nineMensMorrisService.CheckIsMill(myboard, nextPositionRow, nextPositionCol);
            if (isMill)
            {
                _nineMensMorrisService.SetHaveMillsBeforeRefresh(true);
                if (myColorIs == "red")
                {
                    selectedPostions = _nineMensMorrisService.PositionsForSelection('1', myboard);
                }
                else
                {
                    selectedPostions = _nineMensMorrisService.PositionsForSelection('0', myboard);

                }

                _nineMensMorrisService.SetInRefreshSelectedPostion(selectedPostions);
                await Clients.Group(gameLobbyId).SendAsync("NewPosition", newPlayer, id, position, 0, false);

                await Clients.Client(myConnectionId).SendAsync("SelectedPostions", selectedPostions);
                await Clients.Caller.SendAsync("StopTimer"); //veszely lehet
                _sharedDb.MoveTime[gameLobbyId][playerName]= DateTime.Now;
                await Clients.Caller.SendAsync("TimerStart");

            }
            else
            {
           
                await Clients.Group(gameLobbyId).SendAsync("NewPosition", newPlayer, id, position, 0, true);
                await Clients.Caller.SendAsync("StopTimer"); 
                _sharedDb.MoveTime[gameLobbyId][newPlayer]= DateTime.Now;
                string opponentConnectionId = AddOpponentPlayerConnectionId(gameLobbyId, playerName);
                await Clients.Client(opponentConnectionId).SendAsync("TimerStart");
                _nineMensMorrisService.SetPlayerInCountDown(newPlayer);
                if (myPieceId<10)
                    existMutablePieceForOpponent = _nineMensMorrisService.NotHaveMutablePiece(myboard, '1');
                else
                    existMutablePieceForOpponent = _nineMensMorrisService.NotHaveMutablePiece(myboard, '0');

                if (existMutablePieceForOpponent == false)
                {
                    _nineMensMorrisService.SetWinnerExist(true);
                    _nineMensMorrisService.SetWinnerPlayer(playerName);
                    List<Player> gameLobbyData = GetLobbyData(gameLobbyId);
                    List<Player> newScores = _gameScoreCalculatorService.CalculateScores(Guid.Parse(gameLobbyId), playerName, gameLobbyData);
                    await Clients.Group(gameLobbyId).SendAsync("AddWinnerPlayer", playerName);
                    await Clients.Group(gameLobbyId).SendAsync("StopTimer");
                    await Clients.Group(gameLobbyId).SendAsync("UpdateLobby", newScores);
                    await _gameScoreRepository.ChangeScoreInDatabase(newScores, "Nine Men's Morris");

                }

            }

            return true;
        }


        public async Task<bool> JumpPiece(string gameLobbyId, string playerName, string position, string id, string previousPosition)
        {
            int prevPositionRow = int.Parse(previousPosition[0].ToString());
            int prevPositionCol = int.Parse(previousPosition[1].ToString());
            int nextPositionRow = int.Parse(position[0].ToString());
            int nextPositionCol = int.Parse(position[1].ToString());
            int myPieceId = int.Parse(id);
         
            string myColorIs;
            List<string> selectedPostions;
            if (myPieceId<10)
            {
                myColorIs = "red";
            }
            else
            {
                myColorIs = "green";
            }

            BoardNineMens myboard = _sharedDb.ninemensgame[gameLobbyId];

            if (myboard.Board[nextPositionRow, nextPositionCol] == '-')
            {
                myboard.Board[prevPositionRow, prevPositionCol] = '-';

                if(myColorIs == "red")
                    myboard.Board[nextPositionRow, nextPositionCol] = '0';
                else
                    myboard.Board[nextPositionRow, nextPositionCol] = '1';
            }

            ChangePiecePostionInList(_sharedDb.PiecePositionList, id, position);

            string newPlayer = _nineMensMorrisService.ChangeCurrentPlayer(playerName);
            string myConnectionId = _sharedDb.Playerconn[gameLobbyId][playerName];

            bool isMill = _nineMensMorrisService.CheckIsMill(myboard, nextPositionRow, nextPositionCol);
            if (isMill)
            {
                _nineMensMorrisService.SetHaveMillsBeforeRefresh(true);
                if (myColorIs == "red")
                {
                    selectedPostions = _nineMensMorrisService.PositionsForSelection('1', myboard);
                }
                else
                {
                    selectedPostions = _nineMensMorrisService.PositionsForSelection('0', myboard);

                }
                _nineMensMorrisService.SetInRefreshSelectedPostion(selectedPostions);
                await Clients.Group(gameLobbyId).SendAsync("NewPosition", newPlayer, id, position, 0, false);

                await Clients.Client(myConnectionId).SendAsync("SelectedPostions", selectedPostions);
                await Clients.Caller.SendAsync("StopTimer"); //veszely lehet
                _sharedDb.MoveTime[gameLobbyId][playerName]= DateTime.Now;
                await Clients.Caller.SendAsync("TimerStart");

            }
            else
            {
             
                await Clients.Group(gameLobbyId).SendAsync("NewPosition", newPlayer, id, position, 0, true);
                await Clients.Caller.SendAsync("StopTimer"); //veszely lehet
                _sharedDb.MoveTime[gameLobbyId][newPlayer]= DateTime.Now;
                string opponentConnectionId = AddOpponentPlayerConnectionId(gameLobbyId, playerName);
                await Clients.Client(opponentConnectionId).SendAsync("TimerStart");
                _nineMensMorrisService.SetPlayerInCountDown(newPlayer);



            }

            return true;

        }


        


        public async Task GetCurrentBoard(string gameLobbyId, string userName)
        {
            string myColorIs;
            List<Message> lobbyMessages = new List<Message>();
            ConcurrentDictionary<string,string> myboardState = _sharedDb.PiecePositionList;
            string currentPlayer = _nineMensMorrisService.GetCurrentPlayer();
            int phase = _nineMensMorrisService.GetCurrentGamePhase();
            List<Player> gamelobbyData = GetLobbyData(gameLobbyId);
            string firstPlayerUserName = _nineMensMorrisService.GetPlayer1Name();
            string secondPlayerUserName = _nineMensMorrisService.GetPlayer2Name();
            List<Player> orderGamelobbyData = OrderPlayerList(gamelobbyData, firstPlayerUserName);
            int pieceCountRed = _nineMensMorrisService.GetPlayer1PieceCount();
            int pieceCountGreen = _nineMensMorrisService.GetPlayer2PieceCount();

            if(firstPlayerUserName == userName)
            {
                 myColorIs = "red";
            }
            else if (secondPlayerUserName == userName)
            {
                myColorIs = "green";
            }
            else
            {
                throw new Exception("Problem");
            }

            if (_sharedDb.Messages.ContainsKey(gameLobbyId))
            {
                lobbyMessages = _sharedDb.Messages[gameLobbyId];

            }


            if (_nineMensMorrisService.GetHaveMillsBeforeRefresh() == true)
            {
                await Clients.Caller.SendAsync("Refresh", myboardState, userName, phase, orderGamelobbyData, pieceCountRed, pieceCountGreen, myColorIs,false);
                await Clients.Caller.SendAsync("SelectedPostions", _nineMensMorrisService.GetInRefreshSelectedPostion());
                await Clients.Caller.SendAsync("RefreshMessages", lobbyMessages);
                RefreshCountDown(gameLobbyId, userName);
            }
           else if(_nineMensMorrisService.GetWinnerExist() == true)
            {
                string winnerPlayerName = _nineMensMorrisService.GetWinnerPlayer();
                await Clients.Caller.SendAsync("Refresh", myboardState, winnerPlayerName, phase, orderGamelobbyData, pieceCountRed, pieceCountGreen, myColorIs, true);
                await Clients.Caller.SendAsync("RefreshMessages", lobbyMessages);
            }
            else
            {
                await Clients.Caller.SendAsync("Refresh", myboardState, currentPlayer, phase, orderGamelobbyData, pieceCountRed, pieceCountGreen, myColorIs, false);
                await Clients.Caller.SendAsync("RefreshMessages", lobbyMessages);
                RefreshCountDown(gameLobbyId, userName);
            }


        }

        public async Task RefreshCountDown(string gameLobbyId, string userName)
        {

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
                if (userName == _nineMensMorrisService.GetPlayerInCountDown())
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

        public List<Player> OrderPlayerList(List<Player> gamelobbyData, string firstPlayerUserName)
        {
            List<Player> orderPlayer = new List<Player>();
            if (gamelobbyData[0].UserName == firstPlayerUserName)
            {
                orderPlayer.Add(gamelobbyData[0]);
            }
            else
            {
                orderPlayer.Add(gamelobbyData[1]);
            }

            if(orderPlayer.Count == 1)
            {
                if (orderPlayer[0].UserName == gamelobbyData[0].UserName)
                    orderPlayer.Add(gamelobbyData[1]);
                
                else
                    orderPlayer.Add(gamelobbyData[0]);
            }

            return orderPlayer;
        }

    }
}
