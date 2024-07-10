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
using BIE.Core.API.DatasetHandlers;
using BieMetadata;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;
using Microsoft.AspNetCore.Http;
using NetTopologySuite;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;
using ProjNet.IO.CoordinateSystems;
using static System.Collections.Specialized.BitVector32;
using NetTopologySuite.Algorithm;
using MySqlX.XDevAPI.Relational;
using Org.BouncyCastle.Asn1.Cms;
using MySqlX.XDevAPI.Common;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;
using Accord.Math;

namespace BIE.Core.API.Controllers
{
    /// <summary>
    /// Batch controller
    /// </summary>
    [Route(Global.API_CONTROLLER)]
    [ApiController]
    public class DatasetController : Controller
    {
        // ReSharper disable once InconsistentNaming
        private readonly ILogger<DatasetController> _logger;

        private MetadataDbHelper mMetadataDbHelper;
        private static readonly WKTReader GeoReader = new(new NtsGeometryServices(new PrecisionModel(), 4326));
        private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 4326);


        private MetadataDbHelper MetadataDbHelper
        {
            get
            {
                if (mMetadataDbHelper != null)
                {
                    return mMetadataDbHelper;
                }

                mMetadataDbHelper = new MetadataDbHelper();
                if (mMetadataDbHelper.CreateConnection())
                {
                    return mMetadataDbHelper;
                }

                Console.WriteLine("could not establish Metadata-database Connection");
                throw new Exception("no metadata DB reachable!");
            }
        }

        public DatasetController(ILogger<DatasetController> logger)
        {
            _logger = logger;

            Console.WriteLine("setting up Dataset Controller");
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
            _logger.LogInformation($"Received request for GetDatasetViewportData with parameters: {parameters}");
            Console.WriteLine("Get Viewport Data");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"Invalid model state: {ModelState}");
                return BadRequest(ModelState);
            }

            // check if the dataset is present in the Metadata
            var metadata = MetadataDbHelper.GetMetadata(parameters.Id);
            if (metadata == null)
            {
                return StatusCode(400, $"Unsupported dataset: {parameters.Id}");
            }

            // select the correct Handler
            IDatasetHandler handler;
            switch (metadata.additionalData.DataType)
            {
                case "SHAPE":
                    handler = new ShapeDatasetHandler(metadata);
                    break;
                case "CSV":
                    // TODO
                    handler = new CsvDatasetHandler(metadata);
                    break;
                default:
                    Console.WriteLine($"Datatype {metadata.additionalData.DataType} is not known.");
                    return StatusCode(400, $"Unsupported dataset type: {metadata.additionalData.DataType}");
            }

            var boundingBox = ApiHelper.GetBoundingBoxFromParameters(parameters);
            return Ok(handler.GetDataInsideArea(boundingBox));
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
                var generalData = new List<DatasetItem>();
                var individualData = new List<DatasetItem>();

                // can still be called even if we have a single point
                var polygons = ApiHelper.ConvertToWktPolygons(request.Location);


                if (request.Location.Count <= 0)
                {
                    return BadRequest("at least a single point has to be given");
                }

                bool onlySinglePoint = request.Location.Count == 1 && request.Location.ElementAt(0).Count == 1;

                Point pointForCalculations = new Point(new Coordinate());
                if (onlySinglePoint)
                {
                    var p = request.Location.First().First();
                    pointForCalculations = new Point(p.ElementAt(0), p.ElementAt(1));
                }
                else
                {
                    // Create a WKTReader instance
                    WKTReader reader = new WKTReader();
                    Geometry geometry = reader.Read(polygons.First());
                    if (geometry is Polygon polygon)
                        pointForCalculations = polygon.Centroid;

                }

                // closest charging stations and number of charging stations
                var radius = 10000000; // Define the radius as needed
                var columns = new HashSet<string> { "operator", "Id", "rated_power_kw" };
                var metadata = MetadataDbHelper.GetMetadata("EV_charging_stations");
                if (metadata != null)
                {
                    var tables = metadata.additionalData.Tables;
                    if (tables.Count == 1)
                    {
                        var chargingStations = getNclosestObjects(tables[0].Name, pointForCalculations.Y, pointForCalculations.X, radius, 3, columns);

                        // Log chargingStations to inspect the content
                        foreach (var station in chargingStations)
                        {
                            _logger.LogInformation($"Station data: {string.Join(", ", station.Select(kv => $"{kv.Key}: {kv.Value}"))}");
                        }

                        var chargingStationsData = chargingStations.Select(h => new DatasetItem
                        {
                            Id = h.ContainsKey("Id") ? h["Id"] : null,
                            Key = "Charging station: " + (h.ContainsKey("operator") ? h["operator"] : "No operator defined"),
                            Value = (h.ContainsKey("Distance") ? h["Distance"] + "m" : "-1") + " | " + (h.ContainsKey("rated_power_kw") ? h["rated_power_kw"] + "kw" : "-1"),
                            MapId = "EV_charging_stations",
                        });

                        individualData.AddRange(chargingStationsData);
                    }
                }


                var housefootprints = new List<Dictionary<String, String>>();
                if(onlySinglePoint)
                {
                    // Additional section for house footprints 
                    columns = new HashSet<string> { "Id" };
                    metadata = MetadataDbHelper.GetMetadata("house_footprints");
                    if (metadata != null)
                    {
                        var p = request.Location.First().First();
                        foreach (var table in metadata.additionalData.Tables)
                            housefootprints.AddRange(getMatchingObjects(table.Name, p.ElementAt(1), p.ElementAt(0), columns));
                        
                    }
                }
                else
                {
                    _logger.LogInformation("Do housefootprints for area");

                    foreach (var polygonWkt in polygons)
                    {
                        // then go through house footprints
                        metadata = MetadataDbHelper.GetMetadata("house_footprints");
                        if (metadata != null)
                        {
                            _logger.LogInformation("metadata from housefootprints retrieved");
                            foreach (var table in metadata.additionalData.Tables)
                            {
                                var sqlQuery = $"SELECT Id, Location.STAsText() AS Location" +
                                    ApiHelper.FromTableIntersectsPolygon(table.Name, polygonWkt);
                                _logger.LogInformation(sqlQuery);

                                housefootprints.AddRange(DbHelper.GetData(sqlQuery));
                            }
                        }

                    }                    
                }
                double totalBuildingArea = 0.0;
                if (housefootprints.Any())
                {
                    var housefootprintsData = housefootprints.Select(h => new DatasetItem
                    {
                        Id = h.ContainsKey("Id") ? h["Id"] : null,
                        Key = "House footprints",
                        Value = h.ContainsKey("Location") ? ApiHelper.getArea(GeoReader.Read(h["Location"]) as Polygon).ToString("0.##") + "m²" : "-1",
                        MapId = "House footprints",
                    });
                    individualData.AddRange(housefootprintsData);
                    
                    long totalCountHouseFootprints = housefootprints.Count();
                    long totalCountChargingStations = 0l;
                    foreach (var hdata in housefootprints)
                    {
                        
                            totalBuildingArea += hdata.ContainsKey("Location") ? ApiHelper.getArea(GeoReader.Read(hdata["Location"]) as Polygon) : 0;
                        
                    }
                    if (!onlySinglePoint)
                    {
                        generalData.Add(new DatasetItem
                        {
                            Id = "Total number of buildings in area",
                            Key = "Total number of buildings in area",
                            Value = totalCountHouseFootprints.ToString(),
                            MapId = ""
                        });
                    }
                    generalData.Add(new DatasetItem
                    {
                        Id = "Total building",
                        Key = "Total building area",
                        Value = totalBuildingArea.ToString("0.##") + "m²",
                        MapId = ""
                    });
                }


                //////////////
                if (!onlySinglePoint)
                {
                    WKTReader reader = new WKTReader();
                    double totalAreaSearchPolygon = 0.0;
                    foreach(var polygonWkt in polygons)
                    {
                        Geometry geometry = reader.Read(polygonWkt);
                        if (geometry is Polygon polygon)
                            totalAreaSearchPolygon += ApiHelper.getArea(polygon);
                    }
                    generalData.Insert(0, new DatasetItem
                    {
                        Id = "Searched area",
                        Key = "Searched area",
                        Value = totalAreaSearchPolygon.ToString("0.##") + "m²",
                        MapId = ""
                    });
                    generalData.Add(new DatasetItem
                    {
                        Id = "Potential Area for geothermal use",
                        Key = "Potential Area for geothermal use",
                        Value = Math.Max(totalAreaSearchPolygon - totalBuildingArea, 0).ToString("0.##") + "m²",
                        MapId = ""
                    });
                }

                LocationDataResponse locationDataResponse = new()
                {
                    IndividualData = individualData,
                    GeneralData = generalData
                };

                return Ok(locationDataResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private IEnumerable<Dictionary<string, string>> getNclosestObjects(string datasetId, double longitude, double latitude, double radius, int n, IEnumerable<string> columns)
        {
            string columnsString = string.Join(", ", columns);

            string command = $@"
    SELECT TOP {n}
        {columnsString},
        Location.STAsText() AS Location
    FROM 
        dbo.{datasetId}
    WHERE
        geometry::Point({latitude}, {longitude}, 4326).STBuffer({radius}).STIntersects(Location) = 1
    ORDER BY 
        geometry::Point({latitude}, {longitude}, 4326).STDistance(Location);";

            var results = DbHelper.GetData(command).ToList();

            
            foreach (var result in results)
            {
                if (result.ContainsKey("Location"))
                {
                    var wkt = result["Location"];
                    var point2_wrong = GeoReader.Read(wkt) as Point;
                    if (point2_wrong != null)
                    {

                        var distance = ApiHelper.getDistance(longitude, latitude, point2_wrong);
                        result["Distance"] = distance.ToString("0.##");
                    }
                    else
                    {
                        result["Distance"] = "-1";
                    }
                }
                else
                {
                    result["Distance"] = "-1";
                }
            }

            return results;
        }


        //private long getObjectCount(string datasetId, )
        private IEnumerable<Dictionary<string, string>> getMatchingObjects(
        string datasetId,
        double[] longitude,
        double[] latitude,
        IEnumerable<string> columns)
        {
            return new List<Dictionary<string, string>>();
        }

        private IEnumerable<Dictionary<string, string>> getMatchingObjects(
        string datasetId,
        double longitude,
        double latitude,
        IEnumerable<string> columns)
        {
            // Convert the columns set to a comma-separated string
            string columnsString = string.Join(", ", columns);

            // Define the SQL command with the columns string
            string command = $@"
                SELECT TOP 1000
                    {columnsString},
                    Location.STAsText() AS Location
                FROM 
                    dbo.{datasetId}
                WHERE
                    Location.STIntersects(geometry::Point({latitude}, {longitude}, 4326)) = 1
                ORDER BY 
                    geometry::Point({latitude}, {longitude}, 4326).STDistance(Location);";

            _logger.LogInformation(command);

            // Get data from the database
            var results = DbHelper.GetData(command).ToList();

            foreach (var result in results)
            {
                if (result.ContainsKey("Location"))
                {
                    var wkt = result["Location"];
                    var polygon = GeoReader.Read(wkt) as Polygon;
                    if (polygon != null)
                    {
                        result["Area"] = ApiHelper.getArea(polygon).ToString("0.##");
                    }
                    else
                        result["Area"] = "-1";
                    
                }
                else
                    result["Area"] = "-1 no location";
                
            }

            return results;
        }
    

        public static List<List<SpatialData>> ClusterData(List<SpatialData> data, int numberOfClusters)
        {
            var centroids = data.Select(d => QueryParameters.CalculateCentroid(d.Coordinates)).ToArray();
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
                for (int i = 0; i < lstcordinate.Length; i++)
                {
                    lstcordinate[i] = lstcordinate[i].Trim().Replace(" ", ",");
                    var pairOfCoordinates = lstcordinate[i].Split(',');
                    lstcordinate[i] = pairOfCoordinates[1] + ',' + pairOfCoordinates[0];
                    lstcordinate[i] = lstcordinate[i].Trim().Replace("(", "").Replace(")", "");
                }

                cordinate = string.Join("],[", lstcordinate);
                cordinate = "[[" + cordinate + "]]";
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
        [JsonPropertyName("datasetId")]
        public string DatasetId { get; set; }
        [JsonPropertyName("location")]
        public List<List<List<double>>> Location { get; set; }
    }

    public class LocationDataResponse
    {
        public List<DatasetItem> IndividualData { get; set; }
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