using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CircuitBreakerUsingPolly
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

            // Basic circuit breaker policy
            AsyncCircuitBreakerPolicy<HttpResponseMessage> breakerPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(2, TimeSpan.FromSeconds(60), OnBreak, OnReset, OnHalfOpen);

            // Advanced circuit breaker policy
            //AsyncCircuitBreakerPolicy<HttpResponseMessage> breakerPolicy = Policy
            //    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            //    .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(60), 7, TimeSpan.FromSeconds(60), OnBreak, OnReset, OnHalfOpen);

            HttpClient httpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:31258/api/") // this is the endpoint HttpClient will hit,
            };
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            services.AddSingleton<HttpClient>(httpClient);
            services.AddSingleton<AsyncCircuitBreakerPolicy<HttpResponseMessage>>(breakerPolicy);
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

        private void OnHalfOpen()
        {
            Debug.WriteLine("Connection half open");
        }

        private void OnReset(Context context)
        {
            Debug.WriteLine("Connection reset");
        }

        private void OnBreak(DelegateResult<HttpResponseMessage> delegateResult, TimeSpan timeSpan, Context context)
        {
            Debug.WriteLine($"Connection break: {delegateResult.Result}, {delegateResult.Result}");
        }
    }
}
