using System.Globalization;
using Google.Protobuf;
using Microsoft.Extensions.Caching.Distributed;

namespace Basket.API.IntegrationTests.Database.Redis;

public class RedisDataSeeder(IDistributedCache cache)
{
    public async Task<bool> AddShoppingCartAsync(ShoppingCart shoppingCart,
        CancellationToken cancellationToken = default)
    {
        await cache.SetAsync(shoppingCart.Username, GetShoppingCartAsByteArray(shoppingCart), cancellationToken)
            .ConfigureAwait(false);

        return true;
    }

    public async Task<ShoppingCart?> GetShoppingCartAsync(string username,
        CancellationToken cancellationToken = default)
    {
        ShoppingCart? cachedBasketResult = null;
        var cachedBasket = await cache.GetAsync(username, cancellationToken).ConfigureAwait(false);
        if (cachedBasket is not null)
        {
            var cachedShoppingCart = Proto.ShoppingCart.Parser.ParseFrom(cachedBasket);
            cachedBasketResult = new ShoppingCart
            {
                Username = cachedShoppingCart.Username,
                Items = cachedShoppingCart.ShoppingCartItem.Select(x => new ShoppingCartItem
                {
                    Color = x.Color,
                    ProductName = x.ProductName,
                    Price = decimal.Parse(x.Price),
                    ProductId = x.ProduceId,
                    Quantity = x.Quantity
                }).ToList(),
                Version = cachedShoppingCart.Version
            };
        }

        return cachedBasketResult;
    }

    public async Task<bool> DeleteBasketAsync(string username, CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync(username, cancellationToken).ConfigureAwait(false);
        return true;
    }

    private static byte[] GetShoppingCartAsByteArray(ShoppingCart basket)
    {
        var basketInDb = new Proto.ShoppingCart
        {
            Username = basket.Username,
            TotalPrice = basket.TotalPrice.ToString(CultureInfo.InvariantCulture),
            Version = basket.Version
        };
        basketInDb.ShoppingCartItem.AddRange(basket.Items.Select(x => new Proto.ShoppingCartItem
        {
            Color = x.Color,
            Price = x.Price.ToString(CultureInfo.InvariantCulture),
            ProduceId = x.ProductId?.ToString(),
            ProductName = x.ProductName,
            Quantity = x.Quantity
        }));

        return basketInDb.ToByteArray();
    }
}