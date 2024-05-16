using MongoDB.Driver;
using System.Text.Json;
using TransporteApiMono.Dto;
using TransporteApiMono.Model;

namespace TransporteApiMono.Workers
{
    public class BicisInfoWorker : BackgroundService
    {
        private readonly ILogger<BicisInfoWorker> _logger;

        private readonly IMongoCollection<StationInfo> _stationInfoCollection;

        private static readonly HttpClient client = new();

        public BicisInfoWorker(ILogger<BicisInfoWorker> logger)
        {
            _logger = logger;

            // DOCKER
            var mongoClient = new MongoClient("mongodb://mongo:27017");

            // LOCALHOST
            //var mongoClient = new MongoClient("mongodb://localhost:27017");

            var mongoDatabase = mongoClient.GetDatabase("transporte");

            _stationInfoCollection = mongoDatabase.GetCollection<StationInfo>("stationinfo");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var stringTask = client.GetStreamAsync("https://apitransporte.buenosaires.gob.ar/ecobici/gbfs/stationInformation?client_id=2e7a1773d08c43f9ace278cc7ae6bc38&client_secret=c4c35ed381984d03B025DfA3e9c4591b");

                var stations = await JsonSerializer.DeserializeAsync<StationInformation>(await stringTask);

                foreach (var station in stations.Data.Stations)
                {
                    var stationInfo = new StationInfo
                    {
                        IdStation = station.Id,
                        Name = station.Name,
                        Address = station.Address,
                        Capacity = station.Capacity
                    };

                    var filterDefinition = Builders<StationInfo>.Filter.Eq(p => p.IdStation, stationInfo.IdStation);
                    var updateDefinition = Builders<StationInfo>.Update
                    .Set(p => p.IdStation, stationInfo.IdStation)
                    .Set(p => p.Name, stationInfo.Name)
                    .Set(p => p.Address, stationInfo.Address)
                    .Set(p => p.Capacity, stationInfo.Capacity);
                    var options = new UpdateOptions { IsUpsert = true };
                    _stationInfoCollection.UpdateOne(filterDefinition, updateDefinition, options);
                }

                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}