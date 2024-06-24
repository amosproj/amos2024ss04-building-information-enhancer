// ReSharper disable InconsistentNaming

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BIE.DataPipeline.Metadata;

public class MetadataObject
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string _id { get; set; }
    
    public BasicData basicData { get; set; }
    public MetaData metaData { get; set; }
    
    public class BasicData
    {
        public string DatasetId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public List<string> Tables { get; set; }

        public BasicData()
        {
            Tables = new List<string>();
        }
    }

    public class MetaData
    {
        public string Icon { get; set; }
        public string Type { get; set; }
        public int MinZoomLevel { get; set; }
    }
}