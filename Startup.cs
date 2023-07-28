using System.Globalization;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Stripe;
using StripeApp.Data;
using StripeApp.Data.Repositories;
using StripeApp.Service;
using StripeApp.Stripe;

namespace StripeApp
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
            // services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            // {
            //     builder.AllowAnyOrigin()
            //             .AllowAnyMethod()
            //             .AllowAnyHeader();
            // }));
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new IsoDateTimeConverter()
                    {
                        DateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ",
                        DateTimeStyles = DateTimeStyles.AdjustToUniversal 
                    });
                options.SerializerSettings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
            });

            var connectionString = Configuration.GetConnectionString("StripeDBConnection");
            services.AddDbContext<StripeContext>(options =>
                            options.UseSqlServer(connectionString));

            services.AddAutoMapper(typeof(Startup));
            
            services.AddHttpContextAccessor();
            
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IStripeProcessor, StripeProcessor>();

            var stripeParameter = Configuration.GetSection("Stripe").Get<StripeParameters>();
            services.AddSingleton(stripeParameter);
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

            // app.UseCors("MyPolicy");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
