using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TransporteApiMono.Model
{
    public  class StationStatus
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("idstation")]
        public string IdStation { get; set; }

        [BsonElement("bikesavailable")]
        public int BikesAvailable { get; set; }
    }
}