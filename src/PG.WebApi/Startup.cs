using Marten;
using Marten.Schema.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PG.Adapters.Adapters;
using PG.Adapters.Repositories;
using PG.Core.Abstractions.AcquiringBank;
using PG.Core.Abstractions.Repositories;
using PG.Core.Services;

namespace PG.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CKO Payment Gateway", Version = "v1"});
            });

            services.AddMarten(o =>
            {
                o.Connection(Configuration.GetConnectionString("Marten"));
                o.DefaultIdStrategy = (mapping, storeOptions) => new CombGuidIdGeneration();
            });

            services.AddTransient<IPaymentRepository, MartenPaymentRepository>();
            services.AddTransient<IBankClient, FakeBankClient>();
            services.AddTransient<IPaymentGatewayService, PaymentGatewayService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CKO Payment Gateway v1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
