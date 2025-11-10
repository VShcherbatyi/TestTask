using Microsoft.EntityFrameworkCore;
using testtask.Models;

namespace testtask.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        protected AppDbContext() { }

        public DbSet<Dog> Dogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dog>().HasData(
                new Dog { Id = 1, Name = "Neo", Color = "red&amber", TailLength = 22, Weight = 32 },
                new Dog { Id = 2, Name = "Jessy", Color = "black&white", TailLength = 7, Weight = 14 }
            );
        }
    }
}
