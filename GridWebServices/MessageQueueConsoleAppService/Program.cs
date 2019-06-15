
using Core.Enums;
using Core.Extensions;
using Core.Helpers;
using Core.Models;
using InfrastructureService;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Runtime.Loader;
using System.Threading;


namespace MessageQueueConsoleAppService
{
    /// <summary>
    /// Program class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The connection string
        /// </summary>
        private static string _connectionString;
        /// <summary>
        /// The time interval
        /// </summary>
        private static int _timeInterval;

        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
             .Build();

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {

            LogInfo.Initialize(Configuration);

            _connectionString = Configuration.GetConnectionString("DefaultConnection");
            _timeInterval = GetIntervel();

            bool complete = false;
            var t = new Thread(() =>
            {
                bool toggle = false;
                while (!complete)
                {
                    toggle = !toggle;

                    TimerCallback();
                    Thread.Sleep(_timeInterval);
                }

            });
            t.Start();
            complete = false;
            t.Join();


        }

        /// <summary>
        /// Timers the callback.
        /// </summary>
        /// <param name="o">The o.</param>
        private static async void TimerCallback()
        {
            try
            {
                // Display the date/time when this method got called.
                Console.WriteLine("Start TimerCallback: " + DateTime.Now);
                LogInfo.Information("Start TimerCallback: " + DateTime.Now);
                PublishMessageToQueueDataAccess publishMessageTo = new PublishMessageToQueueDataAccess(_connectionString);
                await publishMessageTo.PushMessagesFromMessageQueueTable();
                Console.WriteLine("End TimerCallback: " + DateTime.Now);
                LogInfo.Information("End TimerCallback: " + DateTime.Now);
                // Force a garbage collection to occur for this demo.
                //GC.Collect();
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
            }
        }

        private static int GetIntervel()
        {
            DatabaseResponse intervelConfigResponse = new DatabaseResponse();

            intervelConfigResponse = ConfigHelper.GetValueByKey(ConfigKeys.MQConsoleInterval.ToString(), _connectionString);

            if (intervelConfigResponse != null && intervelConfigResponse.ResponseCode == (int)DbReturnValue.RecordExists)
            {
                string configValue = (string)intervelConfigResponse.Results;

                return int.Parse(configValue);
            }

            else
            {
                return 0;
            }
        }

    }
}
