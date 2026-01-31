using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using DailyManagementSystem.Models;

namespace DailyManagementSystem.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<DailySpent> DailySpents { get; set; }
        public DbSet<User> Users { get; set; }

        public string DbPath { get; private set; } = string.Empty;

        // Default constructor (required for some scenarios/tools)
        public AppDbContext()
        {
            InitDbPath();
        }

        // Constructor for DI
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            InitDbPath();
        }

        private void InitDbPath()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var path = Path.Combine(folder, "DailyManagementSystem");
            Directory.CreateDirectory(path);
            DbPath = Path.Combine(path, "daily_management.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseSqlite($"Data Source={DbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // --- Clients ---
            modelBuilder.Entity<Client>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            modelBuilder.Entity<Client>()
                .Property(c => c.IsActive)
                .HasDefaultValue(true);

            // --- Orders ---
            modelBuilder.Entity<Order>()
                .Property(o => o.OrderAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Client)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.ClientId)
                .OnDelete(DeleteBehavior.Restrict); // FK_Orders_Clients ON DELETE RESTRICT

            // --- Payments ---
            modelBuilder.Entity<Payment>()
                .Property(p => p.AmountReceived)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Client)
                .WithMany(c => c.Payments)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Restrict); // FK_Payments_Clients ON DELETE RESTRICT

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany() // Assuming Order does not have a Payments collection for now (not in model)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.SetNull); // FK_Payments_Orders ON DELETE SET NULL

            // --- DailySpents ---
            modelBuilder.Entity<DailySpent>()
                .Property(d => d.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DailySpent>()
                .Property(d => d.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<DailySpent>()
                .Property(d => d.Category)
                .HasDefaultValue("General");

            // --- Users ---
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
