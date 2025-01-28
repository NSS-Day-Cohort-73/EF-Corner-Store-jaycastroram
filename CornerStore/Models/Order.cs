using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CornerStore.Models;

public class Order
{
    public int Id { get; set; }
    
    [ForeignKey("Cashier")]
    public int CashierId { get; set; }
    public Cashier Cashier { get; set; }
    
    //data type for the date and time
    [Required]
    [DataType(DataType.DateTime)]
    public DateTime PaidOnDate { get; set; }
    
    // List of order products
    public List<OrderProduct> OrderProducts { get; set; } = new();
    
    // Total price of the order
    // Total does not need set because it is a read-only property and derived from the other values
    public decimal Total
    {
        get
        {
            if (OrderProducts == null) return 0M;
            
            return OrderProducts
                .Where(op => op.Product != null)
                .Sum(op => op.Product.Price * op.Quantity);
        }
    }
}