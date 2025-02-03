namespace CornerStore.Models.DTOs;

public class OrderDTO
{
    public int Id { get; set; }
    
    // Need both for test verification
    public int CashierId { get; set; }  // Original ID for reference
    public string CashierName { get; set; }  // Flattened display name
    
    public DateTime PaidOnDate { get; set; }
    
    // Test checks OrderProducts directly
    public List<OrderProductDTO> OrderProducts { get; set; }  
    
    // Total is checked in test
    public decimal Total { get; set; }
} 