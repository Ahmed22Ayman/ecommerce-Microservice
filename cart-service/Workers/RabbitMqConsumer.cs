using System.Text;
using System.Text.Json;
using CartService.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CartService.Workers;

public class RabbitMqConsumer : BackgroundService
{
    private readonly ILogger<RabbitMqConsumer> _logger;
    private readonly IConfiguration _config;
    private readonly ICartService _carts;

    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqConsumer(ILogger<RabbitMqConsumer> logger, IConfiguration config, ICartService carts)
    {
        _logger = logger;
        _config = config;
        _carts = carts;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var host = _config["RabbitMQ:HostName"] ?? "localhost";
        var port = int.TryParse(_config["RabbitMQ:Port"], out var p) ? p : 5672;
        var user = _config["RabbitMQ:UserName"] ?? "guest";
        var pass = _config["RabbitMQ:Password"] ?? "guest";
        var exchange = _config["RabbitMQ:Exchange"] ?? "order.events";
        var createdQueue = _config["RabbitMQ:OrderCreatedQueue"] ?? "order.created.queue";
        var createdRoutingKey = _config["RabbitMQ:OrderCreatedRoutingKey"] ?? "order.created";

        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = port,
            UserName = user,
            Password = pass,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true, autoDelete: false);
        _channel.QueueDeclare(createdQueue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(createdQueue, exchange, createdRoutingKey);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                _logger.LogInformation("[CartService] Received message on {RoutingKey}: {Payload}", ea.RoutingKey, json);

                if (ea.RoutingKey == createdRoutingKey)
                {
                    var evt = JsonSerializer.Deserialize<OrderCreatedEvent>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (evt != null && evt.UserId != null)
                    {
                        await _carts.ExtendTtlAsync(evt.UserId.ToString()!, TimeSpan.FromDays(7));
                        _logger.LogInformation("Extended cart TTL for user {UserId} by 7 days due to OrderCreated", evt.UserId);
                    }
                }

                _channel!.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process RabbitMQ message");
                // Nack without requeue to avoid tight loops; adjust as needed
                _channel!.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(queue: createdQueue, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        try { _channel?.Close(); } catch { }
        try { _connection?.Close(); } catch { }
        base.Dispose();
    }

    private class OrderCreatedEvent
    {
        public long OrderId { get; set; }
        public long UserId { get; set; }
        public double TotalAmount { get; set; }
        public List<Item>? Items { get; set; }

        public class Item
        {
            public long ProductId { get; set; }
            public int Quantity { get; set; }
            public double Price { get; set; }
        }
    }
}
