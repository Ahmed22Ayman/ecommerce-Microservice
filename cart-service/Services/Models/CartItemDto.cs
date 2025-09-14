namespace CartService.Services.Models;

public record CartItemDto
{
    public long ProductId { get; init; }
    public int Quantity { get; init; }
    public double Price { get; init; }
}
