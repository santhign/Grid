
using Core.Enums;
using Core.Extensions;
using Core.Helpers;
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
            _timeInterval = Configuration.GetSection("TimeInterval").GetValue<int>("Default");            
            // Wait for the user to hit <Enter>
            if (_timeInterval > 0)
            {
                Timer t = new Timer(TimerCallback, null, 0, _timeInterval);

                // Wait for the user to press enter key to start timer
                //Console.ReadLine();
                
            }

            var _quitEvent = new ManualResetEvent(false);
            AssemblyLoadContext.Default.Unloading += ctx =>
            {
                System.Console.WriteLine("Unloding fired");
                LogInfo.Information("Unloding fired");
                _quitEvent.Set();
            };
            System.Console.WriteLine("Waiting for signals");
            LogInfo.Information("Waiting for signals");
            _quitEvent.WaitOne();
            System.Console.WriteLine("Received signal gracefully shutting down");
            LogInfo.Information("Received signal gracefully shutting down");


            //Console.WriteLine("Hello World!");
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
                LogInfo.Information("Start TimerCallback: " + DateTime.Now);
                PublishMessageToQueueDataAccess publishMessageTo = new PublishMessageToQueueDataAccess(_connectionString);
                await publishMessageTo.PushMessagesFromMessageQueueTable();
                Console.WriteLine("End TimerCallback: " + DateTime.Now);
                LogInfo.Information("End TimerCallback: " + DateTime.Now);
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
