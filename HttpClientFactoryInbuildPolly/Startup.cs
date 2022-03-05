using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Registry;
using System;
using System.Net.Http;

namespace HttpClientFactoryInbuildPolly
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            IPolicyRegistry<string> registry = services.AddPolicyRegistry();

            IAsyncPolicy<HttpResponseMessage> httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .RetryAsync(3);

            registry.Add("SimpleHttpRetryPolicy", httpRetryPolicy);

            IAsyncPolicy<HttpResponseMessage> httWaitAndpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt));

            registry.Add("SimpleWaitAndRetryPolicy", httWaitAndpRetryPolicy);

            IAsyncPolicy<HttpResponseMessage> noOpPolicy = Policy.NoOpAsync()
                .AsAsyncPolicy<HttpResponseMessage>();

            registry.Add("NoOpPolicy", noOpPolicy);

            // Polly http client factory from registry
            services.AddHttpClient("RemoteServer", client =>
            {
                client.BaseAddress = new Uri("http://localhost:51482/api/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }).AddPolicyHandlerFromRegistry(PolicySelector);

            // Polly http client factory simple
            //services.AddHttpClient("RemoteServer", client =>
            //{
            //    client.BaseAddress = new Uri("http://localhost:51482/api/");
            //    client.DefaultRequestHeaders.Add("Accept", "application/json");
            //}).AddPolicyHandler(httpRetryPolicy);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private IAsyncPolicy<HttpResponseMessage> PolicySelector(IReadOnlyPolicyRegistry<string> policyRegistry,
            HttpRequestMessage httpRequestMessage)
        {
            if (httpRequestMessage.RequestUri.LocalPath.StartsWith("find"))
            {
                return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("SimpleHttpRetryPolicy");
            }
            else if (httpRequestMessage.RequestUri.LocalPath.StartsWith("create"))
            {
                return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("NoOpPolicy");
            }
            else
            {
                return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("SimpleWaitAndRetryPolicy");
            }
        }
    }
}
