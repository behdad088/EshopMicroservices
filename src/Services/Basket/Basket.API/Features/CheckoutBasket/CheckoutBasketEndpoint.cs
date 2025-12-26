using System.Net;
using System.Security.Claims;
using Basket.API.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Basket.API.Features.CheckoutBasket;

public static class CheckoutBasketEndpoint
{
    public static IEndpointRouteBuilder MapCheckoutBasketEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/customers/checkout", CheckoutBasketAsync)
            .WithName("CheckoutBasket")
            .Produces<CheckoutBasketResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("checkout Basket")
            .WithDescription("checkout Basket")
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> CheckoutBasketAsync(
        CheckoutBasketRequest request,
        ISender sender,
        HttpContext httpContext,
        IAuthorizationService authorizationService)
    {
        var isAuthorized =
            await authorizationService.CanCheckoutBasketAsync(httpContext, request.Username ?? string.Empty);

        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (!isAuthorized)
        {
            return TypedResults.Forbid();
        }
        
        var command = MapCommand(request, userId);
        var result = await sender.Send(command).ConfigureAwait(false);

        return result switch
        {
            Result.BasketNotFound => TypedResults.NotFound(),
            Result.Success success => TypedResults.Ok(new CheckoutBasketResponse(success.OrderId)),
            Result.ApiClientError apiClientError => MapApiClientErrorResponse(apiClientError),
            _ => TypedResults.InternalServerError()
        };
    }

    private static IResult MapApiClientErrorResponse(
        Result.ApiClientError apiClientError)
    {
        return apiClientError.FailureReason.StatusCode switch
        {
            (int)HttpStatusCode.BadRequest => TypedResults.BadRequest(GetProblemDetailsFromFailureReasons(apiClientError)),
            (int)HttpStatusCode.Forbidden => TypedResults.Forbid(),
            (int)HttpStatusCode.Unauthorized => TypedResults.Unauthorized(),
            _ => TypedResults.InternalServerError()
        };
    }
    
    private static ProblemDetails GetProblemDetailsFromFailureReasons(
        Result.ApiClientError apiClientError)
    {
        var problemDetails = new ProblemDetails
        {
            Detail = "One or more validation errors occurred.",
            Title = apiClientError.FailureReason.ServiceName,
            Status = apiClientError.FailureReason.StatusCode
        };

        if (apiClientError.FailureReason.Errors.Any())
        {
            problemDetails.Extensions["ValidationErrors"] = apiClientError.FailureReason.Errors;
        }

        return problemDetails;
    }
    
    private static CheckoutBasketCommand MapCommand(
        CheckoutBasketRequest? request,
        string? userId)
    {
        return new CheckoutBasketCommand(
            CheckoutRequestModel: new CheckoutRequestModel(
                Username: request?.Username,
                CustomerId: userId,
                OrderName: request?.OrderName,
                BillingAddress: MapAddress(request?.BillingAddress),
                Payment: MapPayment(request?.Payment)
                ));
    }

    private static CheckoutRequestModel.ModulePayment? MapPayment(
        CheckoutBasketRequest.ModulePayment? payment)
    {
        if (payment is null)
            return null;
        
        return new CheckoutRequestModel.ModulePayment(
            payment.CardName,
            payment.CardNumber,
            payment.Expiration,
            payment.Cvv,
            payment.PaymentMethod);
    }
    private static CheckoutRequestModel.ModuleAddress? MapAddress(
        CheckoutBasketRequest.ModuleAddress? address)
    {
        if (address == null)
            return null;
        
        return new CheckoutRequestModel.ModuleAddress(
            address.Firstname,
            address.Lastname,
            address.EmailAddress,
            address.AddressLine,
            address.Country,
            address.State,
            address.ZipCode);
    }
}