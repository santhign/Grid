using Core.Enums;
using Core.Extensions;
using Core.Helpers;
using InfrastructureService;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Core.Models;
using System.Collections.Generic;
using System.Linq;
using Core.DataAccess;


namespace BuddyProcessingApp
{
    class Program
    {
        private static string _connectionString;

        private static int _timeInterval;
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
             .Build();
        static void Main(string[] args)
        {          

            LogInfo.Initialize(Configuration);      

            LogInfo.Information("Buddy Console App is Started");

            _connectionString = Configuration.GetConnectionString("DefaultConnection");

            _timeInterval =  GetIntervel();

            if (_timeInterval > 0)
            {
                Timer t = new Timer(TimerCallback, null, 0, _timeInterval);

                // Wait for the user to press enter key to start timer
                Console.ReadLine();
            }
        }

        private static async void TimerCallback(Object o)
        {
            try
            {
                Console.WriteLine("Start timer: " + DateTime.UtcNow);

                LogInfo.Information("BuddyConsole Start timer: " + DateTime.UtcNow);

                BuddyDataAccess buddyDataAccess = new BuddyDataAccess();

                BuddyHelper buddyHelper = new BuddyHelper(_connectionString);

                DatabaseResponse pendingBuddyResponse = await buddyDataAccess.GetPendingBuddyList(_connectionString);

                if (pendingBuddyResponse != null && pendingBuddyResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                {
                    List<PendingBuddy> pendingBuddyList = new List<PendingBuddy>();

                    pendingBuddyList = (List<PendingBuddy>)pendingBuddyResponse.Results;

                    List<int> orderList = (from buddy in pendingBuddyList select (buddy.OrderID)).ToList();

                    if (orderList != null && orderList.Count > 0)
                    {
                        foreach (int orderID in orderList)
                        {
                            List<PendingBuddy> buddyListToProcess = new List<PendingBuddy>();

                            buddyListToProcess = pendingBuddyList.Where(buddy => buddy.OrderID == orderID).ToList();

                            foreach (PendingBuddy buddy in buddyListToProcess)
                            {  
                                DatabaseResponse customerIDResponse = new DatabaseResponse();

                                int customerID;

                                customerIDResponse = await buddyDataAccess.GetCustomerIdFromOrderId(buddy.OrderID, _connectionString);

                                if (customerIDResponse != null && customerIDResponse.Results != null)
                                {
                                    customerID = (int)customerIDResponse.Results;

                                    BuddyCheckList buddyCheck = new BuddyCheckList { CustomerID = customerID, OrderID = buddy.OrderID, OrderSubscriberID = buddy.OrderSubscriberID, IsProcessed = buddy.IsProcessed, MobileNumber = buddy.MobileNumber };

                                    BuddyCheckList afterProcessing = await buddyHelper.ProcessBuddy(buddyCheck);

                                    buddy.IsProcessed = afterProcessing.IsProcessed;
                                }

                             }

                            List<PendingBuddy> unProcessedBuddies = buddyListToProcess.Where(b => b.IsProcessed == false).ToList();
                                         
                            if (unProcessedBuddies != null && unProcessedBuddies.Count > 0)
                            {
                                List<PendingBuddy> processedBuddies = buddyListToProcess.Where(b => b.IsProcessed == true).ToList();

                                foreach (PendingBuddy upBuddy in processedBuddies)
                                {
                                    DatabaseResponse upBuddyCreateResponse = await buddyDataAccess.UpdatePendingBuddyList(_connectionString,upBuddy);
                                }                                
                            }
                            else
                            {
                                List<PendingBuddy> processedBuddies = buddyListToProcess.Where(b => b.IsProcessed == true).ToList();

                                foreach (PendingBuddy upBuddy in processedBuddies)
                                {
                                    DatabaseResponse upBuddyCreateResponse = await buddyDataAccess.UpdatePendingBuddyList(_connectionString, upBuddy);
                                }

                                DatabaseResponse removeProcessedResponse = await buddyDataAccess.RemoveProcessedBuddyList(_connectionString, buddyListToProcess[0].OrderID);
                               
                                int processed = await buddyHelper.ProcessOrderQueueMessage(buddyListToProcess[0].OrderID);

                            }
                        }
                    }

                }
              
                Console.WriteLine("End Timer: " + DateTime.UtcNow);

                LogInfo.Information("BuddyConsole End timer: " + DateTime.UtcNow);

                GC.Collect();
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
            }
        }

        private  static int GetIntervel()
        {
            DatabaseResponse intervelConfigResponse = new DatabaseResponse();

            intervelConfigResponse =  ConfigHelper.GetValueByKey(ConfigKeys.BuddyTrialIntervel.ToString(), _connectionString);

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
