public class CashierDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    // Flattened: Combines FirstName + LastName for convenience
    public string FullName { get; set; }
    
    // Flattened: Instead of full Orders list, just show count
    // Reduces data size and complexity
    public int OrderCount { get; set; }
} 