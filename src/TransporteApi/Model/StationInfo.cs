using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TransporteApi.Model
{
    public  class StationInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("idstation")]
        public string IdStation { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("address")]
        public string Address { get; set; }

        [BsonElement("capacity")]
        public int Capacity { get; set; }
    }
}