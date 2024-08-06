using System.Text.Json;
using Basket.API.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace Basket.API.IntegrationTests.Database.Redis;

public class RedisDataSeeder(IDistributedCache cache)
{
    public async Task<bool> AddShoppingCartAsync(ShoppingCart shoppingCart, CancellationToken cancellationToken = default)
    {
        await cache.SetStringAsync(shoppingCart.Username, JsonSerializer.Serialize(shoppingCart), cancellationToken)
            .ConfigureAwait(false);
        
        return true;
    }
    
    
    public async Task<ShoppingCart?> GetShoppingCartAsync(string username, CancellationToken cancellationToken = default)
    {
        ShoppingCart? cachedBasketResult = null;
        var cachedBasket = await cache.GetStringAsync(username, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(cachedBasket))
            cachedBasketResult = JsonSerializer.Deserialize<ShoppingCart>(cachedBasket);

        return cachedBasketResult;
    }
    
    public async Task<bool> DeleteBasketAsync(string username, CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync(username, cancellationToken).ConfigureAwait(false);
        return true;
    }
}