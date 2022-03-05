using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using RetryPatternUsingPolly.PollyUsingDependencyInjection;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace RetryPatternUsingPolly.CombiningPolicies
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class PollyWrappingGenericsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IPolicyHolder _policyHolder;

        public PollyWrappingGenericsController(IPolicyHolder policyHolder, HttpClient httpClient)
        {
            _policyHolder = policyHolder;
            _httpClient = httpClient;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            string requestEndpoint = $"inventory/{id}";

            HttpResponseMessage response = await _policyHolder.TimeoutRetryAndFallbackWrap.ExecuteAsync(
                async token => await _httpClient.GetAsync(requestEndpoint, token), CancellationToken.None);

            if (response.IsSuccessStatusCode)
            {
                int itemsInStock = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
                return Ok(itemsInStock);
            }

            if (response.Content != null)
            {
                return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
            }
            return StatusCode((int)response.StatusCode);
        }
    }
}
