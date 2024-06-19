using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace APIGateway.Models
{
    public class DatasetData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("basicData")]
        public DatasetBasicData BasicData { get; set; }

        [BsonElement("metaData")]
        public DatasetMetadata MetaData { get; set; }
    }
}
