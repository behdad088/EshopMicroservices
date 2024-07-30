namespace Basket.API.Features.GetBasket;

public record GetBasketQuery(string Username) : IQuery<GetBasketResult>;
public record GetBasketResult(ShoppingCart ShoppingCart);

public class GetBasketQueryHandler : IQueryHandler<GetBasketQuery, GetBasketResult>
{
    public Task<GetBasketResult> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}