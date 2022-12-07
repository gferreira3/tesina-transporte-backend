using ActualizacionService.Dto;
using ActualizacionService.Model;
using MongoDB.Driver;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ActualizacionService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly IMongoCollection<Alerta> _alertaCollection;

        private ConnectionFactory _connectionFactory;

        private IConnection _connection;

        private IModel _channel;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;

            // DOCKER
            var mongoClient = new MongoClient("mongodb://mongo:27017");

            // LOCALHOST
            //var mongoClient = new MongoClient("mongodb://localhost:27017");

            var mongoDatabase = mongoClient.GetDatabase("transporte");

            _alertaCollection = mongoDatabase.GetCollection<Alerta>("alertas");
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _connectionFactory = new ConnectionFactory
            {
                // DOCKER
                HostName = "rabbitmq",
                // LOCALHOST
                //HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                DispatchConsumersAsync = true
            };
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "hello",
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

            return base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            _connection.Close();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (bc, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                try
                {
                    Console.WriteLine("Alertas recibidas!!!");

                    var entities = JsonSerializer.Deserialize<Entity[]>(message);

                    foreach (var entity in entities)
                    {
                        var text = entity.Alert.DescriptionText.Translation[0].Text;
                        _alertaCollection.InsertOne(new Alerta { Message = text });
                    }
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            _channel.BasicConsume(queue: "hello", autoAck: false, consumer: consumer);       

            await Task.CompletedTask;
        }
    }
}