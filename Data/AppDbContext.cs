using Microsoft.EntityFrameworkCore;
using ResaleApi.Models;

namespace ResaleApi.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Reseller> Resellers { get; set; }
        public DbSet<ResellerPhone> ResellerPhones { get; set; }
        public DbSet<ResellerContact> ResellerContacts { get; set; }
        public DbSet<ResellerAddress> ResellerAddresses { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CustomerOrder> CustomerOrders { get; set; }
        public DbSet<CustomerOrderItem> CustomerOrderItems { get; set; }
        public DbSet<BreweryOrder> BreweryOrders { get; set; }
        public DbSet<BreweryOrderItem> BreweryOrderItems { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Reseller configuration
            modelBuilder.Entity<Reseller>(entity =>
            {
                entity.ToTable("Resellers");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Cnpj).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                
                entity.Property(e => e.Cnpj).IsRequired().HasMaxLength(14);
                entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.TradeName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            });

            // ResellerPhone configuration
            modelBuilder.Entity<ResellerPhone>(entity =>
            {
                entity.ToTable("ResellerPhones");
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.Reseller)
                    .WithMany(e => e.Phones)
                    .HasForeignKey(e => e.ResellerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ResellerContact configuration
            modelBuilder.Entity<ResellerContact>(entity =>
            {
                entity.ToTable("ResellerContacts");
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.Reseller)
                    .WithMany(e => e.Contacts)
                    .HasForeignKey(e => e.ResellerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ResellerAddress configuration
            modelBuilder.Entity<ResellerAddress>(entity =>
            {
                entity.ToTable("ResellerAddresses");
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.Reseller)
                    .WithMany(e => e.Addresses)
                    .HasForeignKey(e => e.ResellerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Volume).HasColumnType("decimal(18,2)");
            });

            // CustomerOrder configuration
            modelBuilder.Entity<CustomerOrder>(entity =>
            {
                entity.ToTable("CustomerOrders");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                
                entity.HasOne(e => e.Reseller)
                    .WithMany(e => e.CustomerOrders)
                    .HasForeignKey(e => e.ResellerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // CustomerOrderItem configuration
            modelBuilder.Entity<CustomerOrderItem>(entity =>
            {
                entity.ToTable("CustomerOrderItems");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Discount).HasColumnType("decimal(18,2)");
                
                entity.HasOne(e => e.CustomerOrder)
                    .WithMany(e => e.Items)
                    .HasForeignKey(e => e.CustomerOrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Product)
                    .WithMany(e => e.CustomerOrderItems)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // BreweryOrder configuration
            modelBuilder.Entity<BreweryOrder>(entity =>
            {
                entity.ToTable("BreweryOrders");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BreweryOrderNumber).IsUnique();
                
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                
                entity.HasOne(e => e.Reseller)
                    .WithMany(e => e.BreweryOrders)
                    .HasForeignKey(e => e.ResellerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // BreweryOrderItem configuration
            modelBuilder.Entity<BreweryOrderItem>(entity =>
            {
                entity.ToTable("BreweryOrderItems");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Discount).HasColumnType("decimal(18,2)");
                
                entity.HasOne(e => e.BreweryOrder)
                    .WithMany(e => e.Items)
                    .HasForeignKey(e => e.BreweryOrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Product)
                    .WithMany(e => e.BreweryOrderItems)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
} 