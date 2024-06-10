namespace GameLab.Models
{
    public class GameScores
    {
        public Guid Id { get; set; }

        public Guid GameId { get; set; } 

        public string UserId { get; set; }

        public int Score { get; set; }



        public Game Game { get; set; } = null!;

        public User User { get; set; } = null!;


    }
}
