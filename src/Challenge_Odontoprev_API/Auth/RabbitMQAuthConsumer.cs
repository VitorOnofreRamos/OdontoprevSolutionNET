using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Challenge_Odontoprev_API.Auth;

public class RabbitMQAuthConsumer : BackgroundService
{
    private readonly ILogger<RabbitMQAuthConsumer> _logger;
    private readonly IConfiguration _configuration;
    private IConnection _connection;
    private IChannel _channel;
    private readonly string _authExchange;
    private readonly string _userCreatedQueue;
    private readonly string _userLoggedInQueue;

    public RabbitMQAuthConsumer(ILogger<RabbitMQAuthConsumer> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        _authExchange = configuration["RabbitMQSettings:AuthExhange"];
        _userCreatedQueue = configuration["RabbitMQSettings:UserCreatedQueue"];
        _userLoggedInQueue = configuration["RabbitMQSettings:UserLoggedInQueue"];
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RabbitMQ Auth Consumer starting...");

        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQSettings:HostName"],
            UserName = _configuration["RabbitMQSettings:UserName"],
            Password = _configuration["RabbitMQSettings:Password"],
            Port = int.Parse(_configuration["RabbitMQSettings:Port"]),
        };

        try
        {
            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken);

            // Declare exchange and queues
            await _channel.ExchangeDeclareAsync(
                exchange: _authExchange,
                type: ExchangeType.Topic,
                durable: true,
                cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: _userCreatedQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: _userLoggedInQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await _channel.QueueBindAsync(
                queue: _userCreatedQueue,
                exchange: _authExchange,
                routingKey: "user.created",
                cancellationToken: cancellationToken);

            await _channel.QueueBindAsync(
                queue: _userLoggedInQueue,
                exchange: _authExchange,
                routingKey: "user.loggedin",
                cancellationToken: cancellationToken);

            _logger.LogInformation("RabbitMQ Auth Consumer started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting RabbitMQ Auth Consumer");
            throw;
        }

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RabbitMQ Auth Consumer is listening for messages...");

        // Configure consumer for user created events
        var consumerUserCreated = new AsyncEventingBasicConsumer(_channel);
        consumerUserCreated.ReceivedAsync += async (sender, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation($"Useer Created Message Received: {message}");

                // Process message
                await ProcessUserCreatedMessageAsync(message);

                // Ackowledge the message
                await _channel.BasicAcksAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processig user created message");
                // Negative acknowledge to requeue the message
                await _channel.BasicAcksAsync(ea.DeliveryTag, false, true);
            }
        };

        // Configure consumer for user login events
        var consumerUserLoggedIn = new AsyncEventingBasicConsumer(_channel);
        consumerUserLoggedIn.ReceivedAsync += async (sender, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation($"User Login Message Received: {message}");

                // Process message
                await ProcessUserLoggedInMessageAsync(message);

                // Ackowledge the message
                await _channel.BasicAcksAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processig user created message");
                // Negative acknowledge to requeue the message
                await _channel.BasicAcksAsync(ea.DeliveryTag, false, true);
            }
        };

        //Start consuming
        var consumerTag1 = await _channel.BasicConsumeAsync(
            queue: _userCreatedQueue,
            autoAck: false,
            consumer: consumerUserCreated,
            cancellationToken: stoppingToken);

        var consumerTag2 = await _channel.BasicConsumeAsync(
            queue: _userLoggedInQueue,
            autoAck: false,
            consumer: consumerUserLoggedIn,
            cancellationToken: stoppingToken);

        // Keep the service runnig until cancellation is requested
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessUserCreatedMessageAsync(string message)
    {
        var userCreated = JsonConvert.DeserializeObject<UserCreatedEvent>(message);]

        // Add your custom processing logic here
        // For example, syncing user data with your application's user store

        _logger.LogInformation($"User created: {userCreated.Username} with role {userCreated.Role}");

        await Task.CompletedTask;
    }

    private async Task ProcessUserLoggedInMessageAsync(string message)
    {
        var userLoggedIn = JsonConvert.DeserializeObject<UserLoggedInEvent>(message);

        // Add your custom processing logic here
        // For example, syncing user data with your application's user store

        _logger.LogInformation($"User logged in: {userLoggedIn.Username} at {userLoggedIn.LoggedInAt}");

        await Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RabbitMQ Auth Consumer is stopping...");

        if (_channel != null && _channel.IsOpen)
        {
            await _channel.CloseAsync(cancellationToken);
        }

        if (_connection != null && _connection.IsOpen)
        {
            await _connection.CloseAsync(cancellationToken);
        }

        _logger.LogInformation("RabbitMQ Auth Consumer stopped");

        await base.StopAsync(cancellationToken);
    }
}

// Classes para deserialização de eventos
public class UserCreatedEvent
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserLoggedInEvent
{
    public string Id { get; set; }
    public string Username { get; set; }
    public DateTime LoggedInAt { get; set; }
}
