using CornerStore.Models;

namespace CornerStore.Models.DTOs;

public static class DTOMapper
{
    // Extension method for Product
    public static ProductDTO ToDTO(this Product product)
    {
        return new ProductDTO
        {
            Id = product.Id,
            ProductName = product.ProductName,
            Brand = product.Brand,
            Price = product.Price,
            Category = product.Category,
            CategoryName = product.Category?.CategoryName
        };
    }

    // Extension method for Order
    public static OrderDTO ToDTO(this Order order)
    {
        Console.WriteLine("Converting Order to DTO");
        Console.WriteLine($"Order Total before conversion: {order.Total}");
        
        var dto = new OrderDTO
        {
            Id = order.Id,
            CashierId = order.CashierId,
            CashierName = $"{order.Cashier?.FirstName} {order.Cashier?.LastName}",
            PaidOnDate = order.PaidOnDate,
            OrderProducts = order.OrderProducts.Select(op => new OrderProductDTO
            {
                ProductName = op.Product?.ProductName,
                Brand = op.Product?.Brand,
                Price = op.Product?.Price ?? 0m,
                Quantity = op.Quantity,
                Subtotal = (op.Product?.Price ?? 0m) * op.Quantity
            }).ToList(),
            Total = order.Total
        };
        
        Console.WriteLine($"DTO Total after conversion: {dto.Total}");
        return dto;
    }

    // Extension method for Cashier
    public static CashierDTO ToDTO(this Cashier cashier)
    {
        return new CashierDTO
        {
            Id = cashier.Id,
            FirstName = cashier.FirstName,
            LastName = cashier.LastName,
            FullName = $"{cashier.FirstName} {cashier.LastName}",
            OrderCount = cashier.Orders.Count
        };
    }
} 