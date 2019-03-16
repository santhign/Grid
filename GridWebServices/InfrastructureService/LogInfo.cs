using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureService
{
    public class LogInfo
    {
        public static void Initialize(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                   .ReadFrom.Configuration(configuration)
                   .CreateLogger();
        }

        public static void Information(string message)
        {
            Log.Information(message);
        }

        public static void Error(string message)
        {
            Log.Error(message);
        }

        public static void Fatal(Exception exception, string message)
        {
            Log.Fatal(exception, message);
        }

        public static void Warning(string message)
        {
            Log.Warning(message);
        }

        public static void Debug(string message)
        {
            Log.Debug(message);
        }

        public static void EntityInformation(string id, string source, string message)
        {
            string info = "{ \"id\": \"" + id + "\", \"source\": \"" + source + "\", \"message\": \"" + message + "\" }";
            Log.Information(info);
        }
    }
}
