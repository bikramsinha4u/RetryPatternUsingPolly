using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Registry;
using RetryPatternUsingPolly.PollyUsingDependencyInjection;
using RetryPatternUsingPolly.PollyUsingPolicyRegistry;
using RetryPatternUsingPolly.UnitTestingPolicy;
using System.Net.Http;

namespace RetryPatternUsingPolly
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
            
            services.AddSingleton<PolicyHolder>(new PolicyHolder()); // PollyUsingDependencyInjection
            services.AddSingleton<PolicyRegistry>(PolicyRegistryForDI.GetRegistry()); // PollyUsingPolicyRegistry
            
            // DI for Unit Testing Policy
            services.AddSingleton<HttpClient>(HttpClientForDI.GetHttpClient());
            services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(PollyPolicyForDI.GetRetryPolicy());
            services.AddSingleton<IPolicyRegistry<string>>(PollyPolicyForDI.GetRegistry());
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
    }
}
