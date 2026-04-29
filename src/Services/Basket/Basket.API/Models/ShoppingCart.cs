using Marten.Schema;

namespace Basket.API.Models;

public class ShoppingCart
{
    public ShoppingCart()
    {
    }

    public ShoppingCart(string username)
    {
        Username = username;
    }

    [Identity] public string Username { get; set; } = default!;

    public List<ShoppingCartItem> Items { get; set; } = [];
    public decimal TotalPrice => Items.Sum(x => x.Price * x.Quantity);

    // Set before calling the Order Service; allows safe retries if basket delete fails.
    public string? PendingCheckoutOrderId { get; set; }

    [Version] public int Version { get; set; }
}