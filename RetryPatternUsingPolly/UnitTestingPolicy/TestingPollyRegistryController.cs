using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RetryPatternUsingPolly.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class TestingPollyRegistryController : Controller
    {
        readonly IPolicyRegistry<string> _policyRegistry;
        private readonly HttpClient _httpClient;

        public TestingPollyRegistryController(IPolicyRegistry<string> policyRegistry, HttpClient httpClient)
        {
            _policyRegistry = policyRegistry;
            _httpClient = httpClient;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            string requestEndpoint = $"inventory/{id}";

            IAsyncPolicy<HttpResponseMessage> httpRetryPolicy =
                _policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("SimpleHttpRetryPolicy");

            IAsyncPolicy httpClientTimeoutExceptionPolicy =
                _policyRegistry.Get<IAsyncPolicy>("SimpleHttpTimeoutPolicy");

            HttpResponseMessage response =
                await httpRetryPolicy.ExecuteAsync(
                    () => httpClientTimeoutExceptionPolicy.ExecuteAsync(
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
