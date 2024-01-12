using Microsoft.EntityFrameworkCore;
using Quantum.GroupFolder.Models;
using Quantum.Models.DTO;
using Quantum.UserP.Models;

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
        public DbSet<UserInfoOutput> OpenUsers { get; set; }
        public DbSet<TextMessage> Messages {get;set;}
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupUserRole> GroupUserRole { get; set; }
        public DbSet<GroupRequest> GroupRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserInfoOutput>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<Group>()
                .HasKey(u => u.GroupId);

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

            modelBuilder.Entity<UserGroups>()
                .HasOne(u => u.User)
                .WithMany(u => u.Groups)
                .HasForeignKey(u => u.UserId);

            modelBuilder.Entity<UserGroups>()
                .HasOne(u => u.Group)
                .WithMany(u => u.Members)
                .HasForeignKey(u => u.GroupId);

            modelBuilder.Entity<UserGroups>()
                .HasKey(e => new { e.UserId, e.GroupId });

            modelBuilder.Entity<User>()
                .HasMany(u => u.Roles)
                .WithOne()
                .HasForeignKey(u => u.UserId);

            modelBuilder.Entity<GroupUserRole>()
                .HasKey(e => new { e.UserId, e.GroupId });

            modelBuilder.Entity<Group>()
                .HasOne(g => g.GroupRequest)
                .WithOne(r => r.Group)
                .HasForeignKey<Group>(g => g.GroupRequestId)
                .IsRequired();

            modelBuilder.Entity<GroupRequest>()
                .HasKey(gr => gr.GroupRequestId);

            modelBuilder.Entity<GroupRequest>()
                .HasMany(gr => gr.Users)
                .WithMany(uio => uio.GroupRequests);
        }
    }
}