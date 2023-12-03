using Microsoft.EntityFrameworkCore;
using Quantum.Models;
using Quantum.Models.DTO;

namespace Quantum.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=QuantumMessage;Username=postgres;Password=26032005");
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Friends> Friends { get; set; }
        public DbSet<TextMessage> Messages {get;set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.Friends)
                .WithMany(u => u.Users);
        }
    }
}
