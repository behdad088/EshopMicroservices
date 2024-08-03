namespace Basket.API.Data;

internal class CachedBasketRepository(IBasketRepository repository, IDistributedCache cache) : IBasketRepository
{
    public async Task<ShoppingCart> GetBasketAsync(string username, CancellationToken cancellationToken = default)
    {
        var cachedBasket = await cache.GetStringAsync(username, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(cachedBasket))
        {
           var cachedBasketResult = JsonSerializer.Deserialize<ShoppingCart>(cachedBasket);
           return cachedBasketResult!;
        }

        var basketInDb = await repository.GetBasketAsync(username, cancellationToken).ConfigureAwait(false);
        await cache.SetStringAsync(username, JsonSerializer.Serialize(basketInDb), cancellationToken)
            .ConfigureAwait(false);
        
        return basketInDb;
    }

    public async Task<ShoppingCart> StoreBasketAsync(ShoppingCart basket, CancellationToken cancellationToken = default)
    {  
        await repository.StoreBasketAsync(basket, cancellationToken).ConfigureAwait(false);
        await cache.SetStringAsync(basket.Username, JsonSerializer.Serialize(basket), cancellationToken)
            .ConfigureAwait(false);
        
        return basket;
    }

    public async Task<bool> DeleteBasketAsync(string username, CancellationToken cancellationToken = default)
    {
        var isSuccess = await repository.DeleteBasketAsync(username, cancellationToken).ConfigureAwait(false);
        await cache.RemoveAsync(username, cancellationToken).ConfigureAwait(false);
        return isSuccess;
    }
}