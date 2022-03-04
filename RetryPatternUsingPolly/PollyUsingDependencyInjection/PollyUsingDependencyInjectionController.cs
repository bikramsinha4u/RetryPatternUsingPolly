using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RetryPatternUsingPolly.PollyUsingDependencyInjection;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace RetryPatternUsingPolly.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class PollyUsingDependencyInjectionController : Controller
    {
        readonly HttpClient _httpClient;
        private readonly PolicyHolder _policyHolder;

        public PollyUsingDependencyInjectionController(PolicyHolder policyHolder)
        {
            _httpClient = GetHttpClient();
            _policyHolder = policyHolder;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            string requestEndpoint = $"inventory/{id}";

            HttpResponseMessage response = await _policyHolder.HttpRetryPolicy.ExecuteAsync(() => _httpClient.GetAsync(requestEndpoint));

            if (response.IsSuccessStatusCode)
            {
                int itemsInStock = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
                return Ok(itemsInStock);
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
        }

        private HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(@"http://localhost:57696/api/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }
    }
}
