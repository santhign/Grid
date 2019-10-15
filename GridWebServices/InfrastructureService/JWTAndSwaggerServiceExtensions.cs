using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Serilog;

namespace InfrastructureService
{
    public static class JWTAndSwaggerServiceExtensions
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services, string serviceName, string version = "v1")
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
        public static IServiceCollection AddJWTAuthentication(this IServiceCollection services)
        {
            // configure jwt authentication
            var key = Encoding.ASCII.GetBytes("stratagile grid customer signin jwt hashing secret");
            services.AddAuthentication(x =>
            {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
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
        
        public static int GetCustomerIDfromToken(ClaimsPrincipal user)
        {
            int customerID = -1;
            try
            {
                var claimsIdentity = user.Identity as ClaimsIdentity;
                int.TryParse(claimsIdentity.FindFirst(ClaimTypes.Name)?.Value, out customerID);
                return customerID;
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Failed to get CustomerId from claim in {function}", "GetCustomerIDfromToken");
                throw ex;
            }
            
        }

    }
}
