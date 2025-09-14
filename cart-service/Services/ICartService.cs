using CartService.Services.Models;

namespace CartService.Services;

public interface ICartService
{
    Task<CartDto> GetCartAsync(string userId);
    Task AddItemAsync(string userId, CartItemDto item);
    Task RemoveItemAsync(string userId, long productId);
    Task ClearCartAsync(string userId);
    Task SetTtlAsync(string userId, TimeSpan ttl);
    Task ExtendTtlAsync(string userId, TimeSpan additional);
    Task RemoveTtlAsync(string userId);
}
