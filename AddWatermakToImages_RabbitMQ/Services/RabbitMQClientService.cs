﻿using RabbitMQ.Client;

namespace AddWatermakToImages_RabbitMQ.Services
{
    public class RabbitMQClientService : IDisposable
    {

        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        private readonly ILogger<RabbitMQClientService> _logger;

        public static string ExchangeName = "ImageDirectExchange";
        public static string RoutingWatermark = "watermark-route-image";
        public static string QueueName = "queue-watermark-image";


        public RabbitMQClientService(ConnectionFactory connectionFactory,ILogger<RabbitMQClientService> logger)
        {
          _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public IModel Connect() 
        {
            _connection = _connectionFactory.CreateConnection();

            if (_channel != null && _channel.IsOpen == true)
            {
                return _channel;
            }

            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(ExchangeName, type: "direct", true, false);

            _channel.QueueDeclare(QueueName, true, false, false, null);

            _channel.QueueBind(exchange:ExchangeName,queue:QueueName,routingKey:RoutingWatermark);

            _logger.LogInformation("RabbitMQ ile bağlantı kuruldu...");

            return _channel;
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _channel = default;

            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("RabbitMQ ile bağlantı sonlandırıldı...");
        }
    }
}
