using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using System.Text;
using PaymentService.Data;
using PaymentService.Models;

var builder = WebApplication.CreateBuilder(args);

// Config
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "change-me";
var sqlitePath = builder.Configuration["Database:Path"] ?? "payments.db";
var rabbitHost = builder.Configuration["RabbitMQ:HostName"] ?? "localhost";
var rabbitPort = int.TryParse(builder.Configuration["RabbitMQ:Port"], out var p) ? p : 5672;
var rabbitUser = builder.Configuration["RabbitMQ:UserName"] ?? "guest";
var rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? "guest";
var exchange  = builder.Configuration["RabbitMQ:Exchange"] ?? "payment.events";

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PaymentsDbContext>(opt =>
    opt.UseSqlite($"Data Source={sqlitePath}")
);

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

builder.Services.AddSingleton<IConnection>(_ =>
{
    var factory = new ConnectionFactory
    {
        HostName = rabbitHost,
        Port = rabbitPort,
        UserName = rabbitUser,
        Password = rabbitPass
    };
    return factory.CreateConnection();
});

var app = builder.Build();

// DB ensure created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// DTOs
record CreatePaymentRequest(long OrderId, long UserId, double Amount, bool SimulateFailure = false);

// Endpoints
app.MapPost("/api/payments", async (CreatePaymentRequest req, PaymentsDbContext db, IConnection conn) =>
{
    var payment = new Payment
    {
        OrderId = req.OrderId,
        UserId = req.UserId,
        Amount = req.Amount,
        Status = req.SimulateFailure ? "FAILED" : "SUCCESS",
        PaymentDate = DateTime.UtcNow
    };

    db.Payments.Add(payment);
    await db.SaveChangesAsync();

    using var channel = conn.CreateModel();
    channel.ExchangeDeclare(exchange, type: ExchangeType.Topic, durable: true, autoDelete: false);

    var routingKey = payment.Status == "SUCCESS" ? "payment.success" : "payment.failed";
    var payload = System.Text.Json.JsonSerializer.Serialize(new
    {
        paymentId = payment.PaymentId,
        orderId = payment.OrderId,
        userId = payment.UserId,
        amount = payment.Amount,
        status = payment.Status,
        paymentDate = payment.PaymentDate
    });
    var body = Encoding.UTF8.GetBytes(payload);
    channel.BasicPublish(exchange, routingKey, basicProperties: null, body: body);

    return Results.Ok(new { payment.PaymentId, payment.Status });
}).RequireAuthorization();

app.Run();
