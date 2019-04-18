
using Core.Enums;
using Core.Extensions;
using Core.Helpers;
using InfrastructureService;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
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
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _timeInterval = configuration.GetSection("TimeInterval").GetValue<int>("Default");
            Timer t = new Timer(TimerCallback, null, 0, _timeInterval);
            // Wait for the user to hit <Enter>
            Console.ReadLine();
        }

        /// <summary>
        /// Timers the callback.
        /// </summary>
        /// <param name="o">The o.</param>
        private static async void TimerCallback(Object o)
        {
            try
            {
                // Display the date/time when this method got called.
                Console.WriteLine("Start TimerCallback: " + DateTime.Now);
                PublishMessageToQueueDataAccess publishMessageTo = new PublishMessageToQueueDataAccess(_connectionString);
                await publishMessageTo.PushMessagesFromMessageQueueTable();
                Console.WriteLine("End TimerCallback: " + DateTime.Now);
                // Force a garbage collection to occur for this demo.
                GC.Collect();
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
            }
        }
    }
}
