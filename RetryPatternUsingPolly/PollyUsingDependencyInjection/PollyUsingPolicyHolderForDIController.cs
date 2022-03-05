using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RetryPatternUsingPolly.PollyUsingDependencyInjection;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RetryPatternUsingPolly.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class PollyUsingPolicyHolderForDIController : Controller
    {
        readonly HttpClient _httpClient;
        private readonly IPolicyHolder _policyHolder;

        public PollyUsingPolicyHolderForDIController(IPolicyHolder policyHolder, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _policyHolder = policyHolder;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            string requestEndpoint = $"inventory/{id}";

            HttpResponseMessage response = await _policyHolder.HttpRetryPolicy.ExecuteAsync(
                () => _policyHolder.HttpClientTimeoutExceptionPolicy.ExecuteAsync(
                async token => await _httpClient.GetAsync(requestEndpoint, token), CancellationToken.None));

            if (response.IsSuccessStatusCode)
            {
                int itemsInStock = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
                return Ok(itemsInStock);
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
        }
    }
}
