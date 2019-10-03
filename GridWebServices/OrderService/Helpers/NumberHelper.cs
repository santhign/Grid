using Core.Enums;
using Core.Extensions;
using Core.Helpers;
using Core.Models;
using InfrastructureService;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OrderService.DataAccess;
using OrderService.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Helpers
{
    public class NumberHelper
    {
        IConfiguration _iconfiguration;

        public NumberHelper(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }
        public async Task<NumberDetails> GetNumberFromBSS(int CustomerID)
        {
            NumberDetails _details = new NumberDetails();
            OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);
            BSSAPIHelper bsshelper = new BSSAPIHelper();
            DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());
            GridBSSConfi config = bsshelper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);
            config.GridDefaultAssetLimit = 1;
            DatabaseResponse serviceCAF = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());
            DatabaseResponse requestIdRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.GetAssets.ToString(), CustomerID, (int)BSSCalls.NewSession, "");
            ResponseObject res = new ResponseObject();
            try
            {
                res = await bsshelper.GetAssetInventory(config, (((List<ServiceFees>)serviceCAF.Results)).FirstOrDefault().ServiceCode, (BSSAssetRequest)requestIdRes.Results);
                if (res != null && res.Response != null && res.Response.asset_details != null && (int.Parse(res.Response.asset_details.total_record_count) > 0))
                {
                    BSSNumbers numbers = new BSSNumbers();

                    numbers.FreeNumbers = bsshelper.GetFreeNumbers(res);
                    string number = numbers.FreeNumbers[0].MobileNumber;
                    string json = bsshelper.GetJsonString(numbers.FreeNumbers); // json insert

                    DatabaseResponse updateBssCallFreeNumbers = await _orderAccess.UpdateBSSCallNumbers(json, ((BSSAssetRequest)requestIdRes.Results).userid, ((BSSAssetRequest)requestIdRes.Results).BSSCallLogID);
                    DatabaseResponse requestIdToUpdateMainLineRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), CustomerID, (int)BSSCalls.ExistingSession, number);

                    BSSUpdateResponseObject bssUpdateResponse = new BSSUpdateResponseObject();
                    try
                    {
                        //line blocking
                        bssUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateMainLineRes.Results, number, false);
                        if (bsshelper.GetResponseCode(bssUpdateResponse) == "0")
                        {
                            _details.Number = number;
                            _details.UserSessionID = ((BSSAssetRequest)requestIdToUpdateMainLineRes.Results).userid;
                            return _details;
                        }
                        else
                        {
                            return _details;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));
                        return _details;
                    }
                }
                else
                {
                    return _details;
                }
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));
                return _details;
            }
        }

        public async Task<NumberDetails> BlockNumber(int CustomerID, string number)
        {
            NumberDetails _details = new NumberDetails();
            BSSAPIHelper bsshelper = new BSSAPIHelper();
            OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);
            DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());
            GridBSSConfi config = bsshelper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);
            DatabaseResponse requestIdToUpdateMainLineRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), CustomerID, (int)BSSCalls.ExistingSession, number);

            BSSUpdateResponseObject bssUpdateResponse = new BSSUpdateResponseObject();
            try
            {
                //line blocking
                bssUpdateResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateMainLineRes.Results, number, false);
                LogInfo.Information(JsonConvert.SerializeObject(bssUpdateResponse.Response));
                if (bsshelper.GetResponseCode(bssUpdateResponse) == "0")
                {
                    LogInfo.Information("Number assigned to order subscriber");
                    _details.Number = number;
                    _details.UserSessionID = ((BSSAssetRequest)requestIdToUpdateMainLineRes.Results).userid;
                    return _details;
                }
                else
                {
                    LogInfo.Information("Number assign failed" + bsshelper.GetResponseCode(bssUpdateResponse));
                    return _details;
                }
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));
                return _details;
            }
        }

        public async Task<bool> UnblockNumber(int CustomerID, string number, string source)
        {
            try
            {
                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);
                DatabaseResponse numberlog = await _orderAccess.LogUnblockNumber(CustomerID, number, source);
                string configValue = ConfigHelper.GetValueByKey("UnBlockingApp", _iconfiguration).Results.ToString();
                bool haveApp = false;
                bool.TryParse(configValue, out haveApp);
                if (!haveApp)
                {
                    //to be removed once number unblocking console app is implemented.
                    BSSAPIHelper bsshelper = new BSSAPIHelper();
                    DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());
                    GridBSSConfi config = bsshelper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);
                    DatabaseResponse requestIdToUpdateLineRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.UpdateAssetStatus.ToString(), CustomerID, (int)BSSCalls.ExistingSession, number);

                    //un reachable condition, but for safety
                    BSSUpdateResponseObject bssUpdateBuddyResponse = await bsshelper.UpdateAssetBlockNumber(config, (BSSAssetRequest)requestIdToUpdateLineRes.Results, number, true);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));
                return false;
            }
        }

        public async Task<bool> UnBlockMultipleNumbers(int CustomerID, List<NumberDetails> numbers, string source)
        {
            try
            {
                foreach (NumberDetails number in numbers)
                {
                    await UnblockNumber(CustomerID, number.Number, source);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));
                return false;
            }
        }

        public async Task<bool> IsValidNumberForExisitingReference(int CustomerID, string number)
        {
            DataAccessHelper _DataHelper = null;
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@CustomerID",  SqlDbType.Int ),
                    new SqlParameter( "@Number",  SqlDbType.NVarChar )

                };
                parameters[0].Value = CustomerID;
                parameters[1].Value = number;
                _DataHelper = new DataAccessHelper("Orders_ValidateNumberForExisitingReference", parameters, _iconfiguration);
                int result = await _DataHelper.RunAsync(); // 102 /105
                if (result == (int)DbReturnValue.RecordExists)
                {
                    return false;
                }
                else
                {
                    return true;
                }
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
