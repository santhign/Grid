﻿using System;
using System.Collections.Generic;
using System.IO;
using Serilog;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using InfrastructureService;

namespace AdminService
{
    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddEnvironmentVariables()
              .Build();
        public static void Main(string[] args)
        {
            LogInfo.Initialize(Configuration);
            LogInfo.Information("Admin Service is running");
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls(Configuration["hostUrl"])
                .UseStartup<Startup>();
    }
}
