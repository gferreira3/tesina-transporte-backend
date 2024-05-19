using ExtraccionService.Dto;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ExtraccionService.Workers
{
    public class SubtesWorker : BackgroundService
    {
        private static readonly HttpClient client = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Sleeping to wait for Rabbit...");
            Task.Delay(10000).Wait();
            Console.WriteLine("Init Rabbit...");

            var factory = new ConnectionFactory() { HostName = "rabbitmq", Port = 5672, UserName = "guest", Password = "guest" };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "alertas",
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

            while (!stoppingToken.IsCancellationRequested)
            {
                var stringTask = client.GetStreamAsync("https://apitransporte.buenosaires.gob.ar/subtes/serviceAlerts?json=1&client_id=2e7a1773d08c43f9ace278cc7ae6bc38&client_secret=c4c35ed381984d03B025DfA3e9c4591b");

                var alerts = await JsonSerializer.DeserializeAsync<ServiceAlert>(await stringTask);

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(alerts.Entity));

                channel.BasicPublish(exchange: "",
                                    routingKey: "alertas",
                                    basicProperties: null,
                                    body: body);

                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}