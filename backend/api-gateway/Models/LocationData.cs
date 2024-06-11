namespace APIGateway.Models
{
    public class LocationDataRequest
    {
        public string DatasetId { get; set; }
        public List<Coordinate> Location { get; set; }
    }

    public class Coordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class LocationDataResponse
    {
        public List<DatasetItem> CurrentDatasetData { get; set; }
        public List<DatasetItem> GeneralData { get; set; }

    }

    public class DatasetItem
    {
        public string key { get; set; }
        public string value { get; set; }
        public string mapId { get; set; } // Optional  -> for "open in map" functionality
    }


}
