﻿namespace APIGateway.Models
{
    public class LocationDataRequest
    {
        public string DatasetId { get; set; } = string.Empty;
        public List<Coordinate> Location { get; set; } = new List<Coordinate>();
    }

    public class Coordinate
    {
        public double Latitude { get; set; } = 0;
        public double Longitude { get; set; } = 0;
    }

    public class LocationDataResponse
    {
        public List<DatasetItem> CurrentDatasetData { get; set; } = new List<DatasetItem>();
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
