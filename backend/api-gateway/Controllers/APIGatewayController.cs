using Microsoft.AspNetCore.Mvc;
using System.Linq;
using APIGateway.Models;
using System.Text.Json;
using BIE.Core.API.Services;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BIE.Core.API.Controllers
{
    [Route("api/")]
    [ApiController]
    public class APIGatewayController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<APIGatewayController> _logger;
        private readonly MongoDBService _mongoDBService;

        public APIGatewayController(HttpClient httpClient,
            ILogger<APIGatewayController> logger,
            MongoDBService mongoDBService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _mongoDBService = mongoDBService;
        }

        /// <summary>
        /// Gets the list of available datasets with id, name and description.
        /// </summary>
        /// <returns>A list of available datasets</returns>
        [HttpGet("getDatasetList")]
        [ProducesResponseType(typeof(List<DatasetBasicData>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetDataSetList()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var collection = _mongoDBService.GetDatasetsCollection();
            var datasets = await collection.Find(_ => true).ToListAsync();
            var datasetBasicDataList = datasets.Select(d => d.BasicData).ToList();

            return Ok(datasetBasicDataList);
        }

        /// <summary>
        /// Gets the metadata for the given dataset. Contains things like Icon, visualization types
        /// </summary>
        /// <param name="datasetID">The ID of the dataset</param>
        /// <returns>Metadata for the specified dataset</returns>
        [HttpGet("getDatasetMetadata")]
        [ProducesResponseType(typeof(DatasetMetadata), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetDatasetMetadata([FromQuery, Required] string datasetID)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var collection = _mongoDBService.GetDatasetsCollection();
            var dataset = await collection.Find(d => d.BasicData.DatasetId == datasetID).FirstOrDefaultAsync();

            if (dataset == null)
            {
                return NotFound($"Dataset with ID {datasetID} not found.");
            }

            return Ok(dataset.MetaData);
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

            _logger.LogInformation($"Fetching data for DatasetID: {request.DatasetId}");

            // Generate mock data for the current dataset
            var currentDatasetData = GenerateMockData("charging_stations", 3);

            // Generate mock data for general data from other datasets
            var generalData = GenerateMockData("house_footprints", 3);

            var extraRows = GenerateMockData("extra_dataset", 1);

            var response = new LocationDataResponse
            {
                CurrentDatasetData = currentDatasetData,
                GeneralData = generalData,
                ExtraRows = extraRows
            };

            return Ok(response);
        }

        // Helper method to generate mock data
        private List<DatasetItem> GenerateMockData(string mapId, int count)
        {
            var random = new Random();
            var data = new List<DatasetItem>();

            for (int i = 0; i < count; i++)
            {
                var item = new DatasetItem
                {
                    Id = Guid.NewGuid().ToString(), // Generate a unique ID
                    Key = $"{mapId} Item {i + 1} (distance)",
                    Value = $"{random.Next(50, 1000)} m",
                    MapId = mapId
                };
                data.Add(item);
            }

            return data;
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
            [FromQuery, Required] string datasetID,
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

            var metadata = _mongoDBService.GetDatasetMetadata(datasetID);

            if (metadata == null)
            {
                return StatusCode(400, $"Unsupported dataset ID of {datasetID}");
            }
            
            if (datasetID == "house_footprints")
            {
                return Ok(MockData.MockHouseFootprints);
                //targetUrl = $"http://api-composer:80/api/v1.0/Dataset/2/data?ZoomLevel={zoomLevel}&BottomLat={BottomLat}&BottomLong={BottomLong}&TopLat={TopLat}&TopLong={TopLong}";
            }

            string targetUrl;
            if (datasetID == "charging_stations")
            {
                targetUrl =
                    $"http://api-composer:80/api/v1.0/Dataset/1/data?ZoomLevel={zoomLevel}&BottomLat={BottomLat}&BottomLong={BottomLong}&TopLat={TopLat}&TopLong={TopLong}";
            }
            else

            {
                _logger.LogError($"Unsupported dataset ID of {datasetID}");
            }

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