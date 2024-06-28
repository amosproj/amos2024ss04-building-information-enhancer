﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
// ReSharper disable InconsistentNaming

namespace BieMetadata;

public class MetadataObject
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string _id { get; set; } = string.Empty;
    
    [BsonElement("basicData")]
    public BasicData basicData { get; set; } = new BasicData();

    [BsonElement("additionalData")]
    public AdditionalData additionalData { get; set; } = new AdditionalData();
    
    /// <summary>
    /// The general and most important data about a dataset.
    /// </summary>
    public class BasicData
    {
        /// <summary>
        /// The Id of the dataset
        /// </summary>
        public string DatasetId { get; set; } = string.Empty;
        
        /// <summary>
        /// The displayname of the dataset
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// a short description of the dataset
        /// </summary>
        public string ShortDescription { get; set; } = string.Empty;
        
        /// <summary>
        /// the icon used to display dataset
        /// </summary>
        public string Icon { get; set; } = string.Empty;
    }

    /// <summary>
    /// The additional data for each of the datasets, queried at a request.
    /// </summary>
    public class AdditionalData
    {
        public string Icon { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// the Datatype of the dataset. Used to determine how to handle Dataset
        /// </summary>
        public string DataType { get; set; } = string.Empty;

        public string LongDescription { get; set; } = string.Empty;

        /// <summary>
        /// Zoom level is higher the closer you look at something. If current zoom level is below this, it shouldn't display any value.
        /// </summary>
        public int MinZoomLevel { get; set; } = 0;
        
        /// <summary>
        /// The zoom threshold where areas start to turn into markers
        /// </summary>
        public int MarkersThreshold { get; set; } = 0;

        /// <summary>
        /// The display property is the property that should be shown in a popup.
        /// </summary>
        public string DisplayProperty { get; set; } = string.Empty;

        /// <summary>
        /// Table data populated by the data pipeline. Contains the name and the size of the all .yaml files correlated to that specific dataset.
        /// </summary>
        public List<TableData> Tables { get; set; } = new List<TableData>();
    }
    
    /// <summary>
    /// Table data populated by the data pipeline. Contains the name and the size of the all .yaml files correlated to that specific dataset.
    /// </summary>
    public class TableData
    {
        // The name of the .yaml file
        public string Name { get; set; } = string.Empty;
        // The number of lines of data in that file.
        public int NumberOfLines { get; set; } = 0;
        
        public BoundingBox? BoundingBox { get; set; }
    }
}
