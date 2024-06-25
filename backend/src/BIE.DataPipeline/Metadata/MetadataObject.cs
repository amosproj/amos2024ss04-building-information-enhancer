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
        public List<TableData> Tables { get; set; }

        public BasicData()
        {
            Tables = new List<TableData>();
        }
    }

    public class MetaData
    {
        public string Icon { get; set; }
        public string Type { get; set; }

        /// <summary>
        /// Zoom level is higher the closer you look at something. If current zoom level is below this, it shouldn't display any value.
        /// </summary>
        public int MinZoomLevel { get; set; }

        /// <summary>
        /// The display property is the property that should be shown in a popup.
        /// </summary>
        public string DisplayProperty { get; set; }
    }
    
    public class TableData
    {
        public string Name { get; set; }
        public int NumberOfLines { get; set; }
    }
}