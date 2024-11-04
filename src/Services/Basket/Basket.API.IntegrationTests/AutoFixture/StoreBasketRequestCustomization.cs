using AutoFixture;
using Basket.API.Features.StoreBasket;
using Basket.API.Models.Dtos;

namespace Basket.API.IntegrationTests.AutoFixture;

public class StoreBasketRequestCustomization : ICustomization
{
    private const string ValidCharacters = "ABCDEFGHIKLMNOPQRSTVXYZ";

    private static readonly string? Username = new(Enumerable.Range(0, 10)
        .Select(_ => ValidCharacters[Random.Shared.Next(0, 10)]).ToArray());

    public void Customize(IFixture fixture)
    {
        fixture.Customize<StoreBasketRequest>(composer => composer
            .With(r => r.ShoppingCart, () => new BasketDtoRequest(Username!, [
                new BasketItem(
                    1,
                    "Test Color",
                    100,
                    Ulid.NewUlid().ToString(),
                    "Test Product")
            ])));
    }
}