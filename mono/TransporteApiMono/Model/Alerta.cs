using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TransporteApiMono.Model
{
    public  class Alerta
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("idalerta")]
        public string IdAlerta { get; set; }

        [BsonElement("mensaje")]
        public string Mensaje { get; set; }
    }
}