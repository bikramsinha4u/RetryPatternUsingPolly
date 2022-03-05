using Polly;
using Polly.Registry;
using System;
using System.Net.Http;

namespace RetryPatternUsingPolly.UnitTestingPolicy
{
    public static class PollyPolicyForDI
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            IAsyncPolicy<HttpResponseMessage> httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .RetryAsync(3);

            return httpRetryPolicy;
        }

        public static IPolicyRegistry<string> GetRegistry()
        {
            IPolicyRegistry<string> registry = new PolicyRegistry();

            IAsyncPolicy<HttpResponseMessage> httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .RetryAsync(3);

            registry.Add("SimpleHttpRetryPolicy", httpRetryPolicy);

            IAsyncPolicy httpClientTimeoutExceptionPolicy = Policy.Handle<HttpRequestException>()
                .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(retryAttempt));

            registry.Add("SimpleHttpTimeoutPolicy", httpClientTimeoutExceptionPolicy);

            return registry;
        }
    }
}
