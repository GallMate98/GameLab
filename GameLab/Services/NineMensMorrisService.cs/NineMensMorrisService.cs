using GameLab.Models;
using GameLab.Services.DataService;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace GameLab.Services.NineMensMorrisService.cs
{
    public class NineMensMorrisService
    {
        private static string? player1Name = null;
        private static string? player2Name = null;
        private static string? currentPlayer = null;
        private bool gameInPogres = false;
        private int player1PieceCount = 9;
        private int player2PieceCount = 9;
        private int gamePhase = 1;
        private readonly SharedDb _sharedDb;
        private List<string> refreshselectedPostion = new List<string>();
        private bool millPhase = false;
        private bool existWinner = false;
        private string winnerPlayer = "";
        private string playerInCountDown = "";
        private bool playerJoined = false;

        public NineMensMorrisService(SharedDb sharedDb)
        {
            _sharedDb = new SharedDb();
        }

        public void Reset()
        {
            player1Name = null;
            player2Name = null;
            currentPlayer = null;
            gameInPogres = false;
            player1PieceCount = 9;
            player2PieceCount = 9;
            gamePhase = 1;
            SharedDb _sharedDb;
            refreshselectedPostion = new List<string>();
            millPhase = false;
            existWinner = false;
            winnerPlayer = "";
            playerInCountDown = "";
            playerJoined = false;
        }

        public void SetPlayers(string playerUsername)
        {
            if (player1Name == null)
            {
                player1Name = playerUsername;
                currentPlayer = SetFirstCurrentPlayer(player1Name);
            }
            else
            {
                if (player2Name == null)
                {
                    player2Name = playerUsername;
                    gameInPogres = true;
                }

            }
        }

        public bool GetGameInPogres()
        {
            return gameInPogres;
        }
        public void SetGameInPogres(bool changeGameInPogres)
        {
            gameInPogres = changeGameInPogres;
        }

        public void SetPiece (int nextRow, int nextCol, int prevRow, int prevCol, BoardNineMens board, char  pieceType)
        {
            board.Board[prevRow, prevCol] = '-';
            board.Board[nextRow, nextCol] = pieceType;
            currentPlayer = ChangeCurrentPlayer(currentPlayer);
        }

        public bool CheckPosition(string userName, int row, int col, BoardNineMens board, string myColorIs)
        {
            if (userName != currentPlayer)
                return false;


            if (row < 0 || row >= 8 || col < 0 || col >= 8)
                return false;

            if (board.Board[row, col] != '-')
                return false;


            if ((userName == player1Name && myColorIs == "red") || (userName == player2Name && myColorIs == "green"))
            {
                board.Board[row, col] = (userName == player1Name) ? '0' : '1';
                if (CheckGamePhase() == 1)

                {
                    if (userName == player1Name)
                        player1PieceCount--;
                    else
                        player2PieceCount--;
                }
                currentPlayer = ChangeCurrentPlayer(currentPlayer);
                return true;
            }
            else
            {
                return false;
            }
        }



        public int CheckGamePhase(int pieceInBooard = 0)
        {
            if (player2PieceCount != 0 || pieceInBooard == 0)
                gamePhase = 1;
    
          else if( pieceInBooard  > 3)
               gamePhase = 2;
            
           else
              gamePhase = 3;
            
         return gamePhase;
 
        }

        public int GetCurrentGamePhase()
        {
            return gamePhase;
        }

        public bool GetHaveMillsBeforeRefresh()
        {
            return millPhase;
        }

        public bool GetWinnerExist()
        {
            return existWinner;
        }

        public void SetWinnerExist(bool ExistWinner)
        {
            existWinner = ExistWinner;
        }

        public string GetWinnerPlayer()
        {
            return winnerPlayer;
        }

        public void SetWinnerPlayer(string player)
        {
            winnerPlayer = player;
        }


        public bool GetPlayerJoined()
        {
            return playerJoined;
        }

        public void SetPlayerJoined(bool joined)
        {
            playerJoined = joined;
        }

        public void SetHaveMillsBeforeRefresh(bool setMillPhase)
        {
            millPhase = setMillPhase;
        }

        public List<string> GetInRefreshSelectedPostion()
        {
            return refreshselectedPostion;
        }

        public void SetInRefreshSelectedPostion(List<string> selectedPostion)
        {
            refreshselectedPostion = selectedPostion;
        }

        public void ClearInRefreshSelectedPostion()
        {
            refreshselectedPostion.Clear();
        }


        public int PieceCount(char pieceType, BoardNineMens myboard)
        {
            int counter = 0;
            for(int i=0; i<myboard.Board.GetLength(0); i++)
            {
                for(int j=0; j<myboard.Board.GetLength(1); j++)
                {
                    if (myboard.Board[i,j] == pieceType)
                    {
                        counter++;
                    }
                }
            }
            return counter;

        }

        public List<string> PositionsForSelection(char pieceType, BoardNineMens myboard)
        {
            List<string> selectedPostions = new List<string>();
            List<string> selectedPostionsOnlyPartOfMill = new List<string>();
            bool existPostionNotPartOfMill = false;

            for (int i = 0; i<myboard.Board.GetLength(0); i++)
            {
                for (int j = 0; j<myboard.Board.GetLength(1); j++)
                {
                    if (myboard.Board[i, j] ==  pieceType)
                    {
                        bool isPartOfMill = CheckIsMill(myboard, i, j);
                        string myPostion = i.ToString() + j.ToString();

                        if (!isPartOfMill)
                        {
                            selectedPostions.Add(myPostion);
                            existPostionNotPartOfMill=true;
                        }
                        else
                            selectedPostionsOnlyPartOfMill.Add(myPostion);
                    }
                }
            }

            if(existPostionNotPartOfMill)
                return selectedPostions;
            else
                return selectedPostionsOnlyPartOfMill;

        }


        public string GetPlayer1Name()
        {
            return player1Name;
        }
        public string GetPlayer2Name()
        {
            return player2Name;
        }
        public int GetPlayer1PieceCount()
        {
            return player1PieceCount;
        }

        public int GetPlayer2PieceCount()
        {
            return player2PieceCount;
        }

        public void SetPlayerInCountDown(string player)
        {
            playerInCountDown = player;
        }

        public string GetPlayerInCountDown()
        {
            return playerInCountDown;
        }

        public string ChangeCurrentPlayer(string currentPlayer)
        {
            if (currentPlayer == player1Name)
            {
                currentPlayer = player2Name;
            }
            else
            {
                currentPlayer = player1Name;
            }
            return currentPlayer;
        }

        public string GetCurrentPlayer()
        {
            return currentPlayer;
        }

        public string SetFirstCurrentPlayer(string player)
        {
            currentPlayer = player;
            return currentPlayer;
        }



        public bool CheckIsMill(BoardNineMens board, int row, int col)
        {
            if (col % 2 != 0)
            {
                if (row == 0)
                {
                    if (board.Board[row, col] == board.Board[row+1, col] && board.Board[row, col] == board.Board[row+2, col])
                    {
                        return true;
                    }
                }
                if (row == 1)
                {
                    if (board.Board[row-1, col] == board.Board[row, col] && board.Board[row, col] == board.Board[row+1, col])
                    {
                        return true;
                    }
                }
                if (row == 2)
                {
                    if (board.Board[row-2, col] == board.Board[row-1, col] && board.Board[row-1, col] == board.Board[row, col])
                    {
                        return true;
                    }

                }

                if (col != 7)
                {
                    if (board.Board[row, col-1] == board.Board[row, col] && board.Board[row, col] == board.Board[row, col+1])
                    {
                        return true;
                    }
                }
                else
                {
                    if (board.Board[row, 0] == board.Board[row, col] && board.Board[row, col] == board.Board[row, col-1])
                    {
                        return true;
                    }
                }



            }
            else
            {
                if (col == 6)
                {
                    if (board.Board[row, col] == board.Board[row, col+1] && board.Board[row, col+1] == board.Board[row, 0])
                    {
                        return true;
                    }

                    if (board.Board[row, col-2] == board.Board[row, col-1] && board.Board[row, col-1] == board.Board[row, col])
                    {
                        return true;
                    }

                }

                if (col == 0)
                {
                    if (board.Board[row, col] == board.Board[row, 7] && board.Board[row, 7] == board.Board[row, 6])
                    {
                        return true;
                    }
                    if (board.Board[row, col] == board.Board[row, col+1] && board.Board[row, col+1] == board.Board[row, col+2])
                    {
                        return true;
                    }
                }
                if (col != 6 && col != 0)
                {
                    if (board.Board[row, col] == board.Board[row, col+1] && board.Board[row, col+1] == board.Board[row, col+2])
                    {
                        return true;
                    }
                    if (board.Board[row, col-2] == board.Board[row, col-1] && board.Board[row, col-1] == board.Board[row, col])
                    {
                        return true;
                    }
                }


            }

            return false;
        }

        public List<string> NieghbourPostions(int row, int col)
        {
            List<string> myNieghbours = new List<string>();
            if (col % 2 != 0)
            {
                myNieghbours.Add(row.ToString()+(col-1).ToString());

                if (col != 7)
                {
                    myNieghbours.Add(row.ToString()+(col+1).ToString());
                }
                if (col == 7)
                {
                    myNieghbours.Add(row.ToString()+"0");
                }
                if (row % 2 != 0)
                {
                    myNieghbours.Add((row+1).ToString()+col.ToString());
                    myNieghbours.Add((row-1).ToString()+col.ToString());

                }
                if (row == 0)
                {
                    myNieghbours.Add((row+1).ToString()+col.ToString());
                }
                if (row == 2)
                {
                    myNieghbours.Add((row-1).ToString()+col.ToString());
                }
            }
            else
            {
                myNieghbours.Add(row.ToString()+(col+1).ToString());

                if (col!=0)
                    myNieghbours.Add(row.ToString()+(col-1).ToString());
                else
                    myNieghbours.Add(row.ToString()+"7");
            }

            return myNieghbours;
        }
        public List<string> CheckedNieghbourPostions(BoardNineMens board, List<string> neighbourPostions)
        {
            List<string> myCheckedNieghbours = new List<string>();
            foreach (string neighbourPostion in neighbourPostions)
            {
                int neighbourPostionRow = int.Parse(neighbourPostion[0].ToString());
                int neighbourPostionCol = int.Parse(neighbourPostion[1].ToString());
                if (board.Board[neighbourPostionRow, neighbourPostionCol] == '-')
                {
                    myCheckedNieghbours.Add(neighbourPostion);
                }

            }

            return myCheckedNieghbours;
        }

        public bool ValidMove(List<string> ChekedNeighbourPostions, string nextPostion)
        {
            foreach (string ChekedNeighbourPostion in ChekedNeighbourPostions)
            {
                if (ChekedNeighbourPostion == nextPostion)
                    return true;
            }

            return false;
        }

        public bool NotHaveMutablePiece (BoardNineMens board, char pieceType)
        {
  
            for (int i=0; i<board.Board.GetLength(0); i++)
            {
                for (int j = 0; j<board.Board.GetLength(1); j++)
                {
                    if (board.Board[i,j] == pieceType)
                    {
                        List<string> nieghbourPostions = NieghbourPostions(i, j);
                        List<string> availablePosition = CheckedNieghbourPostions(board, nieghbourPostions);
                        if(availablePosition.Count > 0)
                        {
                            return true;
                        }

                    }
                }
            }
            return false;
        }
    }
}
