﻿namespace GameLab.Models
{
    public class Game
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Url { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }

        public ICollection<GameScores>? GameScores { get; set; }
    }
}
