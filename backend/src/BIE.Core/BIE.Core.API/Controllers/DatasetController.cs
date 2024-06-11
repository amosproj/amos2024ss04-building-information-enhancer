using Microsoft.AspNetCore.Mvc;
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

namespace BIE.Core.API.Controllers
{
    /// <summary>
    /// Batch controller
    /// </summary>
    [Route(Global.API_CONTROLLER)]
    [ApiController]
    public class DatasetController : Controller
    {
        /// <summary>
        /// Get a record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("1/data")]
        public ActionResult Get( [FromQuery] QueryParameters parameters)
        {
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
                // Console.WriteLine(command);
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
                    // chop of last ,
                    response = response[..^1];
                }

                response += "]}";

                return Ok(response);
            }
            catch (ServiceException se)
            {
                return BadRequest(se.Message);
            }
        }

        /// <summary>
        /// Get a record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("2/data")]
        public ActionResult GetHausumringeData([FromQuery] QueryParameters parameters)
        {
            try
            {
                var bottomLat = parameters.BottomLat;
                var bottomLong = parameters.BottomLong;
                var topLat = parameters.TopLat;
                var topLong = parameters.TopLong;

                DbHelper.CreateDbConnection();

                string command = @"
         SELECT Id, Location.STAsText() AS Location, Location.STGeometryType() AS Type
         FROM dbo.Hausumringe_mittelfranken
         WHERE Location.STIntersects(geography::STGeomFromText('POLYGON((
            {0} {1},
            {0} {3},
            {2} {3},
            {2} {1},
            {0} {1}
         ))', 4326)) = 1";

                string formattedQuery = string.Format(command, bottomLong, bottomLat, topLong, topLat);

                // Console.WriteLine(command);
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
                    // chop of last ,
                    response = response[..^1];
                }

                response += "]}";

                return Ok(response);
            }
            catch (ServiceException se)
            {
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

                // Console.WriteLine(command);
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
                    return Ok("{\"type\":\"FeatureCollection\",\"features\":[]}");
                }
                var clusters = QueryParameters.ClusterData(spatialDataList,Convert.ToInt32(parameters.ZoomLevel));
                var geoJson = QueryParameters.ConvertToGeoJson(clusters);
                return Ok(JsonConvert.SerializeObject(geoJson));
            }
            catch (ServiceException se)
            {
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
            [BindRequired] public float BottomLat { get; set; }
            [BindRequired] public float BottomLong { get; set; }
            [BindRequired] public float TopLat { get; set; }
            [BindRequired] public float TopLong { get; set; }
            
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
    }
