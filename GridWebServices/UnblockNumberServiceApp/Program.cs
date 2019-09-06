using Core.Enums;
using Core.Extensions;
using Core.Helpers;
using Core.Models;
using InfrastructureService;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace UnblockNumberServiceApp
{

    public class NumberDetails
    {
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
                var result = await GetCustomerNumber();
                if(result != null && result.CustomerID != 0 && string.IsNullOrEmpty(result.MobileNumber))
                    await UnblockNumber(result.CustomerID, result.MobileNumber);
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

        public async static Task<bool> UnblockNumber(int CustomerID, string number)
        {
            try
            {
                //to be removed once number unblocking console app is implemented.
                BSSAPIHelper bsshelper = new BSSAPIHelper();
                DatabaseResponse configResponse = await GetConfiguration(ConfiType.BSS.ToString());
                GridBSSConfi config = bsshelper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);
                DatabaseResponse requestIdToUpdateLineRes = await GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), CustomerID, (int)BSSCalls.ExistingSession, number);

                //un reachable condition, but for safety
                BSSUpdateResponseObject bssUpdateBuddyResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateLineRes.Results, number, true);
                return true;
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));
                return false;
            }
        }

        public async static Task<NumberDetails> GetCustomerNumber()
        {
            DataAccessHelper _DataHelper = null;
            NumberDetails numberDetails = new NumberDetails();
            try
            {
                _DataHelper = new DataAccessHelper("Orders_GetUnBlockNumbers",  _connectionString);
                DataTable dt = new DataTable();
                int result = await _DataHelper.RunAsync(dt); // 102 /105
                
                if(dt.Rows.Count > 0)
                {
                    numberDetails = (from model in dt.AsEnumerable()
                                        select new NumberDetails()
                                        {
                                            MobileNumber = model.Field<string>("MobileNumber"),
                                            CustomerID = model.Field<int>("CustomerID")                                          

                                        }).FirstOrDefault();

                }

                //DatabaseResponse response = new DatabaseResponse { ResponseCode = result,Results = number };

                return numberDetails;
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public static async Task<DatabaseResponse> GetConfiguration(string configType)
        {
            DataAccessHelper _DataHelper = null;
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@ConfigType",  SqlDbType.NVarChar )

                };

                parameters[0].Value = configType;

                _DataHelper = new DataAccessHelper("Admin_GetConfigurations", parameters, _connectionString);

                DataTable dt = new DataTable();

                int result = await _DataHelper.RunAsync(dt); // 102 /105

                DatabaseResponse response = new DatabaseResponse();

                if (result == 105)
                {

                    List<Dictionary<string, string>> configDictionary = new List<Dictionary<string, string>>();

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        configDictionary = LinqExtensions.GetDictionary(dt);
                    }

                    response = new DatabaseResponse { ResponseCode = result, Results = configDictionary };

                }

                else
                {
                    response = new DatabaseResponse { ResponseCode = result };
                }

                return response;
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async static Task<DatabaseResponse> GetBssApiRequestId(string source, string apiName, int customerId, int isNewSession, string mobileNumber)
        {
            DataAccessHelper _DataHelper = null;
            try
            {

                SqlParameter[] parameters =
               {
                    new SqlParameter( "@Source",  SqlDbType.NVarChar ),
                    new SqlParameter( "@APIName",  SqlDbType.NVarChar ),
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@IsNewSession",  SqlDbType.Int ),
                    new SqlParameter( "@MobileNumber",  SqlDbType.NVarChar),
                };

                parameters[0].Value = source;
                parameters[1].Value = apiName;
                parameters[2].Value = customerId;
                parameters[3].Value = isNewSession;
                parameters[4].Value = mobileNumber;

                _DataHelper = new DataAccessHelper("Admin_GetRequestIDForBSSAPI", parameters, _connectionString);

                DataTable dt = new DataTable();

                await _DataHelper.RunAsync(dt);

                BSSAssetRequest assetRequest = new BSSAssetRequest();

                DatabaseResponse response = new DatabaseResponse();

                if (dt.Rows.Count > 0)
                {
                    assetRequest = (from model in dt.AsEnumerable()
                                    select new BSSAssetRequest()
                                    {
                                        request_id = model.Field<string>("RequestID"),
                                        userid = model.Field<string>("UserID"),
                                        BSSCallLogID = model.Field<int>("BSSCallLogID"),

                                    }).FirstOrDefault();
                }

                response = new DatabaseResponse { Results = assetRequest };

                return response;
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }

        public async Task<DatabaseResponse> UpdateUnBlockNumberDetails(int CustomerID, string number, string remarks, int isProcessed)
        {
            DataAccessHelper _DataHelper = null;
            try
            {
                SqlParameter[] parameters =
               {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@Number",  SqlDbType.NVarChar ),
                    new SqlParameter( "@Remarks",  SqlDbType.NVarChar ),
                    new SqlParameter( "@IsProcessed",  SqlDbType.Int )
                };

                parameters[0].Value = CustomerID;
                parameters[1].Value = number;
                parameters[2].Value = remarks;
                parameters[3].Value = isProcessed;
                
                _DataHelper = new DataAccessHelper("Orders_UpdateUnBlockNumberDetails", parameters, _connectionString);

                int result = await _DataHelper.RunAsync(); // 102 /105

                DatabaseResponse response = new DatabaseResponse { ResponseCode = result };

                return response;
            }

            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
        }
    }
}
