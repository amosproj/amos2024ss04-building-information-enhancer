using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using APIGateway.Models;
using System.ComponentModel.DataAnnotations;
using System.Data;

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
        /// Gets the list of available datasets with id, name and description
        /// </summary>
        /// <returns>Data for the specified dataset in the provided viewport bounds and zoom level.</returns>
        [HttpGet("getDatasetList")]
        [ProducesResponseType(typeof(DatasetListResponse), 200)]
        [ProducesResponseType(400)]
        public IActionResult GetDataSetList()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var datasetInfoList = new DatasetListResponse
            {
                basicInfoList = [
                new() {
                    datasetId = "empty_map",
                    name = "Empty Map",
                    description = "An empty, default map of Germany, with no data loaded."
                },
                new()
                {
                    datasetId = "charging_stations",
                    name = "Charging stations",
                    description = "Locations of all charging stations in Germany."
                },
                new()
                {
                    datasetId = "house_footprints",
                    name = "House Footprints",
                    description = "Footprints for the houses."
                }]
            };

            return Ok(datasetInfoList);
        }

        /// <summary>
        /// Gets the metadata for the given dataset. Contains things like icon, visualization types
        /// </summary>
        /// <returns>Data for the specified dataset in the provided viewport bounds and zoom level.</returns>
        [HttpGet("getDatasetMetadata")]
        [ProducesResponseType(typeof(DatasetListResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]

        public IActionResult GetDatasetMetadata([FromQuery, Required] int datasetID)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(new DatasetMetadata { icon = "ChargingStation", type="marker" });
        }

        /// <summary>
        /// Loads the location data for the given point or poylgon.
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

            _logger.LogInformation($"Fetching data for DatasetID: {request.DatasetId}");
            // Here the port 80 is used not 8080. This is due to the docker containers using the interal ports to communicate and not the external ones.
            // cant do anything at the moment
            // Mock data for current dataset
            var currentDatasetData = new List<DatasetItem>
            {
                new DatasetItem { key = "Charging Station 1 (distance)", value = "234 m", mapId = "charging_stations" },
                new DatasetItem { key = "Charging Station 2 (distance)", value = "542 m", mapId = "charging_stations" },
                new DatasetItem { key = "Charging Station 3 (distance)", value = "789 m", mapId = "charging_stations" }
            };

            // Mock data for general data from other datasets
            var generalData = new List<DatasetItem>
            {
                new DatasetItem { key = "House 1 (distance)", value = "50 m", mapId = "house_footprints" },
                new DatasetItem { key = "House 2 (distance)", value = "100 m", mapId = "house_footprints" },
                new DatasetItem { key = "Empty Area (distance)", value = "300 m", mapId = "empty_map" }
            };

            var response = new LocationDataResponse
            {
                CurrentDatasetData = currentDatasetData,
                GeneralData = generalData
            };

            return Ok(response);
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
            [FromQuery, Required] double BottomLat,
            [FromQuery, Required] double BottomLong,
            [FromQuery, Required] double TopLat,
            [FromQuery, Required] double TopLong)
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
