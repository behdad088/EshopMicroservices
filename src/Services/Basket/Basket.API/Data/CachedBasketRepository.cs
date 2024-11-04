using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Google.Protobuf;

namespace Basket.API.Data;

internal class CachedBasketRepository(IBasketRepository repository, IDistributedCache cache) : IBasketRepository
{
    public async Task<ShoppingCart> GetBasketAsync(string username, CancellationToken cancellationToken = default)
    {
        var cachedBasket = await cache.GetAsync(username, cancellationToken).ConfigureAwait(false);
        if (cachedBasket is not null && TryParseBasket(cachedBasket, out var shoppingCart))
            return shoppingCart;

        var basketInDb = await repository.GetBasketAsync(username, cancellationToken).ConfigureAwait(false);
        var basketByteArray = GetShoppingCartAsByteArray(basketInDb);
        await cache.SetAsync(username, basketByteArray, cancellationToken)
            .ConfigureAwait(false);

        return basketInDb;
    }

    public async Task<ShoppingCart> StoreBasketAsync(ShoppingCart basket, CancellationToken cancellationToken = default)
    {
        await repository.StoreBasketAsync(basket, cancellationToken).ConfigureAwait(false);
        var basketByteArray = GetShoppingCartAsByteArray(basket);
        await cache.SetAsync(basket.Username, basketByteArray, cancellationToken).ConfigureAwait(false);

        return basket;
    }

    public async Task<bool> DeleteBasketAsync(string username, CancellationToken cancellationToken = default)
    {
        var isSuccess = await repository.DeleteBasketAsync(username, cancellationToken).ConfigureAwait(false);
        await cache.RemoveAsync(username, cancellationToken).ConfigureAwait(false);
        return isSuccess;
    }

    private static bool TryParseBasket(byte[] basketByteArray, [NotNullWhen(true)] out ShoppingCart? shoppingCart)
    {
        var basket = Proto.ShoppingCart.Parser.ParseFrom(basketByteArray);

        if (basket is null)
        {
            shoppingCart = null;
            return false;
        }

        decimal? totalPrice = decimal.TryParse(basket.TotalPrice, out var totalPriceResult) ? totalPriceResult : null;

        if (totalPrice is null)
        {
            shoppingCart = null;
            return false;
        }

        var shoppingCartItems = new List<ShoppingCartItem>();

        foreach (var item in basket.ShoppingCartItem)
        {
            decimal? price = decimal.TryParse(item.Price, out var parsedPrice) ? parsedPrice : null;
            Ulid? productId = Ulid.TryParse(item.ProduceId, out var parsedProductId) ? parsedProductId : null;

            if (price is null || productId is null)
            {
                shoppingCart = null;
                return false;
            }

            shoppingCartItems.Add(new ShoppingCartItem
            {
                Color = item.Color,
                Price = price.Value,
                ProductId = item.ProduceId,
                ProductName = item.ProductName,
                Quantity = item.Quantity
            });
        }

        shoppingCart = new ShoppingCart
        {
            Username = basket.Username,
            Items = shoppingCartItems,
            Version = basket.Version
        };

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
            ProduceId = x.ProductId.ToString(),
            ProductName = x.ProductName,
            Quantity = x.Quantity
        }));

        return basketInDb.ToByteArray();
    }
}