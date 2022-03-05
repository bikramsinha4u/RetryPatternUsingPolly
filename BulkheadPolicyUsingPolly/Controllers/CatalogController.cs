using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly.Bulkhead;

namespace BulkheadPolicyUsingPolly.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class CatalogController : Controller
    {
        private static int _requestCount = 0;
        private readonly HttpClient _httpClient;
        private readonly AsyncBulkheadPolicy<HttpResponseMessage> _bulkheadIsolationPolicy;

        public CatalogController(HttpClient httpClient, AsyncBulkheadPolicy<HttpResponseMessage> bulkheadIsolationPolicy)
        {
            _httpClient = httpClient;
            _bulkheadIsolationPolicy = bulkheadIsolationPolicy;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            _requestCount++;
            LogBulkheadInfo();
            string requestEndpoint = $"inventory/{id}";

            HttpResponseMessage response = await _bulkheadIsolationPolicy.ExecuteAsync(
                     () => _httpClient.GetAsync(requestEndpoint));

            if (response.IsSuccessStatusCode)
            {
                int itemsInStock = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
                return Ok(itemsInStock);
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
        }

        private void LogBulkheadInfo()
        {
            System.Diagnostics.Debug.WriteLine($"PollyDemo RequestCount {_requestCount}");
            System.Diagnostics.Debug.WriteLine($"PollyDemo BulkheadAvailableCount " +
                                               $"{_bulkheadIsolationPolicy.BulkheadAvailableCount}");
            System.Diagnostics.Debug.WriteLine($"PollyDemo QueueAvailableCount " +
                                               $"{_bulkheadIsolationPolicy.QueueAvailableCount}");
        }
    }
}
