using GameLab.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace GameLab.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DbSet<Game> Games { get; set; }
        public DbSet<GameScores> GameScores { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
             base.OnModelCreating(builder);

            var userId = "9fec45c4-50af-4869-ae9c-5eac73937545";
            var moderatorId = "909e93d6-29c9-4f1d-bf1c-0f9d915f3de0";
            var adminId = "52c1fdbc-fa8c-4692-9ef6-4f1da67820b8";

         

            var roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id = userId,
                    ConcurrencyStamp = userId,
                    Name = "User",
                    NormalizedName = "User".ToUpper(),
                },
                 new IdentityRole
                {
                    Id = moderatorId,
                    ConcurrencyStamp = moderatorId,
                    Name = "Moderator",
                    NormalizedName = "Moderator".ToUpper(),
                },
                  new IdentityRole
                {
                    Id = adminId,
                    ConcurrencyStamp = adminId,
                    Name = "Admin",
                    NormalizedName = "Admin".ToUpper(),
                }
            };
            builder.Entity<IdentityRole>().HasData(roles);

            // Seed data for the Game table
            var games = new List<Game>
            {
                new Game
                {
                    Id = Guid.NewGuid(),
                    Name = "Tic-tac-toe",
                    Url = "tic-tac-toe",
                    ImageUrl = "https://static-00.iconduck.com/assets.00/tic-tac-toe-icon-2048x2048-g58f0u84.png",
                    Description = "This is a tic-tac-toe game."
                },
                new Game
                {
                    Id = Guid.NewGuid(),
                    Name = "Nine Men's Morris",
                    Url = "nine-mens-morris",
                    ImageUrl = "https://play-lh.googleusercontent.com/y91Y53dmNXPmdy_k5KNAPzyVERChcwwH6A_ZHmBXsfrMYQfk_nlN2HLLmH1OlaLs0Q",
                    Description = "This is a Nine Men's Morris game."
                }
            };

            builder.Entity<Game>().HasData(games);

            builder.Entity<GameScores>()
              .HasOne(gs => gs.Game)
              .WithMany(g => g.GameScores)
              .HasForeignKey(gs => gs.GameId)
              .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<GameScores>()
                .HasOne(gs => gs.User)
                .WithMany(u => u.GameScores)
                .HasForeignKey(gs => gs.UserId)
                .OnDelete(DeleteBehavior.Cascade);


        }
    }
}
