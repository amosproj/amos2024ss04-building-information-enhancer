using System.Text.Json.Serialization;

namespace APIGateway.Models
{
    public class LocationDataRequest
    {
        [JsonPropertyName("datasetId")]
        public string DatasetId { get; set; }
        [JsonPropertyName("location")]
        public List<List<List<double>>> Location { get; set; }
    }

    public class Coordinate
    {
        public double Latitude { get; set; } = 0;
        public double Longitude { get; set; } = 0;
    }

    public class LocationDataResponse
    {
        public List<DatasetItem> IndividualData { get; set; } = new List<DatasetItem>();
        public List<DatasetItem> GeneralData { get; set; } = new List<DatasetItem>();
    }

    public class DatasetItem
    {
        public string Id { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string MapId { get; set; } = string.Empty; // Optional  -> for "open in map" functionality
    }


}
