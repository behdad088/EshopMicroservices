using System.Net;

namespace Basket.API.ApiClient;

public abstract record ClientFailures
{
    public static OrderCommandClientFailures OrderCommandClientFailures = new();
    
}

public record OrderCommandClientFailures : ClientFailures
{
    public CreateOrderFailures CreateOrder = new();
    
    public record CreateOrderFailures
    {
        private static string ServiceName => "Order.Command.API_CreateOrder";

        public FailureReasons OrderBadRequest(params Error[] errors) => new FailureReasons(
            ServiceName,
            (int)HttpStatusCode.BadRequest,
            errors);

        public FailureReasons OrderNotFound() => new FailureReasons(ServiceName, (int)HttpStatusCode.NotFound);
    

        public FailureReasons Unauthorised => new FailureReasons(
            ServiceName,
            (int)HttpStatusCode.Unauthorized,
            new Error("Unauthorized",
                "user is not authorized"));

        public FailureReasons Forbidden => new FailureReasons(
            ServiceName,
            (int)HttpStatusCode.Forbidden,
            new Error("Forbidden",
                "Error 403 - Forbidden"));
    }
}

public record FailureReasons(string ServiceName, int StatusCode, params Error[] Errors) : ClientFailures;

public record Error(
    [property: JsonPropertyName("error_name")]
    string ErrorName,
    [property: JsonPropertyName("error_message")]
    string ErrorMessage);

public record ValidationErrors(
    [property: JsonPropertyName("Validation_errors")] ValidationError[] Errors);

public record ValidationError(
    [property: JsonPropertyName("property_name")] string PropertyName,
    [property: JsonPropertyName("reason")] string Reason);