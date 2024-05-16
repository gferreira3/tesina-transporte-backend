using MongoDB.Driver;
using System.Text.Json;
using TransporteApiMono.Dto;
using TransporteApiMono.Model;

namespace TransporteApiMono.Workers
{
    public class SubtesWorker : BackgroundService
    {
        private readonly ILogger<SubtesWorker> _logger;

        private readonly IMongoCollection<Alerta> _alertaCollection;

        private static readonly HttpClient client = new();

        public SubtesWorker(ILogger<SubtesWorker> logger)
        {
            _logger = logger;

            // DOCKER
            var mongoClient = new MongoClient("mongodb://mongo:27017");

            // LOCALHOST
            //var mongoClient = new MongoClient("mongodb://localhost:27017");

            var mongoDatabase = mongoClient.GetDatabase("transporte");

            _alertaCollection = mongoDatabase.GetCollection<Alerta>("alertas");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {        
            while (!stoppingToken.IsCancellationRequested)
            {
                var stringTask = client.GetStreamAsync("https://apitransporte.buenosaires.gob.ar/subtes/serviceAlerts?json=1&client_id=2e7a1773d08c43f9ace278cc7ae6bc38&client_secret=c4c35ed381984d03B025DfA3e9c4591b");

                var alerts = await JsonSerializer.DeserializeAsync<ServiceAlert>(await stringTask);

                foreach (var entity in alerts.Entity)
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

                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}