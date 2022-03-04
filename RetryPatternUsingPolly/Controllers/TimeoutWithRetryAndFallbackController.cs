using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace RetryPatternUsingPolly.Controllers
{
    /// <summary>
    /// Polly Fallback -> Polly Retry -> Polly Timeout -> HttpClient Request
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class TimeoutWithRetryAndFallbackController : Controller
    {
        readonly AsyncFallbackPolicy<HttpResponseMessage> _httpRequestFallbackPolicy;
        private int myCachedNumber = 0;

        readonly AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy;
        private int myRetryCount = 2;

        readonly AsyncTimeoutPolicy _timeoutPolicy;

        readonly HttpClient _httpClient;

        public TimeoutWithRetryAndFallbackController()
        {
            _httpClient = GetHttpClient();

            _httpRequestFallbackPolicy = Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.InternalServerError)
                .Or<TimeoutRejectedException>()
                .FallbackAsync(
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ObjectContent(myCachedNumber.GetType(), myCachedNumber, new JsonMediaTypeFormatter())
                    });

            _httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                    .RetryAsync(myRetryCount);

            _timeoutPolicy = Policy.TimeoutAsync(1); // throws TimeoutRejectException if timeout of 1 second is exceeded
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            string requestEndpoint = $"inventory/TimeoutWithRetryAndFallbackPolicy/{id}";

            HttpResponseMessage response = 
                await 
                _httpRequestFallbackPolicy.ExecuteAsync(() =>
                    _httpRetryPolicy.ExecuteAsync(() => 
                        _timeoutPolicy.ExecuteAsync(
                            async token => await _httpClient.GetAsync(requestEndpoint, token), CancellationToken.None)));

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
