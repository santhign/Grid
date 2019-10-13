using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using InfrastructureService;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace CatelogService
{
    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddEnvironmentVariables()
              //.AddJsonFile($"appsettings.{new WebHostBuilder().GetSetting("environment").ToString().Trim()}.json", optional: true, reloadOnChange: true)
              .Build();

        public static void Main(string[] args)
        {
            //Log.Logger = new LoggerConfiguration()
            //        .ReadFrom.Configuration(Configuration)            
            //        .CreateLogger();

            LogInfo.Initialize(Configuration);
            Log.Information("Catelog Service is running");
            try
            {
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls(Configuration["hostUrl"])
                .UseStartup<Startup>()
                .UseSerilog();
    }
}
