using Microsoft.AspNetCore.Mvc;
using BIE.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using BIE.Core.DBRepository;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using Accord.MachineLearning;
using Newtonsoft.Json;
using System.Data;
using System.ComponentModel.DataAnnotations;

using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;

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

                // Create polygon WKT from bounding box
                var polygonWkt = $"POLYGON(({bottomLong.ToString(new CultureInfo("en-US"))} {bottomLat.ToString(new CultureInfo("en-US"))}, {topLong.ToString(new CultureInfo("en-US"))} {bottomLat.ToString(new CultureInfo("en-US"))}, {topLong.ToString(new CultureInfo("en-US"))} {topLat.ToString(new CultureInfo("en-US"))}, {bottomLong.ToString(new CultureInfo("en-US"))} {topLat.ToString(new CultureInfo("en-US"))}, {bottomLong.ToString(new CultureInfo("en-US"))} {bottomLat.ToString(new CultureInfo("en-US"))}))";

                // SQL Query to find intersecting points
                var sqlQuery = $@"
        SELECT top 1000  operator, Location.AsTextZM() AS Location
        FROM dbo.EV_charging_stations
        WHERE Location.STIntersects(geometry::STGeomFromText('{polygonWkt}', 4326)) = 1;
        ";

                Console.WriteLine(sqlQuery);
                // Get data from database
                var data = DbHelper.GetData(sqlQuery);

                // Construct GeoJSON response
                var response = new StringBuilder();
                response.Append("{\"type\": \"FeatureCollection\",\n\"features\": [");

                foreach (var row in data)
                {
                    var location = row["Location"].ToString();
                    // Extract the coordinates from the POINT string
                    var coordinates = location.Replace("POINT (", "").Replace(")", "").Split(' ');
                    var longitude = coordinates[0];
                    var latitude = coordinates[1];

                    response.Append($@"
            {{
                ""type"": ""Feature"",
                ""geometry"": {{
                    ""type"": ""Point"",
                    ""coordinates"": [{longitude}, {latitude}]
                }},
                ""properties"": {{
                    ""name"": ""{row["operator"]}""
                }}
            }},");
                }

                if (response[response.Length - 1] == ',')
                {
                    response.Length--; // Remove the trailing comma
                }

                response.Append("]}");

                return Ok(response.ToString());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        private ActionResult GetHouseFootprintsData([FromQuery] QueryParameters parameters)
        {
            _logger.LogInformation("Fetching house footprints with parameters: {parameters}", parameters);
            try
            {
                var bottomLong = parameters.BottomLat;
                var bottomLat = parameters.BottomLong;
                var topLong = parameters.TopLat;
                var topLat = parameters.TopLong;

                DbHelper.CreateDbConnection();

                // Create polygon WKT from bounding box
                var polygonWkt = $"POLYGON(({bottomLong.ToString(new CultureInfo("en-US"))} {bottomLat.ToString(new CultureInfo("en-US"))}, {topLong.ToString(new CultureInfo("en-US"))} {bottomLat.ToString(new CultureInfo("en-US"))}, {topLong.ToString(new CultureInfo("en-US"))} {topLat.ToString(new CultureInfo("en-US"))}, {bottomLong.ToString(new CultureInfo("en-US"))} {topLat.ToString(new CultureInfo("en-US"))}, {bottomLong.ToString(new CultureInfo("en-US"))} {bottomLat.ToString(new CultureInfo("en-US"))}))";

                // SQL Query to find intersecting points
                var sqlQuery = $@"
SELECT top 1000  Location.AsTextZM() AS Location, Location.STGeometryType() AS Type
FROM dbo.Hausumringe_mittelfranken_small
WHERE Location.STIntersects(geometry::STGeomFromText('{polygonWkt}', 4326)) = 1;
";
                Console.WriteLine(sqlQuery);
                // Console.WriteLine(command);
                var response = "{\"type\": \"FeatureCollection\",\n\"features\": [";
                foreach (var row in DbHelper.GetData(sqlQuery))
                {
                    response += $@"
{{
  ""type"": ""Feature"",
  ""geometry"": {{
    ""type"": ""{row["Type"]}"",
    ""coordinates"": [{QueryParameters.GetPolygonCordinates(row["Location"])}]
  }},
  ""properties"": {{
    ""text"": ""{row["Type"]}""
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var coordinate = request.Location.FirstOrDefault();
                if (coordinate == null)
                {
                    return BadRequest("Location coordinates are required.");
                }

                var latitude = coordinate.Latitude;
                var longitude = coordinate.Longitude;
                var radius = 10000000; // Define the radius as needed
                var buffer = 1000; // Define the buffer as needed

                var houseFootprintsData = QueryParameters.GetHouseFootprints(latitude, longitude, radius);
                var evChargingStationsData = QueryParameters.GetEVChargingStations(latitude, longitude, buffer);

                var houseFootprints = houseFootprintsData.SelectMany(h =>
                    h.Select(kv => new DatasetItem
                    {
                        Id = h.ContainsKey("Id") ? h["Id"] : null,
                        Key = kv.Key,
                        Value = kv.Value,
                        MapId = h.ContainsKey("MapId") ? h["MapId"] : null // Optional
                    })
                ).ToList();

                var evChargingStations = evChargingStationsData.SelectMany(e =>
                    e.Select(kv => new DatasetItem
                    {
                        Id = e.ContainsKey("Id") ? e["Id"] : null,
                        Key = kv.Key,
                        Value = kv.Value,
                        MapId = e.ContainsKey("MapId") ? e["MapId"] : null // Optional
                    })
                ).ToList();

                var response = new
                {
                    HouseFootprints = houseFootprints,
                    EVChargingStations = evChargingStations
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
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
         FROM dbo.Hausumringe_mittelfranken_small
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
                    lstcordinate[i] = lstcordinate[i].Trim().Replace(" ",",");
                    var pairOfCoordinates = lstcordinate[i].Split(',');
                    lstcordinate[i] = pairOfCoordinates[1] + ',' + pairOfCoordinates[0];
                    lstcordinate[i] = lstcordinate[i].Trim().Replace("(", "").Replace(")", "");
                }

                cordinate = string.Join("],[", lstcordinate);
                cordinate ="[["+cordinate+"]]";
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

            public static List<Dictionary<string, string>> GetEVChargingStations(double latitude, double longitude, int buffer)
            {
                string command = @"
        SELECT TOP 5
            Id,
            Location.STAsText() AS Location,
            geometry::Point({0}, {1}, 4326).STDistance(Location) AS Distance
        FROM 
            dbo.EV_charging_stations
        WHERE
            geometry::Point({0}, {1}, 4326).STBuffer({2}).STIntersects(Location) = 1
        ORDER BY 
            geometry::Point({0}, {1}, 4326).STDistance(Location);";

                string formattedQuery = string.Format(command, latitude, longitude, buffer);
                return DbHelper.GetData(formattedQuery).ToList();
            }

            public static List<Dictionary<string, string>> GetHouseFootprints(double latitude, double longitude, int radius)
            {
                string command = @"
        SELECT TOP 5
            Id,
            Location.STArea() AS Area,
            Location.STAsText() AS Location,
            geometry::Point({0}, {1}, 4326).STDistance(Location) AS Distance
        FROM 
            dbo.Hausumringe_mittelfranken_small
        WHERE
            geometry::Point({0}, {1}, 4326).STBuffer({2}).STIntersects(Location) = 1
        ORDER BY 
            geometry::Point({0}, {1}, 4326).STDistance(Location);";

                string formattedQuery = string.Format(command, latitude, longitude, radius);
                return DbHelper.GetData(formattedQuery).ToList();
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