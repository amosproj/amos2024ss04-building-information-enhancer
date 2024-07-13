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
using LinqStatistics;

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
                return StatusCode(400, $"Unsupported dataset: {parameters.Id}");
            

            // select the correct Handler
            IDatasetHandler handler;
            switch (metadata.additionalData.DataType)
            {
                case "CITYGML":
                    handler = new ShapeDatasetHandler(metadata);
                    break;
                case "SHAPE":
                    handler = new ShapeDatasetHandler(metadata);
                    break;
                case "CSV":
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

            bool onlySinglePoint = request.Location.Count == 1 && request.Location.ElementAt(0).Count == 1;

            try
            {
                LocationDataResponse data;
                if (onlySinglePoint)
                    data = LoadLocationHelper.loadLocationDataForSinglePoint(request);
                else
                    data = LoadLocationHelper.loadLocationDataforPolygons(request);
                return Ok(data);
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get data");
                return BadRequest(ex.ToString());
            }
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
        public List<DatasetItem> SelectionData { get; set; }
        public List<DatasetItem> IndividualData { get; set; }
        
    }

    public class DatasetItem
    {
        public string DisplayName { get; set; }
        public string DatasetId { get; set; } // Optional  -> for "open in map" functionality

        public double[] Coordinate { get; set; }
        public List<double[]> PolygonCoordinates { get; set; }
        public List<SubdataItem> Subdata { get; set; } = new();
        public string Value { get; set; } // some items may not have subdata and should instead be directly displayed with a value

    }

    public class SubdataItem
    {
        public string Key { get; set; }

        public string Value { get; set; }

    }

}