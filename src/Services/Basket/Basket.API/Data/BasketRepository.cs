namespace Basket.API.Data;

internal class BasketRepository(IDocumentSession session) : IBasketRepository
{
    private readonly ILogger _logger = Log.ForContext<BasketRepository>();
    public async Task<ShoppingCart?> GetBasketAsync(string username, CancellationToken cancellationToken = default)
    {
        _logger.Information("Getting basket from database");
        var basket = await session.LoadAsync<ShoppingCart>(username, cancellationToken).ConfigureAwait(false);
        return basket;
    }

    public async Task<ShoppingCart> StoreBasketAsync(ShoppingCart basket, CancellationToken cancellationToken = default)
    {
        _logger.Information("Saving basket to database");
        session.Store(basket);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return basket;
    }

    public async Task<bool> DeleteBasketAsync(string username, CancellationToken cancellationToken = default)
    {
        _logger.Information("Deleting basket from database");
        session.Delete<ShoppingCart>(username);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}