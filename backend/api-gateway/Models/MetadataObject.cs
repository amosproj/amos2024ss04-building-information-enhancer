using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace APIGateway.Models;

public class MetadataObject
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string _id { get; set; } = string.Empty;
    
    [BsonElement("metadataBasicData")]
    public MetadataBasicData metadataBasicData { get; set; } = new MetadataBasicData();

    [BsonElement("metadataAdditionalData")]
    public MetadataAdditionalData metadataAdditionalData { get; set; } = new MetadataAdditionalData();
    
    // The general and most important data about a dataset.
    public class MetadataBasicData
    {
        public string DatasetId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    // The additional data for each of the datasets, queried at a request.
    public class MetadataAdditionalData
    {
        public string Icon { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public string LongDescription { get; set; } = string.Empty;

        // Zoom level is higher the closer you look at something. If current zoom level is below this, it shouldn't display any value.
        public int MinZoomLevel { get; set; } = 0;
        
        // The zoom threshold where areas start to turn into markers
        public int MarkersThreshold { get; set; } = 0;

        // The display property is the property that should be shown in a popup.
        public string DisplayProperty { get; set; } = string.Empty;

        // Table data populated by the data pipeline. Contains the name and the size of the all .yaml files correlated to that specific dataset.
        public List<TableData> Tables { get; set; } = new List<TableData>();
    }
    
    // Table data populated by the data pipeline. Contains the name and the size of the all .yaml files correlated to that specific dataset.
    public class TableData
    {
        // The name of the .yaml file
        public string Name { get; set; } = string.Empty;
        // The number of lines of data in that file.
        public int NumberOfLines { get; set; } = 0;
    }
}