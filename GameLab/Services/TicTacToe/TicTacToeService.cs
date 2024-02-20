using GameLab.Models;
using GameLab.Services.DataService;

namespace GameLab.Services.TicTacToe
{
    public class TicTacToeService
    {

        private readonly SharedDb _sharedDb;

        public TicTacToeService(SharedDb sharedDb)
        {
            _sharedDb = sharedDb;
        }
     private static string player1Name = null;
     private static string player2Name = null;
     private static string currentPlayer = null;

        public void SetPlayers(string playerUsername)
        {
            if(player1Name == null)
            {
                player1Name = playerUsername;
                currentPlayer = player1Name;
            }
            else
            {
                if(player2Name == null)
                {
                    player2Name = playerUsername;
                }
               
            }
        }

        public bool CheckPosition(string gameLobbyId, string userName, int row, int col)
        {
            BoardX0 board = _sharedDb.X0game[gameLobbyId];
            if(userName == currentPlayer) 
            {
                if(row>=0 && row<3 && col>=0 && col<3) 
                {
                    if( board.Board[row ,col] == '-')
                    {
                        if(userName == player1Name)
                        {
                            board.Board[row, col] ='X';
                           currentPlayer = ChangeCurrentPlayer(currentPlayer);
                            return true;
                        }
                        else
                        {
                           board.Board[row, col] ='0';
                           currentPlayer = ChangeCurrentPlayer(currentPlayer);
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public string ChangeCurrentPlayer(string currentPlayer)
        {
            if(currentPlayer == player1Name)
            {
                currentPlayer = player2Name;
            }
            else
            {
                currentPlayer = player1Name;
            }
            return currentPlayer;
        }

        public string GetCurrentPlayer ()
        {
            return currentPlayer;
        }

        public string CheckWin(BoardX0 board)
        {
            
            for (int i = 0; i < 3; i++)
            {
                if (board.Board[i, 0] == 'X' && board.Board[i, 1] == 'X' && board.Board[i, 2] == 'X')
                {
                    return player1Name;
                }
                if (board.Board[0, i] == 'X' && board.Board[1, i] == 'X' && board.Board[2, i] == 'X')
                {
                    return player1Name;
                }

                if (board.Board[i, 0] == '0' && board.Board[i, 1] == '0' && board.Board[i, 2] == '0')
                {
                    return player2Name;
                }
                if (board.Board[0, i] == '0' && board.Board[1, i] == '0' && board.Board[2, i] == '0')
                {
                    return player2Name;
                }
            }


            if (board.Board[0, 0] == 'X' && board.Board[1, 1] == 'X' && board.Board[2, 2] == 'X')
            {
                return player1Name;
            }
            if (board.Board[0, 2] == 'X' && board.Board[1, 1] == 'X' && board.Board[2, 0] == 'X')
            {
                return player1Name;
            }

            if (board.Board[0, 0] == '0' && board.Board[1, 1] == '0' && board.Board[2, 2] == '0')
            {
                return player2Name;
            }
            if (board.Board[0, 2] == '0' && board.Board[1, 1] == '0' && board.Board[2, 0] == '0')
            {
                return player2Name;
            }

            return null;
        }

        public  bool CheckDraw(BoardX0 board)
        {
            for(int i=0; i<3; i++)
            {
                for(int j=0; j<3;j++)
                {
                    if (board.Board[i,j]=='-')
                    {
                        return false;
                    }
                }
            }
            return true;
        }


    }
}
