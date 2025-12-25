using System.ComponentModel.DataAnnotations;

namespace Discount.Grpc.Models;

public class Coupon
{
    public int Id { get; set; }
    
    [MaxLength(120)]
    public string? ProductName { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; } = null!;
    public int Amount { get; set; }
}