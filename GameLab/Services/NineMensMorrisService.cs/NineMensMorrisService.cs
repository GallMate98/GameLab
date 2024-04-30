using GameLab.Models;
using GameLab.Services.DataService;

namespace GameLab.Services.NineMensMorrisService.cs
{
    public class NineMensMorrisService
    {
        private static string player1Name = null;
        private static string player2Name = null;
        private static string currentPlayer = null;
        private readonly SharedDb _sharedDb;
        
        public NineMensMorrisService ()
        {
            _sharedDb = new SharedDb ();
        }

        public void SetPlayers(string playerUsername)
        {
            if (player1Name == null)
            {
                player1Name = playerUsername;
                currentPlayer = player1Name;
            }
            else
            {
                if (player2Name == null)
                {
                    player2Name = playerUsername;
                }

            }
        }

        public bool CheckPosition(string gameLobbyId, string userName, int row, int col)
        {
           BoardNineMens board = _sharedDb.ninemensgame[gameLobbyId];
            if (userName == currentPlayer)
            {
                if (row>=0 && row<8 && col>=0 && col<8)
                {
                    if (board.Board[row, col] == '-')
                    {
                        if (userName == player1Name)
                        {
                            board.Board[row, col] ='0';
                            currentPlayer = ChangeCurrentPlayer(currentPlayer);
                            return true;
                        }
                        else
                        {
                            board.Board[row, col] ='1';
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

    }
}
