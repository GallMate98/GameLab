namespace GameLab.Models
{
    public class BoardX0
    {
        public char [,] Board { get; set; }

        public BoardX0()
        {
            Board = new char[3,3];

            for(int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    Board[i,j] = '-';
                }
            }
        }
    }
}
