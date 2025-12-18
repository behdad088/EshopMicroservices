using eshop.Shared.CQRS.Query;
using eshop.Shared.Logger;

namespace Basket.API.Features.GetBasket;

public record GetBasketQuery(string Username) : IQuery<Result>;

public record Result
{
    public record Success(ShoppingCart ShoppingCart) : Result;

    public record NotFound : Result;
}

public class GetBasketQueryHandler(IBasketRepository basketRepository) : IQueryHandler<GetBasketQuery, Result>
{
    private readonly ILogger _logger = Log.ForContext<GetBasketQueryHandler>();
    public async Task<Result> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        using var _ = LogContext.PushProperty(LogProperties.Username, request.Username);

        _logger.Information("Get Basket for user");
        
        var basket = await basketRepository.GetBasketAsync(request.Username, cancellationToken).ConfigureAwait(false);

        if (basket == null)
            return new Result.NotFound();
        
        return new Result.Success(basket);
    }
}