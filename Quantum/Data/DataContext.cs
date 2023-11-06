using Microsoft.EntityFrameworkCore;
using Quantum.Models;

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
    }
}
