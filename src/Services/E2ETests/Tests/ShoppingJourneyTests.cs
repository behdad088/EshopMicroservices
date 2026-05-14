using E2ETests.Fixtures;
using E2ETests.Helpers;

namespace E2ETests.Tests;

public class ShoppingJourneyTests(E2EFixture fixture) : IClassFixture<E2EFixture>
{
    [Fact]
    public async Task Alice_can_browse_add_checkout_and_read_order()
    {
        // Step 1: Get a product from Catalog Service
        var productsResp = await fixture.Api.GetAsync(
            $"{fixture.CatalogBaseUrl}/api/v1/catalog/products");

        productsResp.Ok.ShouldBeTrue("GET /api/v1/catalog/products failed");

        var productsJson = await productsResp.JsonAsync();
        var productId = productsJson!.Value
            .GetProperty("result")
            .GetProperty("data")
            .EnumerateArray()
            .First()
            .GetProperty("id")
            .GetString()!;

        productId.ShouldNotBeNullOrEmpty();

        // Step 2: Store Alice's Basket Service
        var storeResp = await fixture.Api.PostAsync(
            $"{fixture.BasketBaseUrl}/api/v1/basket/customers",
            CreateBasketObject(productId)
            );

        storeResp.Ok.ShouldBeTrue("POST /api/v1/basket/customers failed");

        // Step 3: Checkout basket in basket service. Basket service will send a request to
        // order command service to create an order, which will then be available in order query service.
        var checkoutResp = await fixture.Api.PostAsync(
            $"{fixture.BasketBaseUrl}/api/v1/basket/customers/checkout",
            CheckoutBasketObject());

        checkoutResp.Ok.ShouldBeTrue("POST /api/v1/basket/customers/checkout failed");

        // Step 4: Poll Order Query until Alice's order appears
        var orders = await PollingHelper.WaitForAsync(
            async () =>
            {
                var resp = await fixture.Api.GetAsync(
                    $"{fixture.OrderQueryBaseUrl}/v1/customer/{fixture.CustomerId}/orders");

                if (!resp.Ok)
                    return null;

                var body = await resp.JsonAsync();

                return body?.GetProperty("data")
                    .GetProperty("data")
                    .EnumerateArray()
                    .ToList();
            },
            predicate: items => items is { Count: > 0 },
            timeoutSeconds: 30,
            intervalSeconds: 2);

        orders.ShouldNotBeNull("Order never appeared in the query side within 30s");
        orders.ShouldNotBeEmpty();
    }

    private APIRequestContextOptions CheckoutBasketObject()
    {
        return new APIRequestContextOptions
        {
            DataObject = new
            {
                username = fixture.Username,
                order_name = $"E2E-Order-{Guid.NewGuid():N}",
                billing_address = new
                {
                    firstname = "Alice",
                    lastname = "Smith",
                    email_address = "AliceSmith@email.com",
                    address_line = "Medicinaregatan 20",
                    country = "Sweden",
                    state = "Vastra Gotaland",
                    zip_code = "41390"
                },
                payment = new
                {
                    card_name = "Alice Smith",
                    card_number = "4234432454657532",
                    expiration = "12/2026",
                    cvv = "123",
                    payment_method = 1
                }
            }
        };
    }

    private APIRequestContextOptions CreateBasketObject(string productId)
    {
        return new APIRequestContextOptions
        {
            DataObject = new
            {
                shopping_cart = new
                {
                    username = fixture.Username,
                    items = new[]
                    {
                        new
                        {
                            quantity = 1,
                            color = "Blue",
                            price = 49.99m,
                            product_id = productId,
                            product_name = "E2E Test Product"
                        }
                    }
                }
            }
        };
    }
}
