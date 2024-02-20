namespace GameLab.Models
{
    public class BoardNineMens
    {
        public char[,] Board { get; set; }

        public BoardNineMens()
        {
            Board = new char[3, 8];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Board[i, j] = '-';
                }
            }
        }
    }
}
