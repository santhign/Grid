//using OrderService.DataAccess;
using Core.Helpers;
using InfrastructureService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OrderService
{
    /// <summary>
    /// Application startup/entry point
    /// </summary>
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public string ServiceName { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ServiceName = "Order";
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            string[] corsArray = ConfigHelper.GetValueByKey("CORSWhitelist", Configuration).Results.ToString().Split(';');
            services.AddCors(o => o.AddPolicy("GridOrderPolicy", builder =>
            {
                builder.WithOrigins(corsArray)
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            services.AddMvc();
            //to access configuration from controller
            services.AddSingleton(Configuration);
            services.AddScoped<OrderService.DataAccess.IChangeRequestDataAccess, OrderService.DataAccess.ChangeRequestDataAccess>();
            services.AddScoped<OrderService.DataAccess.IMessageQueueDataAccess, OrderService.DataAccess.MessageQueueDataAccess>();
            

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory("GridOrderPolicy"));
            });

            services.AddSwaggerDocumentation(ServiceName, "v1");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (!env.IsProduction())
            {
                app.UseSwaggerDocumentation(ServiceName, "v1");
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // Enable Cors
            app.UseCors("GridOrderPolicy");

            app.Use(next => context => {
                context.Request.EnableRewind();

                return next(context);
            });
            app.UseMiddleware<LogMiddleware>();
            app.UseMvc();
            app.Run(async (context) =>
            {
                await context.Response.WriteAsync(ServiceName + " Service!");
            });
        }

        

    }
}
