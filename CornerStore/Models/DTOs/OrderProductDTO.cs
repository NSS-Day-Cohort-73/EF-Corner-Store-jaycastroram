public class OrderProductDTO
{
    // Flattened: Instead of Product object with all properties,
    // we only include the relevant product details
    public string ProductName { get; set; }
    public string Brand { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    
    // Computed property that combines Price * Quantity
    // Instead of making the client calculate this
    public decimal Subtotal { get; set; }
} 