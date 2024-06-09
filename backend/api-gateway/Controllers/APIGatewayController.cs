using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using APIGateway.Models;
using System.ComponentModel.DataAnnotations;

namespace BIE.Core.API.Controllers
{
    [Route("api/")]
    [ApiController]
    public class APIGatewayController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<APIGatewayController> _logger;

        public APIGatewayController(HttpClient httpClient, ILogger<APIGatewayController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Gets the dataset viewport data based on the provided parameters.
        /// </summary>
        /// <param name="datasetID">The ID of the dataset.</param>
        /// <param name="zoomLevel">The zoom level.</param>
        /// <param name="BottomLat">The bottom latitude of the viewport.</param>
        /// <param name="BottomLong">The bottom longitude of the viewport.</param>
        /// <param name="TopLat">The top latitude of the viewport.</param>
        /// <param name="TopLong">The top longitude of the viewport.</param>
        /// <returns>Data for the specified dataset in the provided viewport bounds and zoom level.</returns>
        [HttpGet("getDatasetViewportData")]
        [ProducesResponseType(typeof(GeoJsonResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetDatasetViewportData(
            [FromQuery, Required] int datasetID, 
            [FromQuery] int zoomLevel, 
            [FromQuery, Required] float BottomLat, 
            [FromQuery, Required] float BottomLong, 
            [FromQuery, Required] float TopLat, 
            [FromQuery, Required] float TopLong)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation($"Fetching data for DatasetID: {datasetID}, ZoomLevel: {zoomLevel}, Viewport: [{BottomLat}, {BottomLong}] to [{TopLat}, {TopLong}]");
            // Here the port 80 is used not 8080. This is due to the docker containers using the interal ports to communicate and not the external ones.
            var targetUrl = $"http://api-composer:80/api/v1.0/Dataset/1/data?ZoomLevel={zoomLevel}&BottomLat={BottomLat}&BottomLong={BottomLong}&TopLat={TopLat}&TopLong={TopLong}";

            try
            {
                var response = await _httpClient.GetAsync(targetUrl);
                response.EnsureSuccessStatusCode(); // Throw if not a success code.
                var content = await response.Content.ReadAsStringAsync();
                var geoJsonResponse = JsonSerializer.Deserialize<GeoJsonResponse>(content);
                
                return Ok(geoJsonResponse);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching data from the target service.");
                return StatusCode(500, "Error fetching data from the target service.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
