using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace RetryPatternUsingPolly.UnitTestingPolicy
{
    public static class HttpClientForDI
    {
        public static HttpClient GetHttpClient()
        {
            HttpClient httpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:57696/api/") // this is the endpoint HttpClient will hit,
            };

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return httpClient;
        }
    }
}
