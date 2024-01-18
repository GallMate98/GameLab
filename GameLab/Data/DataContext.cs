using GameLab.Models;
using Microsoft.EntityFrameworkCore;

namespace GameLab.Data

{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options){   }

        public DbSet<User> Users { get; set; }
    }
}
