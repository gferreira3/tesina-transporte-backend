using ActualizacionService.Dto;
using ActualizacionService.Model;
using MongoDB.Driver;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ActualizacionService.Workers
{
    public class BicisStatusWorker : BackgroundService
    {
        private readonly ILogger<BicisStatusWorker> _logger;

        private readonly IMongoCollection<StationStatus> _stationStatusCollection;

        private ConnectionFactory _connectionFactory;

        private IConnection _connection;

        private IModel _channel;

        public BicisStatusWorker(ILogger<BicisStatusWorker> logger)
        {
            _logger = logger;

            // DOCKER
            var mongoClient = new MongoClient("mongodb://mongo:27017");

            // LOCALHOST
            //var mongoClient = new MongoClient("mongodb://localhost:27017");

            var mongoDatabase = mongoClient.GetDatabase("transporte");

            _stationStatusCollection = mongoDatabase.GetCollection<StationStatus>("stationstatus");
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Sleeping to wait for Rabbit...");
            Task.Delay(10000).Wait();
            Console.WriteLine("Init Rabbit...");

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
            _channel.QueueDeclare(queue: "stationstatus",
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
                    var stations = JsonSerializer.Deserialize<Data>(message);

                    foreach (var station in stations.Stations)
                    {
                        var stationStatus = new StationStatus
                        {
                            IdStation = station.Id,
                            BikesAvailable = station.BikesAvailable
                        };

                        var filterDefinition = Builders<StationStatus>.Filter.Eq(p => p.IdStation, stationStatus.IdStation);
                        var updateDefinition = Builders<StationStatus>.Update
                        .Set(p => p.IdStation, stationStatus.IdStation)
                        .Set(p => p.BikesAvailable, stationStatus.BikesAvailable);
                        var options = new UpdateOptions { IsUpsert = true };
                        _stationStatusCollection.UpdateOne(filterDefinition, updateDefinition, options);
                    }
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR!!!: " + ex.Message);
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            _channel.BasicConsume(queue: "stationstatus", autoAck: false, consumer: consumer);

            await Task.CompletedTask;
        }
    }
}