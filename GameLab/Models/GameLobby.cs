namespace GameLab.Models
{
    public class GameLobby
    {
        public Guid Id { get; set; }

        public Player Player1 { get; set; }

        public Player Player2 { get; set; }

        public Guid GameId { get; set; }

    }
}
