namespace CornerStore.Models.DTOs;  // Already has access to CornerStore.Models

public class ProductDTO
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public string Brand { get; set; }
    public decimal Price { get; set; }
    
    // Keep both for flexibility
    public Category Category { get; set; }  // Can access Category directly
    public string CategoryName { get; set; }
} 