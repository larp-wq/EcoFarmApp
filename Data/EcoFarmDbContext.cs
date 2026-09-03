using Microsoft.EntityFrameworkCore;
using EcoFarmApp.Models;

namespace EcoFarmApp.Data
{
    public class EcoFarmDbContext : DbContext
    {
        public EcoFarmDbContext(DbContextOptions<EcoFarmDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = default!;
        public DbSet<Animal> Animals { get; set; } = default!;
        public DbSet<InventoryItem> InventoryItems { get; set; } = default!;
        public DbSet<Sale> Sales { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<InventoryItem>().HasKey(i => i.ItemID);

            modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);
            modelBuilder.Entity<Product>().Property(p => p.StockQuantity).HasPrecision(18, 2);
            modelBuilder.Entity<InventoryItem>().Property(i => i.Quantity).HasPrecision(18, 2);
            modelBuilder.Entity<Sale>().Property(s => s.Quantity).HasPrecision(18, 2);
        }
    }
}