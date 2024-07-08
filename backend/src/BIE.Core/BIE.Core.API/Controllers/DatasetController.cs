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
                if (request.Location.Count <= 0)
                {
                    return BadRequest("at least a single point has to be given");
                }
                else if (request.Location.Count == 1)
                {
                    var coordinate = request.Location.FirstOrDefault().FirstOrDefault();
                    if (coordinate == null)
                    {
                        return BadRequest("Location coordinates are required.");
                    }

                    var latitude = coordinate.ElementAt(0);
                    var longitude = coordinate.ElementAt(1);
                    var results = new List<DatasetItem>();

                    var radius = 10000000; // Define the radius as needed
                    var columns = new HashSet<string> { "operator", "Id" };
                    var metadata = MetadataDbHelper.GetMetadata("EV_charging_stations");
                    if (metadata != null)
                    {
                        var tables = metadata.additionalData.Tables;
                        if (tables.Count == 1)
                        {
                            var chargingStations = getNclosestObjects(tables[0].Name, longitude, latitude, radius, 3, columns);

                            // Log chargingStations to inspect the content
                            foreach (var station in chargingStations)
                            {
                                _logger.LogInformation($"Station data: {string.Join(", ", station.Select(kv => $"{kv.Key}: {kv.Value}"))}");
                            }

                            var chargingStationsData = chargingStations.Select(h => new DatasetItem
                            {
                                Id = h.ContainsKey("Id") ? h["Id"] : null,
                                Key = "Charging station: " + (h.ContainsKey("operator") ? h["operator"] : "No operator defined"),
                                Value = h.ContainsKey("Distance") ? h["Distance"] : "-1 no dist",
                                MapId = "EV_charging_stations",
                            }).ToList();

                            results.AddRange(chargingStationsData);
                        }
                    }

                    // Additional section for house footprints omitted for brevity

                    LocationDataResponse locationDataResponse = new()
                    {
                        CurrentDatasetData = results
                    };

                    return Ok(locationDataResponse);
                }
                return StatusCode(500, $"Currently unsupported operation");
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

            const string epsg27700 = @"PROJCS[""North_America_Albers_Equal_Area_Conic"",
                        GEOGCS[""NAD83"",
                            DATUM[""North_American_Datum_1983"",
                                SPHEROID[""GRS 1980"",6378137,298.257222101,
                                    AUTHORITY[""EPSG"",""7019""]],
                                AUTHORITY[""EPSG"",""6269""]],
                            PRIMEM[""Greenwich"",0,
                                AUTHORITY[""EPSG"",""8901""]],
                            UNIT[""degree"",0.0174532925199433,
                                AUTHORITY[""EPSG"",""9122""]],
                            AUTHORITY[""EPSG"",""4269""]],
                        PROJECTION[""Albers_Conic_Equal_Area""],
                        PARAMETER[""latitude_of_center"",40],
                        PARAMETER[""longitude_of_center"",-96],
                        PARAMETER[""standard_parallel_1"",20],
                        PARAMETER[""standard_parallel_2"",60],
                        PARAMETER[""false_easting"",0],
                        PARAMETER[""false_northing"",0],
                        UNIT[""metre"",1,
                            AUTHORITY[""EPSG"",""9001""]],
                        AXIS[""Easting"",EAST],
                        AXIS[""Northing"",NORTH],
                        AUTHORITY[""ESRI"",""102008""]
                        "; // see http://epsg.io/27700

            var epsg27700_crs = CoordinateSystemWktReader
                .Parse(epsg27700);
            var wgs84 = GeographicCoordinateSystem.WGS84;
            // Create coordinate transformation
            var mTransformation =
                new CoordinateTransformationFactory().CreateFromCoordinateSystems(wgs84, (CoordinateSystem)epsg27700_crs);

            var windsorCastle = new Coordinate(51.483884, -0.604455);
            var buckinghamPalace = new Coordinate(51.501576, -0.141208);
            double[] basic = { -0.604455, 51.483884 };
            double[] epsg27700Point = mTransformation.MathTransform.Transform(basic);

            double[] basic2 = { -0.141208, 51.501576 };
            double[] epsg27700Point2 = mTransformation.MathTransform.Transform(basic2);

            // Extract latitude and longitude
            double latitude1 = epsg27700Point[1];
            double longitude1 = epsg27700Point[0];

            double latitude2 = epsg27700Point2[1];
            double longitude2 = epsg27700Point2[0];

            var distance = new Coordinate(longitude1, latitude1).Distance(new Coordinate(longitude2, latitude2));
            _logger.LogInformation($"windsor castle: {mTransformation.MathTransform.Transform(basic)}");
            _logger.LogInformation($"distance: {distance}");

            double[] sourceInWGS84_arr = {longitude,latitude };
            double[] sourceInEPSG27700_arr = mTransformation.MathTransform.Transform(sourceInWGS84_arr);
            Coordinate sourceInEPSG27700 = new Coordinate(sourceInEPSG27700_arr[0], sourceInEPSG27700_arr[1]);
            _logger.LogInformation($"sourceInWGS84_arr: [{string.Join(", ", sourceInWGS84_arr)}] and sourceInEPSG27700_arr: [{string.Join(", ", sourceInEPSG27700_arr)}] and sourceInEPSG27700: {sourceInEPSG27700}");

            foreach (var result in results)
            {
                if (result.ContainsKey("Location"))
                {
                    var wkt = result["Location"];
                    var point2_wrong = GeoReader.Read(wkt) as Point;
                    if (point2_wrong != null)
                    {
                        double[] targetPointInWGS84 = { point2_wrong.Y, point2_wrong.X };
                        double[] targetPointInEpsg27700 = mTransformation.MathTransform.Transform(targetPointInWGS84);
                        Coordinate targetInEPSG27700 = new Coordinate(targetPointInEpsg27700[0], targetPointInEpsg27700[1]);
                        distance = sourceInEPSG27700.Distance(targetInEPSG27700);
                        _logger.LogInformation($"targetPointInWGS84: [{string.Join(", ", targetPointInWGS84)}] and targetPointInEpsg27700: [{string.Join(", ", targetPointInEpsg27700)}] and targetInEPSG27700: {targetInEPSG27700}");

                        result["Distance"] = distance.ToString();
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
            var results = DbHelper.GetData(command);

            var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
            var point1 = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));

            foreach (var result in results)
            {
                if (result.ContainsKey("Location"))
                {
                    var wkt = result["Location"];
                    var polygon = GeoReader.Read(wkt) as Polygon;
                    if (polygon != null)
                    {
                        result["Area"] = polygon.Area.ToString();
                    }
                    else
                    {
                        result["Area"] = "-1";
                    }
                }
                else
                {
                    result["Area"] = "-1";
                }
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
        public List<List<List<float>>> Location { get; set; }
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