using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TransporteApi.Model
{
    public  class Alerta
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Message { get; set; } = null!;
    }
}