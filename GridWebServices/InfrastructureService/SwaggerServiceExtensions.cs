using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfrastructureService
{
    public static class SwaggerServiceExtensions
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services, string serviceName, string version="v1")
        {
          
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                string title = String.Format("GRID {0} Service API", serviceName);
                c.SwaggerDoc("v1", new Info { Title = title, Version = version });
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    In = "header",
                    Description = "Please enter into field the word 'Bearer' following by space and JWT Token",
                    Name = "Authorization",
                    Type = "apiKey"
                });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>> {
                    { "Bearer", Enumerable.Empty<string>() }
                });

                string xmldocName = string.Format(@"{0}Service.xml", serviceName);
                var xmlDocPath = System.AppDomain.CurrentDomain.BaseDirectory + xmldocName;
                c.IncludeXmlComments(xmlDocPath);
            });            

            return services;
        }

        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app, string serviceName, string version = "v1")
        {
            string title = String.Format("GRID {0} Service API", serviceName);
            //if (!env.IsProduction())
            //{
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(String.Format("/swagger/{0}/swagger.json",version), title);
                c.RoutePrefix = string.Empty;
            });
            //}            

            return app;
        }
    }
}
