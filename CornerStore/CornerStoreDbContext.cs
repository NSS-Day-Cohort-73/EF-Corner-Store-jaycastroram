using Microsoft.EntityFrameworkCore;
using CornerStore.Models;

public class CornerStoreDbContext : DbContext
{
    public DbSet<Cashier> Cashiers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }

    public CornerStoreDbContext(DbContextOptions<CornerStoreDbContext> options)
        : base(options)
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

        // Add sample data
        modelBuilder.Entity<Cashier>().HasData(
            new Cashier { Id = 1, FirstName = "Amy", LastName = "Simpson" },
            new Cashier { Id = 2, FirstName = "Derek", LastName = "Masters" }
        );

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, CategoryName = "Food" },
            new Category { Id = 2, CategoryName = "Cleaning" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, ProductName = "Tuna", Brand = "Bumble Bee", Price = 1.25M, CategoryId = 1 },
            new Product { Id = 2, ProductName = "Toilet Paper", Brand = "Scott", Price = 5.00M, CategoryId = 2 }
        );
    }
}

//The framework:
// Creates the DbContext
// Passes connection options to this constructor
// Manages the database connection lifecycle
// Injects the DbContext into our endpoints