using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using OrderService.DataAccess;
using Core.Models;
using Core.Enums;
using Core.Extensions;
using InfrastructureService;
using Core.Helpers;

namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BSSAssetsController : ControllerBase
    {
        IConfiguration _iconfiguration;

        public BSSAssetsController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }


        [HttpGet]
        public async Task<IActionResult> GetAssets()
        {
            try
            {
                BSSAPIHelper helper = new BSSAPIHelper();

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());               

                GridBSSConfi config = helper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                DatabaseResponse serviceCAF= await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

                DatabaseResponse requestIdRes= await _orderAccess.GetBssApiRequestId(GridMicroservices.Customer.ToString(), BSSApis.GetAssets.ToString(),30); // need to pass customer_id here
                 
                ResponseObject res= await  helper.GetAssetInventory(config, ((List<ServiceFees>) serviceCAF.Results).FirstOrDefault().ServiceCode, ((BSSAssetRequest)requestIdRes.Results).request_id);

                return Ok(new OperationResponse
                {
                    HasSucceeded = true,                   
                    IsDomainValidationErrors = false,
                    ReturnedObject= res
                });

            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });
            }

        }

        /// <summary>
        /// This will return a set of Mobile Numbers from BSS API
        /// </summary>
        /// <param name="token">AuthToken</param>
        /// <returns>OperationResponse
        /// Body{
        ///  HasSucceeded = true,
        ///  IsDomainValidationErrors = false,
        ///  ReturnedObject = {numbers} 
        /// }
        /// </returns>

        // GET: api/Orders/token

        [HttpGet("{token}")]
        public async Task<IActionResult> GetNumbers([FromRoute] string  token)
        {
            try
            {
                BSSAPIHelper helper = new BSSAPIHelper();

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _orderAccess.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    if (!(aTokenResp.CreatedOn < DateTime.UtcNow.AddDays(-7)))
                    {
                        DatabaseResponse systemConfigResponse = await _orderAccess.GetConfiguration(ConfiType.System.ToString());

                        DatabaseResponse bssConfigResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                        GridBSSConfi bssConfig = helper.GetGridConfig((List<Dictionary<string, string>>)bssConfigResponse.Results);

                        GridSystemConfig systemConfig = helper.GetGridSystemConfig((List<Dictionary<string, string>>)systemConfigResponse.Results);

                        DatabaseResponse serviceCAF = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

                        DatabaseResponse requestIdResForFreeNumber = await _orderAccess.GetBssApiRequestId(GridMicroservices.Customer.ToString(), BSSApis.GetAssets.ToString(), aTokenResp.CustomerID);

                        //Getting FreeNumbers
                        ResponseObject res = await helper.GetAssetInventory(bssConfig, ((List<ServiceFees>)serviceCAF.Results).FirstOrDefault().ServiceCode, ((BSSAssetRequest)requestIdResForFreeNumber.Results).request_id, systemConfig.FreeNumberListCount);

                        BSSNumbers numbers = new BSSNumbers();

                        if (res != null)
                        {
                            numbers.FreeNumbers = helper.GetFreeNumbers(res);

                            // get Premium Numbers

                            DatabaseResponse serviceCAFPremium = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Premium.ToString());

                            if (serviceCAFPremium != null && serviceCAFPremium.ResponseCode == (int)DbReturnValue.RecordExists)
                            {
                                List<ServiceFees> premiumServiceFeeList = new List<ServiceFees>();

                                premiumServiceFeeList = (List<ServiceFees>)serviceCAFPremium.Results;

                                int countPerPremium = (systemConfig.PremiumNumberListCount / premiumServiceFeeList.Count);

                                int countBalance = systemConfig.PremiumNumberListCount % premiumServiceFeeList.Count;

                                if (countBalance > 0)
                                {
                                    countPerPremium = countPerPremium + countBalance;
                                }

                                int loopCount = premiumServiceFeeList.Count;

                                int iterator = 0;

                                foreach (ServiceFees fee in premiumServiceFeeList)
                                {
                                    //get code and call premum 
                                    //  fee.PortalServiceName

                                    DatabaseResponse requestIdResForPremium = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.GetAssets.ToString(), aTokenResp.CustomerID);

                                    ResponseObject premumResponse = await helper.GetAssetInventory(bssConfig, fee.ServiceCode, ((BSSAssetRequest)requestIdResForPremium.Results).request_id, countPerPremium);

                                    if (premumResponse != null && premumResponse.Response.asset_details != null)
                                    {
                                        List<PremiumNumbers> premiumNumbers = helper.GetPremiumNumbers(premumResponse, fee);

                                        foreach (PremiumNumbers premium in premiumNumbers)
                                        {
                                            numbers.PremiumNumbers.Add(premium);
                                        }

                                    }
                                    else
                                    {
                                        //failed to get premium

                                        if (iterator == 0)
                                        {
                                            countPerPremium = (systemConfig.PremiumNumberListCount / (premiumServiceFeeList.Count - 1));
                                        }
                                        else if (iterator == 1)
                                        {
                                            if (numbers.PremiumNumbers.Count < countPerPremium * (iterator + 1))

                                                countPerPremium = (systemConfig.PremiumNumberListCount / (premiumServiceFeeList.Count - 2));
                                            else
                                                countPerPremium = (systemConfig.PremiumNumberListCount / (premiumServiceFeeList.Count - 1));
                                        }

                                    }

                                    iterator++;

                                }  // for

                                if (numbers.PremiumNumbers.Count > systemConfig.PremiumNumberListCount)
                                {
                                    int extrPremiumCount = numbers.PremiumNumbers.Count - systemConfig.PremiumNumberListCount;

                                    numbers.PremiumNumbers.RemoveRange(numbers.PremiumNumbers.Count - (extrPremiumCount + 1), extrPremiumCount);
                                }
                            }

                        }
                        else
                        {
                            //failed to get free numbers
                        }

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = true,
                            IsDomainValidationErrors = false,
                            ReturnedObject = numbers

                        });
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = EnumExtensions.GetDescription(DbReturnValue.TokenExpired),
                            IsDomainValidationErrors = true
                        });

                    }
                    
                }

                else
                {
                    // token auth failure
                    LogInfo.Error(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = false
                    });
                }



            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });
            }

        }

        /// <summary>
        /// This will return a set of free or premium Mobile Numbers from BSS API based on the given type input
        /// </summary>
        /// <param name="token">token</param>
        /// <param name="type">type=1-free/2-premium</param>
        /// <returns>OperationResponse
        /// Body{
        ///  HasSucceeded = true,
        ///  IsDomainValidationErrors = false,
        ///  ReturnedObject = {numbers} 
        /// }
        /// </returns>
        [HttpGet("{token}/{type}")]
        public async Task<IActionResult> GetMoreNumbers([FromRoute] string token, int type)
        {
            try
            {

                BSSAPIHelper helper = new BSSAPIHelper();

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _orderAccess.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                    if (!(aTokenResp.CreatedOn < DateTime.UtcNow.AddDays(-7)))
                    {
                       
                        DatabaseResponse systemConfigResponse = await _orderAccess.GetConfiguration(ConfiType.System.ToString());

                        DatabaseResponse bssConfigResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                        GridBSSConfi bssConfig = helper.GetGridConfig((List<Dictionary<string, string>>)bssConfigResponse.Results);

                        GridSystemConfig systemConfig = helper.GetGridSystemConfig((List<Dictionary<string, string>>)systemConfigResponse.Results);

                        DatabaseResponse serviceCAF = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

                        DatabaseResponse requestIdResForFreeNumber = await _orderAccess.GetBssApiRequestId(GridMicroservices.Customer.ToString(), BSSApis.GetAssets.ToString(), aTokenResp.CustomerID);

                        BSSNumbers numbers = new BSSNumbers();

                        if (type == 1) // free numbers
                        {
                            ResponseObject res = await helper.GetAssetInventory(bssConfig, ((List<ServiceFees>)serviceCAF.Results).FirstOrDefault().ServiceCode, ((BSSAssetRequest)requestIdResForFreeNumber.Results).request_id, systemConfig.FreeNumberListCount);

                            if (res != null)
                            {
                                numbers.FreeNumbers = helper.GetFreeNumbers(res); 

                            }
                            else
                            {
                                //failed to get free numbers
                            }
                        }

                        else
                        {
                            // Get Premium Numbers
                            DatabaseResponse serviceCAFPremium = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Premium.ToString());

                            if (serviceCAFPremium != null && serviceCAFPremium.ResponseCode == (int)DbReturnValue.RecordExists)
                            {
                                List<ServiceFees> premiumServiceFeeList = new List<ServiceFees>();

                                premiumServiceFeeList = (List<ServiceFees>)serviceCAFPremium.Results;

                                int countPerPremium = (systemConfig.PremiumNumberListCount / premiumServiceFeeList.Count);

                                int countBalance = systemConfig.PremiumNumberListCount % premiumServiceFeeList.Count;

                                if (countBalance > 0)
                                {
                                    countPerPremium = countPerPremium + countBalance;
                                }

                                int loopCount = premiumServiceFeeList.Count;

                                int iterator = 0;

                                foreach (ServiceFees fee in premiumServiceFeeList)
                                {
                                    //get code and call premum 
                                    //  fee.PortalServiceName

                                    DatabaseResponse requestIdResForPremium = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.GetAssets.ToString(), aTokenResp.CustomerID);

                                    ResponseObject premumResponse = await helper.GetAssetInventory(bssConfig, fee.ServiceCode, ((BSSAssetRequest)requestIdResForPremium.Results).request_id, countPerPremium);

                                    if (premumResponse != null && premumResponse.Response.asset_details != null)
                                    {
                                        List<PremiumNumbers> premiumNumbers = helper.GetPremiumNumbers(premumResponse, fee);

                                        foreach (PremiumNumbers premium in premiumNumbers)
                                        {
                                            numbers.PremiumNumbers.Add(premium);
                                        }

                                    }
                                    else
                                    {
                                        //failed to get premium

                                        if (iterator == 0)
                                        {
                                            countPerPremium = (systemConfig.PremiumNumberListCount / (premiumServiceFeeList.Count - 1));
                                        }
                                        else if (iterator == 1)
                                        {
                                            if (numbers.PremiumNumbers.Count < countPerPremium * (iterator + 1))

                                                countPerPremium = (systemConfig.PremiumNumberListCount / (premiumServiceFeeList.Count - 2));
                                            else
                                                countPerPremium = (systemConfig.PremiumNumberListCount / (premiumServiceFeeList.Count - 1));
                                        }

                                    }

                                    iterator++;

                                }  // for

                                if (numbers.PremiumNumbers.Count > systemConfig.PremiumNumberListCount)
                                {
                                    int extrPremiumCount = numbers.PremiumNumbers.Count - systemConfig.PremiumNumberListCount;

                                    numbers.PremiumNumbers.RemoveRange(numbers.PremiumNumbers.Count - (extrPremiumCount + 1), extrPremiumCount);
                                }
                            }
                        }

                        if(type==1)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = true,
                                IsDomainValidationErrors = false,
                                ReturnedObject = numbers.FreeNumbers

                            });
                        }

                        else
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = true,
                                IsDomainValidationErrors = false,
                                ReturnedObject = numbers.PremiumNumbers

                            });
                        }
                        

                    }

                    else
                    {
                        //Token expired

                        LogInfo.Error(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = false,
                            Message = EnumExtensions.GetDescription(DbReturnValue.TokenExpired),
                            IsDomainValidationErrors = true
                        });

                    }

                }

                else
                {
                    // token auth failure
                    LogInfo.Error(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = false
                    });
                }



            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });
            }

        }
    }
}