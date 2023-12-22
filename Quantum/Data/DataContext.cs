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
        public DbSet<UserInfoOutput> Friends { get; set; }
        public DbSet<TextMessage> Messages {get;set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserInfoOutput>()
        .       HasKey(u => u.UserId);

            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<UserFriends>()
                .HasOne(u => u.User)
                .WithMany(u => u.Friend)
                .HasForeignKey(u => u.UserId);

            modelBuilder.Entity<UserFriends>()
                .HasOne(u => u.Friend)
                .WithMany(u => u.Users)
                .HasForeignKey(u => u.FriendId);

            modelBuilder.Entity<UserFriends>()
                .HasKey(e => new {e.UserId, e.FriendId});
        }
    }
}