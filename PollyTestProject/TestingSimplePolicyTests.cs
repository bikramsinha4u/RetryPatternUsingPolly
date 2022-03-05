using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using Polly;
using RetryPatternUsingPolly.Controllers;
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
    public class TestingSimplePolicyTests
    {
        [Fact]
        public async Task RetryPolicyTest()
        {
            //Arrange 
            int fakeInventoryResponse = 15;
            var httpMessageHandler = new Mock<HttpMessageHandler>();
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

            IAsyncPolicy<HttpResponseMessage> mockPolicy = Policy.NoOpAsync<HttpResponseMessage>();

            TestingSimplePolicyController controller = new TestingSimplePolicyController(mockPolicy, httpClient);

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
