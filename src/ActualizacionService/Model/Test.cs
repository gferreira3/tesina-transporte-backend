using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ActualizacionService.Model
{
    public  class Test
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Message { get; set; } = null!;
    }
}