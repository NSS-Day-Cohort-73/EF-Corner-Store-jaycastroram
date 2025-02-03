using CornerStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using CornerStore.Models.DTOs;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["CornerStoreDbConnectionString"];
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core
builder.Services.AddNpgsql<CornerStoreDbContext>(builder.Configuration["CornerStoreDbConnectionString"]);

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

// Cashier Endpoints, these are the endpoints for the cashier model and its related data 
app.MapGet("/cashiers/{id}", async (CornerStoreDbContext db, int id) =>
{
    // Get cashier with all related data, the ? operator is used to return null if the cashier doesn't exist 
    // the include method is used to include the related data in the query in this case we are including the orders and the order products
    Cashier? cashier = await db.Cashiers
        .Include(c => c.Orders)
            .ThenInclude(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                // the FirstOrDefaultAsync method is used to return the first cashier that matches the id
        .FirstOrDefaultAsync(c => c.Id == id);

    // Return NotFound if cashier doesn't exist
    if (cashier == null)
    {
        // Return the status code 404 if the cashier doesn't exist
        return Results.NotFound();
    }
    // Return the cashier with all its related data that we included in the query and the status code 200
    return Results.Ok(cashier);
});
// Create a new cashier using the POST method using the Cashier model as the request body and db as the database context
// The Cashier model is the data that we are sending to the endpoint and the db is the database context that we are using to save the data 
app.MapPost("/cashiers", async (CornerStoreDbContext db, Cashier cashier) =>
{
    // Add new cashier
    db.Cashiers.Add(cashier);
    // Save the changes to the database using the async method
    // The await keyword is used to wait for the SaveChangesAsync method to complete before returning the results
    await db.SaveChangesAsync();
    Cashier savedCashier = await db.Cashiers
        .Where(c => c.Id == cashier.Id)
        .Include(c => c.Orders)
        .FirstOrDefaultAsync();
    // The Created method is used to return the created cashier with the status code 201
    // The Results.Created method takes two arguments, the first is the URL of the created cashier and the second is the data that we are sending to the endpoint
    return Results.Created($"/cashiers/{cashier.Id}", savedCashier.ToDTO());
});

// Product Endpoints, these are the endpoints for the product model and its related data    
app.MapGet("/products", async (CornerStoreDbContext db, string? search) =>
{
    var products = await db.Products
        .Include(p => p.Category)
        .Include(p => p.OrderProducts)
        .ToListAsync();

    // If search term provided, filter the results
    if (!string.IsNullOrEmpty(search))
    {
        products = products
            .Where(p => 
                p.ProductName.Contains(search, StringComparison.OrdinalIgnoreCase) || 
                p.Brand.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                p.Category.CategoryName.Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    return Results.Ok(products.Select(p => p.ToDTO()));
});
// Create a new product using the POST method using the Product model as the request body and db as the database context
// The Product model is the data that we are sending to the endpoint and the db is the database context that we are using to save the data   
app.MapPost("/products", async (CornerStoreDbContext db, Product product) =>
{
    // Add new product
    db.Products.Add(product);
    // Save the changes to the database using the async method
    // The await keyword is used to wait for the SaveChangesAsync method to complete before returning the results
    await db.SaveChangesAsync();
    
    // Load the category before converting to DTO
    Product savedProduct = await db.Products
        .Where(p => p.Id == product.Id)
        .Include(p => p.Category)
        .FirstOrDefaultAsync();
    // The $"/products/{product.Id}" is the URL of the created product
    // The Results.Created method takes two arguments, the first is the URL of the created product and the second is the data that we are sending to the endpoint and the status code 201   
    //return Results.Created($"/products/{product.Id}", product);
    return Results.Created($"/products/{product.Id}", savedProduct.ToDTO());
});
// Update a product using the PUT method using the Product model as the request body and db as the database context
// The Product model is the data that we are sending to the endpoint and the db is the database context that we are using to save the data   
app.MapPut("/products/{id}", async (CornerStoreDbContext db, int id, Product product) =>
{
    // Check if IDs match, the id is the id of the product that we are updating and the product.Id is the id of the product that we are sending to the endpoint
    // If the IDs don't match, return the status code 400
    if (id != product.Id)
    {
        return Results.BadRequest("URL id does not match product id");
    }
    // Find the existing product, the ? operator is used to return null if the product doesn't exist 
    // the include method is used to include the related data in the query in this case we are including the category
    // the FirstOrDefaultAsync method is used to return the first product that matches the id
    Product? existingProduct = await db.Products
        .Include(p => p.Category)
        .FirstOrDefaultAsync(p => p.Id == id);

    // If product doesn't exist, return 404
    if (existingProduct == null)
    {
        return Results.NotFound($"Product with ID {id} not found");
    }
    // Update the existing product properties
    // The ProductName property is updated with the value from the product that we are sending to the endpoint
    existingProduct.ProductName = product.ProductName;
    // The Brand property is updated with the value from the product that we are sending to the endpoint
    existingProduct.Brand = product.Brand;
    // The Price property is updated with the value from the product that we are sending to the endpoint
    existingProduct.Price = product.Price;
    // The CategoryId property is updated with the value from the product that we are sending to the endpoint
    existingProduct.CategoryId = product.CategoryId;
    // Save the changes to the database using the async method
    // The await keyword is used to wait for the SaveChangesAsync method to complete before returning the results       
    await db.SaveChangesAsync();
    // Return the status code 204 if the product is updated
    // The NoContent method is used to return the status code 204
    return Results.NoContent();
});
// Order Endpoints, these are the endpoints for the order model and its related data
// Get all orders with their related data, the orderDate parameter is optional and is used to filter the orders by the order date   
// The orderDate parameter is a string that is used to filter the orders by the order date
app.MapGet("/orders", async (CornerStoreDbContext db, string? orderDate) =>
{
    // Get all orders with their related data, the ? operator is used to return null if the order doesn't exist 
    // the include method is used to include the related data in the query in this case we are including the order products and the cashier
    List<Order> orders = await db.Orders
        .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
        .Include(o => o.Cashier)
        .ToListAsync();

    // If date provided, filter orders by that date
    // The string.IsNullOrEmpty method is used to check if the orderDate is null or empty
    if (!string.IsNullOrEmpty(orderDate) && DateTime.TryParse(orderDate, out DateTime date))
    // The DateTime.TryParse method is used to parse the orderDate to a DateTime object
    // The out keyword is used to pass the parsed DateTime object to the date variable
    {
        orders = orders
            .Where(o => o.PaidOnDate.Date == date.Date)
    // The Where method is used to filter the orders by the order date
    // The ToList method is used to return a list of orders
            .ToList();
    }
    // Return the orders with the status code 200
    // The Ok method is used to return the orders with the status code 200
    return Results.Ok(orders.Select(o => o.ToDTO()));
    // The Select method is used to return a list of orders with the ToDTO method applied to each order
});
// Get a specific order with its related data, the id parameter is the id of the order that we are getting 
app.MapGet("/orders/{id}", async (CornerStoreDbContext db, int id) =>
{
    // Get specific order with all related data, the ? operator is used to return null if the order doesn't exist 
    Order? order = await db.Orders
    // the include method is used to include the related data in the query in this case we are including the order products and the cashier
        // the FirstOrDefaultAsync method is used to return the first order that matches the id
        .Include(o => o.OrderProducts)
            // the ThenInclude method is used to include the related data in the query. In this case we are including the product due to the relationship between the order product and the product
            .ThenInclude(op => op.Product)
        .Include(o => o.Cashier)
        .FirstOrDefaultAsync(o => o.Id == id);

    // Return NotFound if order doesn't exist
    if (order == null)
    {   
        // Return the status code 404 if the order doesn't exist
        return Results.NotFound();
    }
    // Return the order with the status code 200
    return Results.Ok(order);
});
// Create a new order using the POST method using the Order model as the request body and db as the database context
// The Order model is the data that we are sending to the endpoint and the db is the database context that we are using to save the data       
app.MapPost("/orders", async (CornerStoreDbContext db, Order order) =>
{
    List<OrderProduct> orderProducts = new();
    foreach (OrderProduct orderProduct in order.OrderProducts)
    {
        Product? product = await db.Products.FindAsync(orderProduct.ProductId);
        if (product != null)
        {
            orderProducts.Add(new OrderProduct 
            { 
                ProductId = product.Id,
                Product = product,
                Quantity = orderProduct.Quantity
            });
        }
    }

    Order newOrder = new()
    {
        CashierId = order.CashierId,
        PaidOnDate = order.PaidOnDate,
        OrderProducts = orderProducts
    };

    db.Orders.Add(newOrder);
    await db.SaveChangesAsync();

    Order? savedOrder = await db.Orders
        .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
        .Include(o => o.Cashier)
        .FirstOrDefaultAsync(o => o.Id == newOrder.Id);

    return Results.Created($"/orders/{newOrder.Id}", savedOrder);
});
// Delete an order using the DELETE method using the id parameter as the id of the order that we are deleting
app.MapDelete("/orders/{id}", async (CornerStoreDbContext db, int id) =>
{
    // Find the order, the ? operator is used to return null if the order doesn't exist 
    // the FindAsync method is used to return the first order that matches the id
    Order? order = await db.Orders.FindAsync(id);
    
    // Return NotFound if order doesn't exist
    if (order == null)
    {
        // Return the status code 404 if the order doesn't exist
        return Results.NotFound();
    }

    // Remove the order
    db.Orders.Remove(order);
    // Save the changes to the database using the async method
    // The await keyword is used to wait for the SaveChangesAsync method to complete before returning the results
    await db.SaveChangesAsync();
    // The NoContent method is used to return the status code 204
    return Results.NoContent();
});

app.Run();

//don't move or change this!
public partial class Program { }