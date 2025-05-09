using Auth.API.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;

namespace Auth.API.Services;

public class RabbitMQService
{
    private readonly RabbitMQSettings _settings;
    private IConnection _connection;
    private IChannel _channel;
    private bool _disposed;

    public RabbitMQService(IOptions<RabbitMQSettings> setting)
    {
        _settings = setting.Value;
        InitializeRabbitMQAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeRabbitMQAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            UserName = _settings.UserName,
            Password = _settings.Password,
            Port = _settings.Port
        };

        _connection = await factory.CreateConnectionAsync().ConfigureAwait(false);
        _channel = await _connection.CreateChannelAsync();

        // Declare exhanges
        await _channel.ExchangeDeclareAsync(
            exchange: _settings.AuthExchange,
            type: ExchangeType.Topic,
            durable: true);

        // Declare queue
        await _channel.QueueDeclareAsync(
            queue: _settings.UserCreatedQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await _channel.QueueDeclareAsync(
            queue: _settings.UserLoggedInQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        // Bind queues to exchange
        await _channel.QueueBindAsync(
            queue: _settings.UserCreatedQueue,
            exchange: _settings.AuthExchange,
            routingKey: "user.created");

        await _channel.QueueBindAsync(
            queue: _settings.UserLoggedInQueue,
            exchange: _settings.AuthExchange,
            routingKey: "user.loggedin");
    }

    public async Task PublishUserCreatedEventAsync(User user)
    {
        var message = new
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
        };

        await PublishMessageAsync(_settings.AuthExchange, "user.created", message);
    }

    public async Task PublishUserLoggedInEventAsync(User user)
    {
        var message = new
        {
            Id = user.Id,
            Username = user.Username,
            LoggedInAt = DateTime.UtcNow,
        };

        await PublishMessageAsync(_settings.AuthExchange, "user.loggedin", message);
    }

    private async Task PublishMessageAsync(string exchange, string routingKey, object message)
    {
        var json = JsonConvert.SerializeObject(message);
        var body = Encoding.UTF8.GetBytes(json);

        await _channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            body: body);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        await _channel.DisposeAsync().ConfigureAwait(false);
        await _connection.DisposeAsync().ConfigureAwait(false);
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

public class RabbitMQSettings
{
    public string HostName { get; set; }
    public string UserName { get; set; } 
    public string Password { get; set; }
    public int Port { get; set; }
    public string AuthExchange { get; set; }
    public string UserCreatedQueue { get; set; }
    public string UserLoggedInQueue { get; set; }
}
