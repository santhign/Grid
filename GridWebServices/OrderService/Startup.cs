﻿//using OrderService.DataAccess;
using System.IO.Compression;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using InfrastructureService;
using Microsoft.AspNetCore.Http.Internal;
using Core.Helpers;

namespace OrderService
{
    /// <summary>
    /// Application startup/entry point
    /// </summary>
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
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
                c.SwaggerDoc("v1", new Info { Title = "GRID Order API", Version = "v1" });
                var xmlDocPath = System.AppDomain.CurrentDomain.BaseDirectory + @"OrderService.xml";
                c.IncludeXmlComments(xmlDocPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //if (!env.IsProduction())
            //{
                app.UseSwagger();

                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
                // specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GRID Order API V1");
                });
           // }

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
                await context.Response.WriteAsync("Order Service!");
            });
        }

        

    }
}
