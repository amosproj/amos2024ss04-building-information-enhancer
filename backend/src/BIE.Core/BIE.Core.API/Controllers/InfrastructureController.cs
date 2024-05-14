using Microsoft.AspNetCore.Mvc;
using BIE.Core.BaseRepository;
using BIE.Core.DataObjects;
using BIE.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Parbat.Core.Services;

namespace BIE.Core.API.Controllers
{
    /// <summary>
    /// Batch controller
    /// </summary>
    [Route(Global.API_CONTROLLER)]
    [ApiController]
    public class InfrastructureController : Controller
    {
        private InfrastructureService _service;

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="service">IRepositoryFactory</param>
        public InfrastructureController(InfrastructureService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get a record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public ActionResult<Infrastructure> Get(long id)
        {
            try
            {
                //Infrastructure found = _service.FindByID(id);

                //return Ok(found);
                return Ok();

            }
            catch (ServiceException se)
            {
                return BadRequest(se.Message);
            }
        }

        /// <summary>
        /// List all batch
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult List()
        {
            try
            {
                //var Infrastructure = _service.GetAll();
                //return Ok(Infrastructure);
                return Ok();

            }
            catch (ServiceException se)
            {
                return BadRequest(se.Message);
            }
        }

        /// <summary>
        /// Update a batch
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        [HttpPut]
        public ActionResult Update([FromBody] Infrastructure b)
        {
            try
            {
                //_service.Update(b);
                return NoContent();
            }
            catch (ServiceException se)
            {
                return BadRequest(se.Message);
            }
        }

        /// <summary>
        /// Create 
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult<Infrastructure> Create([FromBody] Infrastructure b)
        {
            try
            {
                //_service.Create(b);
                //return Created("Get", b);
                return Ok();
            }
            catch (ServiceException se)
            {
                return BadRequest(se.Message);
            }

        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public ActionResult Delete(long id)
        {
            try
            {
                //_service.Delete(id);
                return NoContent();
            }
            catch (ServiceException se)
            {
                return BadRequest(se.Message);
            }
        }
    }
}
