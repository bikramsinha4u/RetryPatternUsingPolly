using Polly;
using Polly.Timeout;
using Polly.Wrap;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace RetryPatternUsingPolly.PollyUsingDependencyInjection
{
    public class PolicyHolder : IPolicyHolder
    {
        public IAsyncPolicy<HttpResponseMessage> HttpRetryPolicy { get; set; }
        public IAsyncPolicy HttpClientTimeoutExceptionPolicy { get; set; }

        
        // Combining Policies
        public IAsyncPolicy<HttpResponseMessage> HttpRequestFallbackPolicy { get; set; }
        public IAsyncPolicy<HttpResponseMessage> TimeoutPolicy { get; set; }
        public AsyncPolicyWrap<HttpResponseMessage> TimeoutRetryAndFallbackWrap { get; set; }
        readonly int _cachedResult = 0;

        public PolicyHolder()
        {
            HttpRetryPolicy =
               Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
               .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt), (response, timespan) =>
               {
                   var result = response.Result;
                   // log the result
               });

            HttpClientTimeoutExceptionPolicy = Policy.Handle<HttpRequestException>()
                .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(retryAttempt),
                    onRetry: (exception, timespan) =>
                    {
                        string message = exception.Message;
                        // log the message.
                    }
                );


            // Combining Policies
            HttpRequestFallbackPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .FallbackAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ObjectContent(_cachedResult.GetType(), _cachedResult, new JsonMediaTypeFormatter())
                });
            TimeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(1); // throws TimeoutRejectedException if timeout of 1 second is exceeded
            TimeoutRetryAndFallbackWrap = Policy.WrapAsync(HttpRequestFallbackPolicy, HttpRetryPolicy, TimeoutPolicy);
        }
    }
}
