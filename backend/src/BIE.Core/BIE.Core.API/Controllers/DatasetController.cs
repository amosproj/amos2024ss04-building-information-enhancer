﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BIE.Core.BaseRepository;
using BIE.Core.DataObjects;
using BIE.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BIE.Core.DBRepository;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using Accord.MachineLearning;
using Newtonsoft.Json;
using System.Text;
using System.Data;
using System.ComponentModel.DataAnnotations;

namespace BIE.Core.API.Controllers
{
    /// <summary>
    /// Batch controller
    /// </summary>
    [Route(Global.API_CONTROLLER)]
    [ApiController]
    public class DatasetController : Controller
    {
        private readonly ILogger<DatasetController> _logger;

        public DatasetController(ILogger<DatasetController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get viewport data. so Data for a specific rectangle returned as featurecollection
        /// </summary>
        /// <returns></returns>
        [HttpGet("getDatasetViewportData")]
        [ProducesResponseType(typeof(GeoJsonFeatureCollection), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public ActionResult GetDatasetViewportData([FromQuery] QueryParameters parameters)
        {
            _logger.LogInformation("Received request for GetDatasetViewportData with parameters: {parameters}", parameters);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state: {ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            switch (parameters.Id)
            {
                case "house_footprints":
                    return GetHouseFootprintsData(parameters);
                case "EV_charging_stations":
                    return GetChargingStations(parameters);
                default:
                    _logger.LogWarning("Unsupported dataset ID: {Id}", parameters.Id);
                    return StatusCode(400, $"Unsupported dataset ID of {parameters.Id}");
            }
        }

        private ActionResult GetChargingStations(QueryParameters parameters)
        {
            _logger.LogInformation("Fetching charging stations with parameters: {parameters}", parameters);
            try
            {
                var bottomLat = parameters.BottomLat;
                var bottomLong = parameters.BottomLong;
                var topLat = parameters.TopLat;
                var topLong = parameters.TopLong;
                
                DbHelper.CreateDbConnection();

                var command =
                    "SELECT * " +
                    "FROM dbo.EV_charging_stations" +
                    $" WHERE latitude > {bottomLat} AND latitude < {topLat} AND" +
                    $" longitude > {bottomLong} AND longitude < {topLong};";

                command = command.Replace(",", ".");
                var response = "{\"type\": \"FeatureCollection\",\n\"features\": [";
                foreach (var row in DbHelper.GetData(command))
                {
                    response += $@"
{{
  ""type"": ""Feature"",
  ""geometry"": {{
    ""type"": ""Point"",
    ""coordinates"": [{row["longitude"]}, {row["latitude"]}]
  }},
  ""properties"": {{
    ""name"": ""{row["operator"]}""
}}
}},";
                }

                if (response.Last() == ',')
                {
                    response = response[..^1];
                }

                response += "]}";

                _logger.LogInformation("Charging stations data fetched successfully.");
                return Ok(response);
            }
            catch (ServiceException se)
            {
                _logger.LogError(se, "ServiceException occurred while fetching charging stations data.");
                return BadRequest(se.Message);
            }
        }

        private ActionResult GetHouseFootprintsData([FromQuery] QueryParameters parameters)
        {
            _logger.LogInformation("Fetching house footprints with parameters: {parameters}", parameters);
            try
            {
                var bottomLat = parameters.BottomLat;
                var bottomLong = parameters.BottomLong;
                var topLat = parameters.TopLat;
                var topLong = parameters.TopLong;

                DbHelper.CreateDbConnection();

                string command = @"
         SELECT Id, Location.STAsText() AS Location, Location.STGeometryType() AS Type
         FROM dbo.Hausumringe_mittelfranken_small
         WHERE Location.STIntersects(geography::STGeomFromText('POLYGON((
            {0} {1},
            {0} {3},
            {2} {3},
            {2} {1},
            {0} {1}
         ))', 4326)) = 1";

                string formattedQuery = string.Format(command, bottomLong, bottomLat, topLong, topLat);
                var response = "{\"type\": \"FeatureCollection\",\n\"features\": [";
                foreach (var row in DbHelper.GetData(formattedQuery))
                {
                    response += $@"
{{
  ""type"": ""Feature"",
  ""geometry"": {{
    ""type"": ""{row["Type"]}"",
    ""coordinates"": [{QueryParameters.GetPolygonCordinates(row["Location"])}]
  }},
  ""properties"": {{
    ""id"": ""{row["Id"]}""
}}
}},";
                }

                if (response.Last() == ',')
                {
                    response = response[..^1];
                }

                response += "]}";

                _logger.LogInformation("House footprints data fetched successfully.");
                return Ok(response);
            }
            catch (ServiceException se)
            {
                _logger.LogError(se, "ServiceException occurred while fetching house footprints data.");
                return BadRequest(se.Message);
            }
        }

        /// <summary>
        /// Loads the location data for the given point or polygon.
        /// </summary>
        /// <param name="request">Contains the current dataset id and the list of coordinates. 
        /// In case of a single point a list with a single element.</param>
        /// <returns>Data for the specified point/polygon as a list of key/values.</returns>
        [HttpPut("loadLocationData")]
        [ProducesResponseType(typeof(LocationDataResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public IActionResult LoadLocationData([FromBody, Required] LocationDataRequest request)
        {
            _logger.LogInformation("Received request to load location data: {request}", request);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state: {ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                var coordinate = request.Location.FirstOrDefault();
                if (coordinate == null)
                {
                    _logger.LogWarning("Location coordinates are required.");
                    return BadRequest("Location coordinates are required.");
                }

                var latitude = coordinate.Latitude;
                var longitude = coordinate.Longitude;
                var radius = 10000000; // Define the radius as needed

                DbHelper.CreateDbConnection();

                string command = @"
            SELECT TOP 5
                Id,
                Location.STArea() AS Area,
                Location.STAsText() AS Location,
                geography::Point({0}, {1}, 4326).STDistance(Location) AS Distance
            FROM 
                dbo.Hausumringe_mittelfranken
            WHERE
                geography::Point({0}, {1}, 4326).STBuffer({2}).STIntersects(Location) = 1
            ORDER BY 
                geography::Point({0}, {1}, 4326).STDistance(Location);";

                string formattedQuery = string.Format(command, latitude, longitude, radius);
                var response = new LocationDataResponse
                {
                    CurrentDatasetData = new List<DatasetItem>()
                };

                foreach (var row in DbHelper.GetData(formattedQuery, 600))
                {
                    var area = row["Area"];
                    var distance = row["Distance"];
                    var location = row["Location"];

                    response.CurrentDatasetData.Add(new DatasetItem
                    {
                        Id = row["Id"].ToString(),
                        Key = location,
                        Value = $"{distance}m, {area}m^2",
                        MapId = "house_footprints"
                    });
                }

                _logger.LogInformation("Location data loaded successfully.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while loading location data.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// WIP: DO NOT USE, Get a record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("3/data")]
        public ActionResult GetActualUseData([FromQuery] QueryParameters parameters)
        {
            _logger.LogInformation("Fetching actual use data with parameters: {parameters}", parameters);
            try
            {
                var bottomLat = parameters.BottomLat;
                var bottomLong = parameters.BottomLong;
                var topLat = parameters.TopLat;
                var topLong = parameters.TopLong;
                var stateName = parameters.StateName;

                DbHelper.CreateDbConnection();

                string command = @"
         SELECT top 1000 Id, Location.STAsText() AS Location, Location.STGeometryType() AS Type
         FROM dbo.actual_use_{4}
         WHERE Location.STIntersects(geography::STGeomFromText('POLYGON((
            {0} {1},
            {0} {3},
            {2} {3},
            {2} {1},
            {0} {1}
         ))', 4326)) = 1";

                string formattedQuery = string.Format(command, bottomLong, bottomLat, topLong, topLat, stateName);
                List<object> features = new List<object>();
                foreach (var row in DbHelper.GetData(formattedQuery))
                {
                    var feature = new
                    {
                        type = "Feature",
                        geometry = new
                        {
                            type = (string)row["Type"],
                            coordinates = QueryParameters.GetPolygonCordinates(row["Location"])
                        },
                        properties = new
                        {
                            id = (string)row["Id"]
                        }
                    };
                    features.Add(feature);
                }

                var featureCollection = new
                {
                    type = "FeatureCollection",
                    features = features
                };

                var jsonResponse = JsonConvert.SerializeObject(featureCollection);
                _logger.LogInformation("Actual use data fetched successfully.");
                return Ok(jsonResponse);
            }
            catch (ServiceException se)
            {
                _logger.LogError(se, "ServiceException occurred while fetching actual use data.");
                return BadRequest(se.Message);
            }
        }

        /// <summary>
        /// Get a record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("2/clustereddata")]
        public ActionResult GetHausumringeClusteredData([FromQuery] QueryParameters parameters)
        {
            _logger.LogInformation("Fetching clustered data with parameters: {parameters}", parameters);
            try
            {
                var bottomLat = parameters.BottomLat;
                var bottomLong = parameters.BottomLong;
                var topLat = parameters.TopLat;
                var topLong = parameters.TopLong;
                var spatialDataList = new List<SpatialData>();

                DbHelper.CreateDbConnection();

                string command = @"
         SELECT Top(1000) Id, Location.STAsText() AS Location, Location.STGeometryType() AS Type
         FROM dbo.Hausumringe_mittelfranken
         WHERE Location.STIntersects(geography::STGeomFromText('POLYGON((
            {0} {1},
            {0} {3},
            {2} {3},
            {2} {1},
            {0} {1}
         ))', 4326)) = 1";

                string formattedQuery = string.Format(command, bottomLong, bottomLat, topLong, topLat);
                foreach (var row in DbHelper.GetData(formattedQuery))
                {
                    var location = new List<double[]>();
                    try
                    {
                         location = row["Location"]
                     .Replace("POLYGON ((", "").Replace("))", "")
                     .Split(',')
                     .Select(coord => coord.Trim().Split(' ')
                     .Select(double.Parse).ToArray()).ToList();

                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error parsing location coordinates.");
                        continue;
                    }
                    spatialDataList.Add(new SpatialData
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Coordinates = location,
                        Type = row["Type"]
                    });
                }
                if (spatialDataList.Count == 0)
                {
                    _logger.LogInformation("No spatial data found.");
                    return Ok("{\"type\":\"FeatureCollection\",\"features\":[]}");
                }
                var clusters = QueryParameters.ClusterData(spatialDataList, Convert.ToInt32(parameters.ZoomLevel));
                var geoJson = QueryParameters.ConvertToGeoJson(clusters);
                _logger.LogInformation("Clustered data fetched successfully.");
                return Ok(JsonConvert.SerializeObject(geoJson));
            }
            catch (ServiceException se)
            {
                _logger.LogError(se, "ServiceException occurred while fetching clustered data.");
                return BadRequest(se.Message);
            }
        }

        public class SpatialData
        {
            public int Id { get; set; }
            public List<double[]> Coordinates { get; set; }  
            public string Type { get; set; }
            public int ClusterId { get; set; } 
        }


        public class GeoJsonFeature
        {
            public string Type { get; set; } = "Feature";
            public JObject Geometry { get; set; }
            public JObject Properties { get; set; }
        }

        public class GeoJsonFeatureCollection
        {
            public string Type { get; set; } = "FeatureCollection";
            public List<GeoJsonFeature> Features { get; set; } = new List<GeoJsonFeature>();
        }

        public class QueryParameters
        {
            [BindRequired] public string Id { get; set; }
            [BindRequired] public float BottomLat { get; set; }
            [BindRequired] public float BottomLong { get; set; }
            [BindRequired] public float TopLat { get; set; }
            [BindRequired] public float TopLong { get; set; }
            public string StateName { get; set; }

            public float ZoomLevel { get; set; }

            public static string GetPolygonCordinates(string cordinate)
            {
                cordinate = cordinate.Replace("POLYGON ((", "");
                cordinate = cordinate.Replace("))", "");
                var lstcordinate = cordinate.Split(',');
                for (int i = 0;i<lstcordinate.Length; i++)
                {
                    lstcordinate[i] = lstcordinate[i].Replace(" ",",");

                }

                cordinate = string.Join("],[", lstcordinate);
                cordinate ="["+cordinate+"]";
                return cordinate;
            }
            public static double[] CalculateCentroid(List<double[]> coordinates)
            {
                double centroidX = 0, centroidY = 0;
                int pointCount = coordinates.Count;

                foreach (var point in coordinates)
                {
                    centroidX += point[0];
                    centroidY += point[1];
                }

                return new double[] { centroidX / pointCount, centroidY / pointCount };
            }
            public static List<List<SpatialData>> ClusterData(List<SpatialData> data, int numberOfClusters)
            {
                var centroids = data.Select(d => CalculateCentroid(d.Coordinates)).ToArray();
                var kmeans = new KMeans(numberOfClusters);
                var clusters = kmeans.Learn(centroids);
                int[] labels = clusters.Decide(centroids);

                for (int i = 0; i < labels.Length; i++)
                {
                    data[i].ClusterId = labels[i];  
                }

                var clusteredData = new List<List<SpatialData>>();
                for (int i = 0; i < numberOfClusters; i++)
                {
                    clusteredData.Add(new List<SpatialData>());
                }

                for (int i = 0; i < labels.Length; i++)
                {
                    clusteredData[labels[i]].Add(data[i]);
                }

                return clusteredData;
            }


            public static GeoJsonFeatureCollection ConvertToGeoJson(List<List<SpatialData>> clusters)
            {
                var featureCollection = new GeoJsonFeatureCollection();

                foreach (var cluster in clusters)
                {
                    foreach (var spatialData in cluster)
                    {
                        var coordinates = new JArray(spatialData.Coordinates.Select(coord => new JArray(coord)));

                        var geometry = new JObject
                        {
                            ["type"] = spatialData.Type,
                            ["coordinates"] = new JArray(coordinates)
                        };

                        var properties = new JObject
                        {
                            ["Id"] = spatialData.Id,
                            ["ClusterId"] = spatialData.ClusterId
                        };

                        var feature = new GeoJsonFeature
                        {
                            Geometry = geometry,
                            Properties = properties
                        };

                        featureCollection.Features.Add(feature);
                    }
                }

                return featureCollection;
            }

        }

    }

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
        public string Id { get; set; }

        public string Key { get; set; }
        public string Value { get; set; }
        public string MapId { get; set; } // Optional  -> for "open in map" functionality
    }
}