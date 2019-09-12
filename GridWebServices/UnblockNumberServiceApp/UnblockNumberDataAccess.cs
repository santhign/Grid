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
    public class UnblockNumberDataAccess
    {
        private readonly string _connectionString;
        public UnblockNumberDataAccess(string connectionString)
        {
            _connectionString = connectionString;
        }
        public int GetIntervel()
        {
            DatabaseResponse intervelConfigResponse = new DatabaseResponse();

            intervelConfigResponse = ConfigHelper.GetValueByKey(ConfigKeys.UN_TimeInterval.ToString(), _connectionString);

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

        /// <summary>
        /// Unblocks the number.
        /// </summary>
        /// <param name="CustomerID">The customer identifier.</param>
        /// <param name="number">The number.</param>
        /// <returns></returns>
        public async Task<bool> UnblockNumber(int CustomerID, string number)
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

        /// <summary>
        /// Gets the customer number.
        /// </summary>
        /// <returns></returns>
        public async Task<List<NumberDetails>> GetCustomerNumber()
        {
            DataAccessHelper _DataHelper = null;
            List<NumberDetails> numberDetails = new List<NumberDetails>();
            try
            {
                _DataHelper = new DataAccessHelper("Orders_GetUnBlockNumbers", _connectionString);
                DataTable dt = new DataTable();
                int result = await _DataHelper.RunAsync(dt); // 102 /105

                if (dt.Rows.Count > 0)
                {
                    numberDetails = (from model in dt.AsEnumerable()
                                     select new NumberDetails()
                                     {
                                         MobileNumber = model.Field<string>("MobileNumber"),
                                         CustomerID = model.Field<int>("CustomerID")

                                     }).ToList();

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

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <param name="configType">Type of the configuration.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetConfiguration(string configType)
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

        /// <summary>
        /// Gets the BSS API request identifier.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="apiName">Name of the API.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="isNewSession">The is new session.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <returns></returns>
        public async Task<DatabaseResponse> GetBssApiRequestId(string source, string apiName, int customerId, int isNewSession, string mobileNumber)
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

        /// <summary>
        /// Updates the un block number details.
        /// </summary>
        /// <param name="CustomerID">The customer identifier.</param>
        /// <param name="number">The number.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="isProcessed">The is processed.</param>
        /// <returns></returns>
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
