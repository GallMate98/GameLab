using GameLab.Models;
using GameLab.Services.DataService;
using GameLab.Services.GameLobbyAssignment;
using GameLab.Services.NineMensMorrisService.cs;
using GameLab.Services.TicTacToe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;
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
        private  List<Player> ordergamelobbyData = new List<Player>();
       

        public NineMensMorrisHub(SharedDb sharedDb, IGameAssignmentService gameAssignmentService, NineMensMorrisService nineMensMorrisService)
        {

            _sharedDb = sharedDb;
            _gameAssignmentService = gameAssignmentService;
            _nineMensMorrisService = nineMensMorrisService;
           

        }

        public async Task JoinNineMensMorrisGameLobby(string gameLobbyId, string playerUserName)
 
        {
            if (_nineMensMorrisService.GetGameInPogres() == true && _nineMensMorrisService.GetCurrentPlayer != null)
            {
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
                    await Clients.Group(gameLobbyId).SendAsync("StartGame", starterPlayerUserName);
                    _nineMensMorrisService.SetGameInPogres(true);

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
               
            }
            else
            {
                 newPlayer = _nineMensMorrisService.GetCurrentPlayer();
                await Clients.Group(gameLobbyId).SendAsync("NewPosition", newPlayer, id, position, pieceCount,true);

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
                        await Clients.Group(gameLobbyId).SendAsync("AddWinnerPlayer", playerName);
                      
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
                        await Clients.Group(gameLobbyId).SendAsync("AddWinnerPlayer", playerName);
                        _nineMensMorrisService.SetWinnerPlayer(playerName);
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
                    await Clients.Group(gameLobbyId).SendAsync("AddWinnerPlayer", playerName);
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

            }
            else
            {
           
                await Clients.Group(gameLobbyId).SendAsync("NewPosition", newPlayer, id, position, 0, true);
                if (myPieceId<10)
                    existMutablePieceForOpponent = _nineMensMorrisService.NotHaveMutablePiece(myboard, '1');
                else
                    existMutablePieceForOpponent = _nineMensMorrisService.NotHaveMutablePiece(myboard, '0');

                if (existMutablePieceForOpponent == false)
                {
                    _nineMensMorrisService.SetWinnerExist(true);
                    _nineMensMorrisService.SetWinnerPlayer(playerName);
                    await Clients.Group(gameLobbyId).SendAsync("AddWinnerPlayer", playerName);
                   
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

            }
            else
            {
             
                await Clients.Group(gameLobbyId).SendAsync("NewPosition", newPlayer, id, position, 0, true);

                
            }

            return true;

        }


        //public override async Task OnConnectedAsync()
        //{
        //    await base.OnConnectedAsync();

        //    string userName = Context.User.FindFirstValue(ClaimTypes.Name);

        //    // Ellenőrizzük, hogy a felhasználó már csatlakozott-e korábban, és frissítjük a játék táblát
        //    await RefreshBoardForUser(userName);
        //}

        //public override async Task OnDisconnectedAsync(Exception exception)
        //{
        //    await base.OnDisconnectedAsync(exception);

        //    string userName = Context.User.FindFirstValue(ClaimTypes.Name);

        //    // Felhasználó lecsatlakozásakor is frissítjük a játék táblát
        //    await RefreshBoardForUser(userName);
        //}

        //private async Task RefreshBoardForUser(string userName)
        //{
        //    // Ellenőrizzük, hogy a felhasználó csatlakozott-e valamelyik játékhoz
        //    if (_sharedDb.Playerconn.Any(entry => entry.Value.ContainsKey(userName)))
        //    {
        //        foreach (var gameLobbyId in _sharedDb.Playerconn.Keys)
        //        {
        //            if (_sharedDb.Playerconn[gameLobbyId].ContainsKey(userName))
        //            {
        //                // Ha a felhasználó már csatlakozott egy játékhoz, frissítjük a játék táblát
        //                await GetCurrentBoard(gameLobbyId, userName);
        //                break;
        //            }
        //        }
        //    }
        //}


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
