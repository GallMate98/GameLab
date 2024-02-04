using System.Numerics;

namespace GameLab.Models
{
    public class Lobby
    {
        public Guid Id { get; set; }
       
        public List<Player> Players { get; set; } = new List<Player>();

        public int LobbyScore { get; set; } = 0;

        public Guid GameId { get; set; }
    }
}
