using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CornerStore.Models;

public class Product
{
    public int Id { get; set; }
    //
    [Required]
    public string ProductName { get; set; }
    
    [Required]
    public string Brand { get; set; }
    //data type for the price so that it can be rounded to 2 decimal places
    [Required]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }
    // Foreign key for the category
    [Required]
    [ForeignKey("Category")]
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    // List of order products
    public List<OrderProduct> OrderProducts { get; set; } = new();
}