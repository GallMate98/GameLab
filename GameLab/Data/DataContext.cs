using GameLab.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GameLab.Data

{
    public class DataContext : IdentityDbContext<User>
    {
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
        }
    }
}
