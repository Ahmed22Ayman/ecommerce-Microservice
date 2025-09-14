using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using CartService.Services;
using CartService.Models;
using CartService.Services.Models;
using CartService.Workers;

var builder = WebApplication.CreateBuilder(args);

// Config
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "change-me";
var redisConn = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var options = ConfigurationOptions.Parse(redisConn);
    options.AbortOnConnectFail = false; // don't crash if Redis isn't up yet
    return ConnectionMultiplexer.Connect(options);
});
builder.Services.AddSingleton<ICartService, RedisCartService>();

// RabbitMQ background consumer
builder.Services.AddHostedService<RabbitMqConsumer>();

// Auth
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// Helper to get userId from either claim or body; for simplicity we accept userId in body and validate authenticated

app.MapGet("/api/cart", async (ICartService carts, HttpContext ctx) =>
{
    var userId = ctx.User?.Identity?.Name;
    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();
    var cart = await carts.GetCartAsync(userId!);
    return Results.Ok(cart);
}).RequireAuthorization();

app.MapPost("/api/cart/items", async (ICartService carts, HttpContext ctx, AddCartItemRequest req) =>
{
    var userId = ctx.User?.Identity?.Name;
    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

    await carts.AddItemAsync(userId!, new CartItemDto
    {
        ProductId = req.ProductId,
        Quantity = req.Quantity,
        Price = req.Price
    });
    // default TTL 24h on add
    await carts.SetTtlAsync(userId!, TimeSpan.FromHours(24));
    var cart = await carts.GetCartAsync(userId!);
    return Results.Ok(cart);
}).RequireAuthorization();

app.MapDelete("/api/cart/items/{productId}", async (ICartService carts, HttpContext ctx, long productId) =>
{
    var userId = ctx.User?.Identity?.Name;
    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

    await carts.RemoveItemAsync(userId!, productId);
    var cart = await carts.GetCartAsync(userId!);
    return Results.Ok(cart);
}).RequireAuthorization();

app.MapDelete("/api/cart", async (ICartService carts, HttpContext ctx) =>
{
    var userId = ctx.User?.Identity?.Name;
    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

    await carts.ClearCartAsync(userId!);
    return Results.NoContent();
}).RequireAuthorization();

app.Run();

namespace CartService.Models
{
    public record AddCartItemRequest
    {
        public long ProductId { get; init; }
        public int Quantity { get; init; }
        public double Price { get; init; }
    }
}
