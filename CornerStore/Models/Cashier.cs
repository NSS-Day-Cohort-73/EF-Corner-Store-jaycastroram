namespace CornerStore.Models;

public class Cashier
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<Order> Orders { get; set; } = new();
}