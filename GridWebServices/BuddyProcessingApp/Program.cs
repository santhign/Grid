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
            try
            {
                LogInfo.Initialize(Configuration);

                LogInfo.Information("Buddy Console App is Started");

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
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
            }
        }

        private static async void TimerCallback()
        {
            try
            {
                Console.WriteLine("Start timer action: " + DateTime.UtcNow);

                LogInfo.Information("Start timer action: " + DateTime.UtcNow);

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
              
                Console.WriteLine("End timer action: " + DateTime.UtcNow);

                LogInfo.Information("End timer action: " + DateTime.UtcNow);                
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
            }
        }

        private  static int GetIntervel()
        {
            try
            {

                DatabaseResponse intervelConfigResponse = new DatabaseResponse();

                intervelConfigResponse = ConfigHelper.GetValueByKey(ConfigKeys.BuddyTrialIntervel.ToString(), _connectionString);

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
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return 0;
            }
        }
    }
}
