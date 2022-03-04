using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace RetryPatternUsingPolly.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class TimeoutWithHttpClinetController : Controller
    {
        readonly HttpClient _httpClient;

        readonly AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy;
        private int myRetryCount = 1;

        public TimeoutWithHttpClinetController()
        {
            _httpClient = GetHttpClient();

            _httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                    .RetryAsync(myRetryCount, onRetry: OnRetry);
        }

        public void OnRetry(DelegateResult<HttpResponseMessage> delegateResult, int retryCount)
        {
            if (delegateResult.Exception is HttpRequestException)
            {
                if (delegateResult.Exception.GetBaseException().Message == "the operation timed out")
                {
                    // log message
                }
            }
            else if (delegateResult.Result.StatusCode == HttpStatusCode.GatewayTimeout)
            {
                // log message
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            string requestEndpoint = $"inventory/{id}";

            HttpResponseMessage response = await _httpRetryPolicy.ExecuteAsync(() => _httpClient.GetAsync(requestEndpoint));

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
            httpClient.BaseAddress = new Uri(@"http://10.255.255.1/someUnreachableEndpoint/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }
    }
}
