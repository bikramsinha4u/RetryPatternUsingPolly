using Polly;
using Polly.Registry;
using System;
using System.Net.Http;

namespace RetryPatternUsingPolly.PollyUsingPolicyRegistry
{
    public static class PolicyRegistryForDI
    {
        public static PolicyRegistry GetRegistry()
        {
            PolicyRegistry registry = new PolicyRegistry();

            IAsyncPolicy<HttpResponseMessage> httpRetryPolicy =
               Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
               .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt));

            registry.Add("SimpleHttpWaitAndRetry", httpRetryPolicy);

            IAsyncPolicy httpClientTimeoutException = Policy.Handle<HttpRequestException>()
               .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(retryAttempt));

            registry.Add("HttpClientTimeout", httpClientTimeoutException);

            return registry;
        }
    }
}
