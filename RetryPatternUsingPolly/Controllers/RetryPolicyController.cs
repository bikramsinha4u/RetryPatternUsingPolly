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
    public class RetryPolicyController : Controller
    {
        readonly HttpClient _httpClient;

        readonly AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy;
        private int myRetryCount = 3;

        public RetryPolicyController()
        {
            _httpClient = GetHttpClient();

            _httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .RetryAsync(myRetryCount, onRetry: (response, timespan) =>
                    { 
                        if(response.Result.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            PerformReauthorization();
                        }
                        else if (response.Result.StatusCode == HttpStatusCode.NotFound)
                        {
                            // log message
                        }
                        else if (response.Result.StatusCode == HttpStatusCode.Conflict)
                        {
                            // log message
                        }
                    });
        }

        private void PerformReauthorization()
        {
            throw new NotImplementedException();
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
            httpClient.BaseAddress = new Uri(@"http://localhost:57696/api/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }
    }
}
