using Microsoft.AspNetCore.Mvc;
using BIE.Core.BaseRepository;
using BIE.Core.DataObjects;
using BIE.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BIE.Core.DBRepository;

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
        [HttpGet("1/data/{bottomLat}/{bottomLong}/{topLat}/{topLong}/{zoomLevel}")]
        public ActionResult Get(float bottomLat, float bottomLong, float topLat, float topLong, int zoomLevel)
        {
            try
            {
                DbHelper.CreateDbConnection();

                var command =
                    "SELECT * " + 
                    "FROM dbo.EV_charging_stations" + 
                    $" WHERE latitude > {bottomLat} AND latitude < {topLat} AND"+
                    $" longitude > {bottomLong} AND longitude < {topLong};";

                command = command.Replace(",", ".");
                // Console.WriteLine(command);
                var response = "";
                foreach (var row in DbHelper.GetData(command))
                {
                    response += row["operator"];
                }

                return Ok(response);
            }
            catch (ServiceException se)
            {
                return BadRequest(se.Message);
            }
        }
    }
}