using Core.Helpers;
using InfrastructureService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdminService
{
    /// <summary>
    /// Application startup/entry point
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Application configuration
        /// </summary>
        public IConfiguration Configuration { get; }
        public string ServiceName { get; }

        /// <summary>
        /// Setting configuaration on startup
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ServiceName = "Admin";
        }

        /// <summary>
        ///This method gets called by the runtime. Use this method to add services to the container.
        /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        /// </summary>
        /// <param name="services"></param>
        /// 
        public void ConfigureServices(IServiceCollection services)
        {
            string[] corsArray = ConfigHelper.GetValueByKey("CORSWhitelist", Configuration).Results.ToString().Split(';');
            services.AddCors(o => o.AddPolicy("StratagileAdminPolicy", builder =>
            {
                builder.WithOrigins(corsArray)
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
           
            services.AddMvc();
          
            //to access configuration from controller
            services.AddSingleton(Configuration);
            services.AddScoped<DataAccess.Interfaces.IAdminOrderDataAccess, DataAccess.AdminOrderDataAccess>();
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory("StratagileAdminPolicy"));
            });           

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerDocumentation(ServiceName, "v1");            
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
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
            app.UseCors("StratagileAdminPolicy");
            app.Use(next => context => {
                context.Request.EnableRewind();
                return next(context);
            });
            app.UseMiddleware<LogMiddleware>();

            app.UseMvc(routes =>
            {
                //Custom Route mapping
                routes.MapRoute(
                   name: "BannerDetails",
                   template: "bannerdetails",
                   defaults: new { controller = "Banner", action = "BannerDetails" }
                );
                
            });
            
            app.Run(async (context) =>
            {
                await context.Response.WriteAsync(ServiceName +" Service!");
            });
          
        }       

    }
}
