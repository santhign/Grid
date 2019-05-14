using AdminService.DataAccess;
using System.IO.Compression;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using InfrastructureService;

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

        /// <summary>
        /// Setting configuaration on startup
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        ///This method gets called by the runtime. Use this method to add services to the container.
        /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        /// </summary>
        /// <param name="services"></param>
        /// 
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy("StratagileAdminPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
           
            services.AddMvc();
          
            //to access configuration from controller
            services.AddSingleton(Configuration);

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory("StratagileAdminPolicy"));
            });

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes =
                    ResponseCompressionDefaults.MimeTypes.Concat(
                        new[] { "image/svg+xml" });
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "GRID Admin API", Version = "v1" });
                var xmlDocPath = System.AppDomain.CurrentDomain.BaseDirectory + @"AdminService.xml";
                c.IncludeXmlComments(xmlDocPath);
            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "GRID Admin API V1");
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable Cors
            app.UseCors("StratagileAdminPolicy");
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
                await context.Response.WriteAsync("Admin Service!");
            });
          
        }       

    }
}
