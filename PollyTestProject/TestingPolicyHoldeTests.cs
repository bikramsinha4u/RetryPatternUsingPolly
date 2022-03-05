using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using Polly;
using Polly.Registry;
using RetryPatternUsingPolly.Controllers;
using RetryPatternUsingPolly.PollyUsingDependencyInjection;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PollyTestProject
{
    public class TestingPolicyHoldeTests
    {
        [Fact]
        public async Task GetTest()
        {
            //Arrange 
            int fakeInventoryResponse = 15;
            Mock<HttpMessageHandler> httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(fakeInventoryResponse.ToString(), Encoding.UTF8, "application/json"),
                }));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object);
            httpClient.BaseAddress = new Uri(@"http://some.address.com/v1/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var mockPolicyHolder = new Mock<IPolicyHolder>();
            mockPolicyHolder.SetupAllProperties();
            mockPolicyHolder.Object.HttpClientTimeoutExceptionPolicy = Policy.NoOpAsync();
            mockPolicyHolder.Object.HttpRetryPolicy = Policy.NoOpAsync<HttpResponseMessage>();

            PollyUsingPolicyHolderForDIController controller = new PollyUsingPolicyHolderForDIController(mockPolicyHolder.Object, httpClient);

            //Act
            IActionResult result = await controller.Get(2);

            //Assert
            OkObjectResult resultObject = result as OkObjectResult;
            Assert.NotNull(resultObject);

            int number = (int)resultObject.Value;
            Assert.Equal(15, number);
        }
    }
}
