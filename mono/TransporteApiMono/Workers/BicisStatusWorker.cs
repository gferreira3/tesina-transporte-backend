using MongoDB.Driver;
using System.Text.Json;
using TransporteApiMono.Dto;
using TransporteApiMono.Model;

namespace TransporteApiMono.Workers
{
    public class BicisStatusWorker : BackgroundService
    {
        private readonly ILogger<BicisStatusWorker> _logger;

        private readonly IMongoCollection<StationStatus> _stationStatusCollection;

        private static readonly HttpClient client = new();

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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var stringTask = client.GetStreamAsync("https://apitransporte.buenosaires.gob.ar/ecobici/gbfs/stationStatus?client_id=2e7a1773d08c43f9ace278cc7ae6bc38&client_secret=c4c35ed381984d03B025DfA3e9c4591b");

                var stations = await JsonSerializer.DeserializeAsync<StationInformation>(await stringTask);

                foreach (var station in stations.Data.Stations)
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

                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}