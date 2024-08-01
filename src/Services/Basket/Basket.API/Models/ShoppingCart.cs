using Marten.Schema;
namespace Basket.API.Models;

public class ShoppingCart
{
    [Identity]
    public string Username { get; set; } = default!;
    public List<ShoppingCartItem> Items { get; set; } = new ();
    public decimal TotalPrice => Items.Sum(x => x.Price * x.Quantity);
    
    [Version]
    public int Version { get; set; } 
    
    public ShoppingCart()
    {
    }

    public ShoppingCart(string username)
    {
        Username = username;
    }
}