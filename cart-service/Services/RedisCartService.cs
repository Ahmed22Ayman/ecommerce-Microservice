using System.Text.Json;
using CartService.Services.Models;
using StackExchange.Redis;

namespace CartService.Services;

public class RedisCartService : ICartService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisCartService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = _redis.GetDatabase();
    }

    private static string Key(string userId) => $"cart:{userId}";

    public async Task<CartDto> GetCartAsync(string userId)
    {
        var key = Key(userId);
        var json = await _db.StringGetAsync(key);
        if (json.IsNullOrEmpty)
        {
            return new CartDto { UserId = userId, Items = new List<CartItemDto>() };
        }
        var cart = JsonSerializer.Deserialize<CartDto>(json!) ?? new CartDto { UserId = userId };
        cart.UserId = userId; // ensure
        return cart;
    }

    public async Task AddItemAsync(string userId, CartItemDto item)
    {
        var cart = await GetCartAsync(userId);
        var existing = cart.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
        if (existing is null)
        {
            cart.Items.Add(item);
        }
        else
        {
            var idx = cart.Items.FindIndex(i => i.ProductId == item.ProductId);
            cart.Items[idx] = existing with { Quantity = existing.Quantity + item.Quantity, Price = item.Price };
        }
        await SaveAsync(userId, cart);
    }

    public async Task RemoveItemAsync(string userId, long productId)
    {
        var cart = await GetCartAsync(userId);
        cart.Items.RemoveAll(i => i.ProductId == productId);
        await SaveAsync(userId, cart);
    }

    public async Task ClearCartAsync(string userId)
    {
        await _db.KeyDeleteAsync(Key(userId));
    }

    public async Task SetTtlAsync(string userId, TimeSpan ttl)
    {
        await _db.KeyExpireAsync(Key(userId), ttl);
    }

    public async Task ExtendTtlAsync(string userId, TimeSpan additional)
    {
        var ttl = await _db.KeyTimeToLiveAsync(Key(userId));
        if (ttl.HasValue)
        {
            await _db.KeyExpireAsync(Key(userId), ttl.Value + additional);
        }
        else
        {
            await _db.KeyExpireAsync(Key(userId), additional);
        }
    }

    public async Task RemoveTtlAsync(string userId)
    {
        await _db.KeyPersistAsync(Key(userId));
    }

    private async Task SaveAsync(string userId, CartDto cart)
    {
        var key = Key(userId);
        var json = JsonSerializer.Serialize(cart);
        // preserve existing TTL if any
        var ttl = await _db.KeyTimeToLiveAsync(key);
        await _db.StringSetAsync(key, json);
        if (ttl.HasValue)
        {
            await _db.KeyExpireAsync(key, ttl);
        }
    }
}
