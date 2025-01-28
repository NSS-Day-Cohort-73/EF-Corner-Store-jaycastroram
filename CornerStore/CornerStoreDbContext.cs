using Microsoft.EntityFrameworkCore;
using CornerStore.Models;

public class CornerStoreDbContext : DbContext
{
    public DbSet<Cashier> Cashiers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }
    // Constructor for the CornerStoreDbContext class is used for dependency injection and database connection
    public CornerStoreDbContext(DbContextOptions<CornerStoreDbContext> context) : base(context)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Relationships are similar references on an ERD
        // One to many relationship between order and order product
        modelBuilder.Entity<OrderProduct>()
            .HasOne(op => op.Order)
            .WithMany(o => o.OrderProducts)
            .HasForeignKey(op => op.OrderId);

        // One to many relationship between product and order product
        modelBuilder.Entity<OrderProduct>()
            .HasOne(op => op.Product)
            .WithMany(p => p.OrderProducts)
            .HasForeignKey(op => op.ProductId);

        // One to many relationship between product and category
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);

        // One to many relationship between order and cashier
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Cashier)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CashierId);
    }
}

//The framework:
// Creates the DbContext
// Passes connection options to this constructor
// Manages the database connection lifecycle
// Injects the DbContext into our endpoints