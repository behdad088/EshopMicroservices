using AutoFixture;
using Basket.API.Features.StoreBasket;
using Basket.API.Models.Dtos;

namespace Basket.API.IntegrationTests.AutoFixture;

public class StoreBasketRequestCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<StoreBasketRequest>(composer => composer
            .With(r => r.ShoppingCart, () => new BasketDtoRequest(Username: Username!, Items:
            [
                new BasketItem(
                    Quantity: 1, Color: "Test Color", Price: 100, fixture.Create<Guid>(), ProductName: "Test Product")
            ])));
    }

    private const string ValidCharacters = "ABCDEFGHIKLMNOPQRSTVXYZ";
    private static readonly string? Username = new string(Enumerable.Range(0, 10)
        .Select(_ => ValidCharacters[Random.Shared.Next(0, 10)]).ToArray());
}