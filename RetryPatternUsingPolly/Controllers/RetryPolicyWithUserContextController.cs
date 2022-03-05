using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace RetryPatternUsingPolly.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class RetryPolicyWithUserContextController : Controller
    {
        readonly HttpClient _httpClient;

        readonly AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy;
        private int myRetryCount = 3;

        public RetryPolicyWithUserContextController()
        {
            _httpClient = GetHttpClient();

            _httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .RetryAsync(myRetryCount, onRetry: (response, timespan, context) =>
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

                        if (context.ContainsKey("Host"))
                        {
                            // log message conext["Host"]
                        }
                        else if (context.ContainsKey("User-Agent"))
                        {
                            // log message conext["User-Agent"]
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

            var host = Request.Headers.FirstOrDefault(h => h.Key == "Host").Value;
            var userAgent = Request.Headers.FirstOrDefault(h => h.Key == "UserAgent").Value;

            IDictionary<string, object> contextDictionary = new Dictionary<string, object>
            {
                { "Host", host },
                { "User-Agent", userAgent }
            };

            Context context = new Context("CatalogContext", contextDictionary);

            HttpResponseMessage response = await _httpRetryPolicy.ExecuteAsync(
                (context) => _httpClient.GetAsync(requestEndpoint), context);

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
