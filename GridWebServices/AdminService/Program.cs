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
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder();
            var env = host.GetSetting("environment");
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true);
            var configuration = builder.Build();
            LogInfo.Initialize(configuration);
            LogInfo.Information("Admin Service is running");

            host.UseKestrel()
                .UseUrls(configuration["hostUrl"])
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build()
                .Run();
        }
    }
}
