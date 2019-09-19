using Core.Enums;
using Core.Helpers;
using InfrastructureService;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;

namespace UnblockNumberServiceApp
{
    /// <summary>
    /// NumberDetails class
    /// </summary>
    public class NumberDetails
    {
        public int ID { get; set; }
        public int CustomerID { get; set; }
        public string MobileNumber { get; set; }
    }
    class Program
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
            UnblockNumberDataAccess unblockNumberDataAccess = new UnblockNumberDataAccess(_connectionString);
            _timeInterval = unblockNumberDataAccess.GetIntervel();

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
                UnblockNumberDataAccess unblockNumberDataAccess = new UnblockNumberDataAccess(_connectionString);
                // Display the date/time when this method got called.
                LogInfo.Information("Start TimerCallback: " + DateTime.Now);
                var resultList = await unblockNumberDataAccess.GetCustomerNumber();
                foreach (var result in resultList)
                {
                    if (result != null && result.CustomerID != 0 && !string.IsNullOrEmpty(result.MobileNumber))
                    {
                        try
                        {
                            var response = await unblockNumberDataAccess.UnblockNumber(result.CustomerID, result.MobileNumber);
                            if (response)
                            {
                                await unblockNumberDataAccess.UpdateUnBlockNumberDetails(result.CustomerID, result.MobileNumber, null, 1, result.ID);
                            }
                            else
                            {
                                await unblockNumberDataAccess.UpdateUnBlockNumberDetails(result.CustomerID, result.MobileNumber, null, 0, result.ID);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                            await unblockNumberDataAccess.UpdateUnBlockNumberDetails(result.CustomerID, result.MobileNumber, null, 0, result.ID);
                        }
                    }
                }
                LogInfo.Information("End TimerCallback: " + DateTime.Now);
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
            }
        }        
    }
}
