using System.Text.Json;
using System.Text.Json.Serialization;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Basket.API.IntegrationTests.ApiGivens;

public class OrderCommandGiven(WireMockServer orderCommandWireMockServer)
{
    
    public OrderCommandGiven ACreateOrderSuccessResponse(
        string customerId,
        string orderId)
    {
        orderCommandWireMockServer
            .Given(Request.Create()
                .WithPath($"/api/v1/customers/{customerId}/orders/*")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithBody(JsonSerializer.Serialize(new CreateOrderResponse(orderId)))
                .WithStatusCode(201));
        return this;
    }
    
    public OrderCommandGiven ReturningBadRequest(ValidationErrors validationErrors)
    {
        orderCommandWireMockServer
            .Given(Request.Create()
                .UsingAnyMethod())
            .RespondWith(Response.Create()
                .WithStatusCode(400)
                .WithBody(JsonSerializer.Serialize(validationErrors)));
        return this;
    }
    
    public OrderCommandGiven ReturningInternalServerError()
    {
        orderCommandWireMockServer
            .Given(Request.Create()
                .UsingAnyMethod())
            .RespondWith(Response.Create()
                .WithStatusCode(500));
        return this;
    }
    
    public OrderCommandGiven ReturningUnauthorized()
    {
        orderCommandWireMockServer
            .Given(Request.Create()
                .UsingAnyMethod())
            .RespondWith(Response.Create()
                .WithStatusCode(401));
        return this;
    }

    public OrderCommandGiven ReturningForbidden()
    {
        orderCommandWireMockServer
            .Given(Request.Create()
                .UsingAnyMethod())
            .RespondWith(Response.Create()
                .WithStatusCode(403));
        return this;
    }
}

public record ValidationErrors(
    [property: JsonPropertyName("validation_errors")] ValidationError[] Errors);

public record ValidationError(
    [property: JsonPropertyName("property_name")] string PropertyName,
    [property: JsonPropertyName("reason")] string Reason);
    
public record CreateOrderResponse(
    [property: JsonPropertyName("order_id")]
    string? Id);