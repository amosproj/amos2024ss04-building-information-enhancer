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
        public List<DatasetItem> IndividualData { get; set; } = [];
        public List<DatasetItem> SelectionData { get; set; } = [];
    }

    public class DatasetItem
    {
        public string DisplayName { get; set; } = string.Empty;
        public string DatasetId { get; set; } = string.Empty;

        public double[] Coordinate { get; set; } = [];
        public List<double[]> PolygonCoordinates { get; set; } = [];
        public List<SubdataItem> Subdata { get; set; } = [];
        public string Value { get; set; } = string.Empty; // some items may not have subdata and should instead be directly displayed with a value

    }

    public class SubdataItem
    {
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

    }


}
