
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Drawing;
using System.Text;
using System.Text.Json;

namespace AddWatermakToImages_RabbitMQ.Services
{
    public class ImageWatermarkProcessBackgroundService : BackgroundService
    {
        private readonly RabbitMQClientService _rabbitmqClientService;
        private readonly ILogger<ImageWatermarkProcessBackgroundService> _logger;
        private RabbitMQ.Client.IModel _channel;

        public ImageWatermarkProcessBackgroundService(RabbitMQClientService rabbitmqClientService, ILogger<ImageWatermarkProcessBackgroundService> logger)
        {
            _rabbitmqClientService = rabbitmqClientService;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            _channel.BasicConsume(RabbitMQClientService.QueueName, false, consumer);

            consumer.Received += Consumer_Received;

            return Task.CompletedTask;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitmqClientService.Connect();

            _channel.BasicQos(0, 1, false);

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {

            try
            {

                var imageCreatedEvent = JsonSerializer.Deserialize<productImageCreatedEvent>(Encoding.UTF8.GetString(@event.Body.ToArray()));

                var path = Path.Combine(Directory.GetCurrentDirectory(), "C:\\Users\\yavuz\\source\\repos\\AddWatermakToImages_RabbitMQ\\AddWatermakToImages_RabbitMQ\\wwwroot\\Images", imageCreatedEvent.ImageName);

                using var img = Image.FromFile(path);

                using var graphic = Graphics.FromImage(img);

                var font = new Font(FontFamily.GenericSansSerif, 30, FontStyle.Bold, GraphicsUnit.Pixel);

                var textSize = graphic.MeasureString("RabbitMQ ile yazı yazdırma", font);

                var color = Color.FromArgb(128, 255, 255, 255);

                var brush = new SolidBrush(color);

                var position = new Point(img.Width - (int)textSize.Width + 30, img.Height - (int)textSize.Height);

                graphic.DrawString("RabbitMQ ile yazı yazdırma", font, brush, position);

                img.Save("C:\\Users\\yavuz\\source\\repos\\AddWatermakToImages_RabbitMQ\\AddWatermakToImages_RabbitMQ\\wwwroot\\Images\\Watermarks" + imageCreatedEvent.ImageName);

                img.Dispose();

                graphic.Dispose();

                _channel.BasicAck(@event.DeliveryTag, false);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return Task.CompletedTask;
        }

    }
}
