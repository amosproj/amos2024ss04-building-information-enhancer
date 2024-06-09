using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
    public async Task<IActionResult> GetDatasetViewportData(
        [FromQuery] int datasetID, 
        [FromQuery] int zoomLevel, 
        [FromQuery] int BottomLat, 
        [FromQuery] int BottomLong, 
        [FromQuery] int TopLat, 
        [FromQuery] int TopLong)
    {
        _logger.LogInformation($"Fetching data for DatasetID: {datasetID}, ZoomLevel: {zoomLevel}, Viewport: [{BottomLat}, {BottomLong}] to [{TopLat}, {TopLong}]");
        
        // Assuming the target service URL and endpoint
        var targetUrl = $"https://target-service-url/dataset/{datasetID}/viewport?zoomLevel={zoomLevel}&BottomLat={BottomLat}&BottomLong={BottomLong}&TopLat={TopLat}&TopLong={TopLong}";

        var response = await _httpClient.GetAsync(targetUrl);
        var content = await response.Content.ReadAsStringAsync();
        
        return Content(content, response.Content.Headers.ContentType?.MediaType);
    }
}
