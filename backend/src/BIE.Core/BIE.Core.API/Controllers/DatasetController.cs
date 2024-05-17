using Microsoft.AspNetCore.Mvc;
using BIE.Core.BaseRepository;
using BIE.Core.DataObjects;
using BIE.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public ActionResult Get(int bottomLat, int bottomLong, int topLat, int topLong, int zoomLevel)
        {
            try
            {
                return Ok();

            }
            catch (ServiceException se)
            {
                return BadRequest(se.Message);
            }
        }

    }
}
