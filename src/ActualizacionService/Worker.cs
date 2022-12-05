using ActualizacionService.Model;
using MongoDB.Driver;
using MongoDB.Driver.Core.Connections;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ActualizacionService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly IMongoCollection<Test> _testCollection;

        private ConnectionFactory _connectionFactory;

        private RabbitMQ.Client.IConnection _connection;

        private IModel _channel;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;

            var mongoClient = new MongoClient("mongodb://localhost:27017");

            var mongoDatabase = mongoClient.GetDatabase("transporte");

            _testCollection = mongoDatabase.GetCollection<Test>("test");
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
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
                    Console.WriteLine(" [x] Received {0}", message);

                    _testCollection.InsertOne(new Test { Message = message });

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