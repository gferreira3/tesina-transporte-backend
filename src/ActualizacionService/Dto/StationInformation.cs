using System.Text.Json.Serialization;

namespace ActualizacionService.Dto
{
    public class StationInformation
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("stations")]
        public Station[] Stations { get; set; }
    }

    public class Station
    {
        [JsonPropertyName("station_id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("capacity")]
        public int Capacity { get; set; }

        [JsonPropertyName("num_bikes_available")]
        public int BikesAvailable { get; set; }
    }
}
