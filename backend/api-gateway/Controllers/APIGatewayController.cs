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
                    description = "An empty, default map of Germany, with no data loaded.",
                    icon = "",
                },
                new()
                {
                    datasetId = "charging_stations",
                    name = "Charging stations",
                    description = "Locations of all charging stations in Germany.",
                    icon = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"32\" height=\"32\" fill=\"#000000\" viewBox=\"0 0 256 256\"><path d=\"M134.62,123.51a8,8,0,0,1,.81,7.46l-16,40A8,8,0,0,1,104.57,165l11.61-29H96a8,8,0,0,1-7.43-11l16-40A8,8,0,1,1,119.43,91l-11.61,29H128A8,8,0,0,1,134.62,123.51ZM248,86.63V168a24,24,0,0,1-48,0V128a8,8,0,0,0-8-8H176v88h16a8,8,0,0,1,0,16H32a8,8,0,0,1,0-16H48V56A24,24,0,0,1,72,32h80a24,24,0,0,1,24,24v48h16a24,24,0,0,1,24,24v40a8,8,0,0,0,16,0V86.63A8,8,0,0,0,229.66,81L210.34,61.66a8,8,0,0,1,11.32-11.32L241,69.66A23.85,23.85,0,0,1,248,86.63ZM160,208V56a8,8,0,0,0-8-8H72a8,8,0,0,0-8,8V208Z\"></path></svg>",
                },
                new()
                {
                    datasetId = "house_footprints",
                    name = "House Footprints",
                    description = "Footprints for the houses.",
                    icon = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"32\" height=\"32\" fill=\"#000000\" viewBox=\"0 0 256 256\"><path d=\"M232,56H72V40a8,8,0,0,0-8-8H48A32,32,0,0,0,16,64V176a32,32,0,0,0,32,32H232a8,8,0,0,0,8-8V64A8,8,0,0,0,232,56ZM32,64A16,16,0,0,1,48,48h8v96H48a31.82,31.82,0,0,0-16,4.29ZM224,192H48a16,16,0,0,1,0-32H64a8,8,0,0,0,8-8V72H224ZM104,136a8,8,0,0,0,0,16h16v8a8,8,0,0,0,16,0v-8h24v8a8,8,0,0,0,16,0v-8h16a8,8,0,0,0,0-16H176V120h16a8,8,0,0,0,0-16H176V96a8,8,0,0,0-16,0v8H136V96a8,8,0,0,0-16,0v8H104a8,8,0,0,0,0,16h16v16Zm32-16h24v16H136Z\"></path></svg>"
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
            return Ok(new DatasetMetadata { icon = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 256 256\"><rect width=\"256\" height=\"256\" fill=\"none\"/><path d=\"M216,112v16c0,53-88,88-88,112,0-24-88-59-88-112V112\" fill=\"none\" stroke=\"currentColor\" stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"24\"/><path d=\"M80,56h96a48,48,0,0,1,48,48v0a8,8,0,0,1-8,8H40a8,8,0,0,1-8-8v0A48,48,0,0,1,80,56Z\" fill=\"none\" stroke=\"currentColor\" stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"24\"/><path d=\"M128,56V48a32,32,0,0,1,32-32\" fill=\"none\" stroke=\"currentColor\" stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"24\"/></svg>", type="marker" });
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
            
            string targetUrl;
            if (datasetID == "charging_stations")
            {
                targetUrl = $"http://api-composer:80/api/v1.0/Dataset/1/data?ZoomLevel={zoomLevel}&BottomLat={BottomLat}&BottomLong={BottomLong}&TopLat={TopLat}&TopLong={TopLong}";
            }
            else if (datasetID == "house_footprints")
            {
                return Ok(MockData.MockHouseFootprints);
                //targetUrl = $"http://api-composer:80/api/v1.0/Dataset/2/data?ZoomLevel={zoomLevel}&BottomLat={BottomLat}&BottomLong={BottomLong}&TopLat={TopLat}&TopLong={TopLong}";
            }
            else
            {
                _logger.LogError($"Unsupported dataset ID of {datasetID}");
                return StatusCode(400, $"Unsupported dataset ID of {datasetID}");
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
