using Microsoft.EntityFrameworkCore;
using HazelnutVeb.Models;

namespace HazelnutVeb.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Explicitly match case for PostgreSQL tables
            modelBuilder.Entity<Client>().ToTable("Clients");
            modelBuilder.Entity<Sale>().ToTable("Sales");
            modelBuilder.Entity<Expense>().ToTable("Expenses");
            modelBuilder.Entity<Inventory>().ToTable("Inventory");
            modelBuilder.Entity<Reservation>().ToTable("Reservations");
            modelBuilder.Entity<User>().ToTable("Users");

        }
    }
}
