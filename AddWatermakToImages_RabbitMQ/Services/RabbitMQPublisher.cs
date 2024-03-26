using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace AddWatermakToImages_RabbitMQ.Services
{
    public class RabbitMQPublisher
    {
       private readonly RabbitMQClientService _rabbitMQClientService;

        public RabbitMQPublisher(RabbitMQClientService rabbitMQClientService)
        {
            _rabbitMQClientService = rabbitMQClientService;
        }

        public void Publish(productImageCreatedEvent productImageCreatedEvent) 
        {
            var channel = _rabbitMQClientService.Connect();

            var bodyString = JsonSerializer.Serialize(productImageCreatedEvent);

            var bodyByte = Encoding.UTF8.GetBytes(bodyString);

            var properties = channel.CreateBasicProperties();

            properties.Persistent = true;

            channel.BasicPublish(RabbitMQClientService.ExchangeName, RabbitMQClientService.RoutingWatermark,
                basicProperties: properties, body: bodyByte);

        }
    }
}
