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
        public ActionResult GetHausumringedata([FromQuery] QueryParameters parameters)
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
         FROM dbo.SpatialData
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
        }
    }
}