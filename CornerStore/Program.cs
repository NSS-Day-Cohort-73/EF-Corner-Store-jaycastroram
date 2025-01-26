using CornerStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core and provides dummy value for testing
builder.Services.AddNpgsql<CornerStoreDbContext>(builder.Configuration["CornerStoreDbConnectionString"] ?? "testing");

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Cashier Endpoints
app.MapGet("/cashiers/{id}", async (CornerStoreDbContext db, int id) =>
{
    var cashier = await db.Cashiers
        .Include(c => c.Orders)
            .ThenInclude(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
        .FirstOrDefaultAsync(c => c.Id == id);

    if (cashier == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(cashier);
});

app.MapPost("/cashiers", async (CornerStoreDbContext db, Cashier cashier) =>
{
    db.Cashiers.Add(cashier);
    await db.SaveChangesAsync();
    return Results.Created($"/cashiers/{cashier.Id}", cashier);
});

// Product Endpoints
app.MapGet("/products", async (CornerStoreDbContext db, string? search) =>
{
    IQueryable<Product> query = db.Products
        .Include(p => p.Category)
        .Include(p => p.OrderProducts);

    if (!string.IsNullOrEmpty(search))
    {
        search = search.ToLower();
        query = query.Where(p => p.ProductName.ToLower().Contains(search) || 
                                p.Brand.ToLower().Contains(search) ||
                                p.Category.CategoryName.ToLower().Contains(search));
    }

    return await query.ToListAsync();
});

app.MapPost("/products", async (CornerStoreDbContext db, Product product) =>
{
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/products/{product.Id}", product);
});

app.MapPut("/products/{id}", async (CornerStoreDbContext db, int id, Product product) =>
{
    if (id != product.Id)
    {
        return Results.BadRequest();
    }

    db.Entry(product).State = EntityState.Modified;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Order Endpoints
app.MapGet("/orders", async (CornerStoreDbContext db, string? orderDate) =>
{
    IQueryable<Order> query = db.Orders
        .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
        .Include(o => o.Cashier);

    if (!string.IsNullOrEmpty(orderDate) && DateTime.TryParse(orderDate, out DateTime date))
    {
        query = query.Where(o => o.PaidOnDate.Date == date.Date);
    }

    return await query.ToListAsync();
});

app.MapGet("/orders/{id}", async (CornerStoreDbContext db, int id) =>
{
    var order = await db.Orders
        .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
        .Include(o => o.Cashier)
        .FirstOrDefaultAsync(o => o.Id == id);

    if (order == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(order);
});

app.MapPost("/orders", async (CornerStoreDbContext db, Order order) =>
{
    // Load and attach products for each order product
    foreach (var orderProduct in order.OrderProducts)
    {
        var product = await db.Products.FindAsync(orderProduct.ProductId);
        if (product != null)
        {
            orderProduct.Product = product;
            db.Entry(product).State = EntityState.Unchanged;
        }
    }

    db.Orders.Add(order);
    await db.SaveChangesAsync();

    // Reload the order with all related data
    var savedOrder = await db.Orders
        .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
        .Include(o => o.Cashier)
        .FirstOrDefaultAsync(o => o.Id == order.Id);

    return Results.Created($"/orders/{order.Id}", savedOrder);
});

app.MapDelete("/orders/{id}", async (CornerStoreDbContext db, int id) =>
{
    var order = await db.Orders.FindAsync(id);
    if (order == null)
    {
        return Results.NotFound();
    }

    db.Orders.Remove(order);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

//don't move or change this!
public partial class Program { }