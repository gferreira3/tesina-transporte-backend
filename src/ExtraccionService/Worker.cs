using ExtraccionService.Dto;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ExtraccionService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private static readonly HttpClient client = new HttpClient();

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                // DOCKER
                var factory = new ConnectionFactory() { HostName = "rabbitmq", Port = 5672, UserName = "guest", Password = "guest" };

                // LOCALHOST
                //var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672, UserName = "guest", Password = "guest" };
                
                var connection = factory.CreateConnection();
                var channel = connection.CreateModel();

                channel.QueueDeclare(queue: "hello",
                            durable: false,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);

                var stringTask = client.GetStreamAsync("https://apitransporte.buenosaires.gob.ar/colectivos/serviceAlerts?json=1&client_id=2e7a1773d08c43f9ace278cc7ae6bc38&client_secret=c4c35ed381984d03B025DfA3e9c4591b");

                var alerts = await JsonSerializer.DeserializeAsync<ServiceAlert>(await stringTask);

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(alerts.Entity));

                channel.BasicPublish(exchange: "",
                                    routingKey: "hello",
                                    basicProperties: null,
                                    body: body);
                Console.WriteLine("Alertas enviadas!!!");

                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}