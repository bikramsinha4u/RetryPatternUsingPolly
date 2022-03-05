using Polly;
using Polly.Wrap;
using System.Net.Http;

namespace RetryPatternUsingPolly.PollyUsingDependencyInjection
{
    public interface IPolicyHolder
    {
        IAsyncPolicy<HttpResponseMessage> HttpRetryPolicy { get; set; }
        IAsyncPolicy HttpClientTimeoutExceptionPolicy  { get; set; }

        
        // Combining Policies
        IAsyncPolicy<HttpResponseMessage> HttpRequestFallbackPolicy { get; set; }
        IAsyncPolicy<HttpResponseMessage> TimeoutPolicy { get; set; }
        AsyncPolicyWrap<HttpResponseMessage> TimeoutRetryAndFallbackWrap { get; set; }
    }
}
