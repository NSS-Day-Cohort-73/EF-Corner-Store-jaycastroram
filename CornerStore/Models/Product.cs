namespace CornerStore.Models;

public class Product
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public string Brand { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    public List<OrderProduct> OrderProducts { get; set; } = new();
}