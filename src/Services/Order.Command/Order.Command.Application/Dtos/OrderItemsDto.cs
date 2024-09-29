namespace Order.Command.Application.Dtos;

public record OrderItemsDto(
    Guid Id,
    Guid? OrderId,
    Guid? ProductId,
    int? Quantity,
    decimal? Price);