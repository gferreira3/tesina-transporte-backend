using ActualizacionService.Dto;
using ActualizacionService.Model;
using MongoDB.Driver;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ActualizacionService.Workers
{
    public class SubtesWorker : BackgroundService
    {
        private readonly IMongoCollection<Alerta> _alertaCollection;

        private ConnectionFactory _connectionFactory;

        private IConnection _connection;

        private IModel _channel;

        public SubtesWorker(ILogger<SubtesWorker> logger)
        {
            var mongoClient = new MongoClient("mongodb://mongo:27017");
            var mongoDatabase = mongoClient.GetDatabase("transporte");

            _alertaCollection = mongoDatabase.GetCollection<Alerta>("alertas");
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Sleeping to wait for Rabbit...");
            Task.Delay(10000).Wait();
            Console.WriteLine("Init Rabbit...");

            _connectionFactory = new ConnectionFactory
            {
                HostName = "rabbitmq",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                DispatchConsumersAsync = true
            };
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "alertas",
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
                    var entities = JsonSerializer.Deserialize<Entity[]>(message);

                    foreach (var entity in entities)
                    {
                        var text = entity.Alert.DescriptionText.Translation[0].Text;
                        var routeId = entity.Alert.InformedEntity[0].RouteID;
                        var alerta = new Alerta
                        {
                            IdAlerta = entity.Id,
                            Mensaje = string.Format("{0} - {1}", routeId, text)
                        };

                        var filterDefinition = Builders<Alerta>.Filter.Eq(p => p.IdAlerta, alerta.IdAlerta);
                        var updateDefinition = Builders<Alerta>.Update.Set(p => p.IdAlerta, alerta.IdAlerta).Set(p => p.Mensaje, alerta.Mensaje);
                        var options = new UpdateOptions { IsUpsert = true };
                        _alertaCollection.UpdateOne(filterDefinition, updateDefinition, options);
                    }
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception)
                {
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            _channel.BasicConsume(queue: "alertas", autoAck: false, consumer: consumer);

            await Task.CompletedTask;
        }
    }
}