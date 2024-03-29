﻿using System.Linq;
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
using OrderService.Enums;
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

        /// <summary>
        /// This will return BSS Assets by default limit in configuration
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [Route("GetAssets")]
        [HttpPost]
        public async Task<IActionResult> GetAssets([FromHeader(Name = "Grid-Authorization-Token")] string token)
        {
            try
            {
                if(string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)
                    
                });

                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                           .SelectMany(x => x.Errors)
                                                           .Select(x => x.ErrorMessage))
                            });
                        }
                        BSSAPIHelper bsshelper = new BSSAPIHelper();

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                        GridBSSConfi config = bsshelper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                        DatabaseResponse serviceCAF = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

                        DatabaseResponse requestIdRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Customer.ToString(), BSSApis.GetAssets.ToString(), customerID, (int)BSSCalls.NewSession, "");

                        ResponseObject res = new ResponseObject();

                        try
                        {
                            res=await bsshelper.GetAssetInventory(config, ((List<ServiceFees>)serviceCAF.Results).FirstOrDefault().ServiceCode, (BSSAssetRequest)requestIdRes.Results);
                        }

                        catch (Exception ex)
                        {
                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));
                             
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed),
                                IsDomainValidationErrors = false
                            });

                        }
                        
                        BSSNumbers receivedNumbers = new BSSNumbers();

                        receivedNumbers.FreeNumbers = bsshelper.GetFreeNumbers(res);

                        string json = bsshelper.GetJsonString(receivedNumbers.FreeNumbers); // json insert

                        DatabaseResponse updateBssCallFeeNumbers = await _orderAccess.UpdateBSSCallNumbers(json, ((BSSAssetRequest)requestIdRes.Results).userid, ((BSSAssetRequest)requestIdRes.Results).BSSCallLogID);

                        return Ok(new OperationResponse
                        {
                            HasSucceeded = true,
                            IsDomainValidationErrors = false,
                            ReturnedObject = res
                        });
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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

        [HttpGet]

        public async Task<IActionResult> GetNumbers([FromHeader(Name = "Grid-Authorization-Token")] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                           .SelectMany(x => x.Errors)
                                                           .Select(x => x.ErrorMessage))
                            });
                        }
                        BSSAPIHelper bsshelper = new BSSAPIHelper();

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        DatabaseResponse systemConfigResponse = await _orderAccess.GetConfiguration(ConfiType.System.ToString());

                        DatabaseResponse bssConfigResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                        GridBSSConfi bssConfig = bsshelper.GetGridConfig((List<Dictionary<string, string>>)bssConfigResponse.Results);

                        GridSystemConfig systemConfig = bsshelper.GetGridSystemConfig((List<Dictionary<string, string>>)systemConfigResponse.Results);

                        DatabaseResponse serviceCAF = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

                        DatabaseResponse requestIdResForFreeNumber = await _orderAccess.GetBssApiRequestId(GridMicroservices.Customer.ToString(), BSSApis.GetAssets.ToString(), customerID, (int)BSSCalls.NewSession, "");

                        ResponseObject res = new ResponseObject();

                        //Getting FreeNumbers
                        try
                        {
                            res= await bsshelper.GetAssetInventory(bssConfig, ((List<ServiceFees>)serviceCAF.Results).FirstOrDefault().ServiceCode, (BSSAssetRequest)requestIdResForFreeNumber.Results, systemConfig.FreeNumberListCount);
                        }

                        catch (Exception ex)
                        {
                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));

                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed),
                                IsDomainValidationErrors = false
                            });

                        }                       

                        BSSNumbers numbers = new BSSNumbers();

                        if (res != null)
                        {
                            numbers.FreeNumbers = bsshelper.GetFreeNumbers(res);

                            //insert these number into database
                            string json = bsshelper.GetJsonString(numbers.FreeNumbers); // json insert

                            DatabaseResponse updateBssCallFeeNumbers = await _orderAccess.UpdateBSSCallNumbers(json, ((BSSAssetRequest)requestIdResForFreeNumber.Results).userid, ((BSSAssetRequest)requestIdResForFreeNumber.Results).BSSCallLogID);

                            if (updateBssCallFeeNumbers.ResponseCode == (int)DbReturnValue.CreateSuccess)
                            {
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

                                        DatabaseResponse requestIdResForPremium = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.GetAssets.ToString(), customerID, (int)BSSCalls.NewSession, "");

                                        ResponseObject premumResponse = new ResponseObject();

                                        try
                                        {
                                            premumResponse= await bsshelper.GetAssetInventory(bssConfig, fee.ServiceCode, (BSSAssetRequest)requestIdResForPremium.Results, countPerPremium);
                                        }

                                        catch (Exception ex)
                                        {
                                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));

                                        }                                        

                                        if (premumResponse != null && premumResponse.Response!=null && premumResponse.Response.asset_details != null)
                                        {
                                            List<PremiumNumbers> premiumNumbers = bsshelper.GetPremiumNumbers(premumResponse, fee);

                                            List<FreeNumber> premiumToLogNumbers = bsshelper.GetFreeNumbers(premumResponse);

                                            string jsonPremium = bsshelper.GetJsonString(premiumToLogNumbers);

                                            DatabaseResponse updateBssCallPremiumNumbers = await _orderAccess.UpdateBSSCallNumbers(jsonPremium, ((BSSAssetRequest)requestIdResForPremium.Results).userid, ((BSSAssetRequest)requestIdResForPremium.Results).BSSCallLogID);

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
                                //failded to update BSS call numbers so returning
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

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// <param name="token"></param>
        /// <param name="request">request</param>
        /// <returns>OperationResponse
        /// Body{
        ///  HasSucceeded = true,
        ///  IsDomainValidationErrors = false,
        ///  ReturnedObject = {numbers} 
        /// }
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> GetMoreNumbers([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody]BSSRequestMore request)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                           .SelectMany(x => x.Errors)
                                                           .Select(x => x.ErrorMessage))
                            });
                        }
                        BSSAPIHelper bsshelper = new BSSAPIHelper();

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration); 

                        DatabaseResponse systemConfigResponse = await _orderAccess.GetConfiguration(ConfiType.System.ToString());

                        DatabaseResponse bssConfigResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                        GridBSSConfi bssConfig = bsshelper.GetGridConfig((List<Dictionary<string, string>>)bssConfigResponse.Results);

                        GridSystemConfig systemConfig = bsshelper.GetGridSystemConfig((List<Dictionary<string, string>>)systemConfigResponse.Results);

                        DatabaseResponse serviceCAF = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

                        DatabaseResponse requestIdResForFreeNumber = await _orderAccess.GetBssApiRequestId(GridMicroservices.Customer.ToString(), BSSApis.GetAssets.ToString(), customerID, (int)BSSCalls.NewSession, "");

                        BSSNumbers numbers = new BSSNumbers();

                        if (request.Type == 1) // free numbers
                        {
                            ResponseObject res = new ResponseObject();
                            try
                            {
                                res=await bsshelper.GetAssetInventory(bssConfig, ((List<ServiceFees>)serviceCAF.Results).FirstOrDefault().ServiceCode, (BSSAssetRequest)requestIdResForFreeNumber.Results, systemConfig.FreeNumberListCount);
                            }

                            catch (Exception ex)
                            {
                                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));

                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed),
                                    IsDomainValidationErrors = false
                                });

                            }                           

                            if (res != null)
                            {
                                numbers.FreeNumbers = bsshelper.GetFreeNumbers(res);

                                //insert these number into database
                                string json = bsshelper.GetJsonString(numbers.FreeNumbers); // json insert

                                DatabaseResponse updateBssCallFeeNumbers = await _orderAccess.UpdateBSSCallNumbers(json, ((BSSAssetRequest)requestIdResForFreeNumber.Results).userid, ((BSSAssetRequest)requestIdResForFreeNumber.Results).BSSCallLogID);
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

                                    DatabaseResponse requestIdResForPremium = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.GetAssets.ToString(), customerID, (int)BSSCalls.NewSession, "");

                                    ResponseObject premumResponse = new ResponseObject();

                                    try
                                    {
                                        premumResponse= await bsshelper.GetAssetInventory(bssConfig, fee.ServiceCode, (BSSAssetRequest)requestIdResForPremium.Results, countPerPremium);
                                    }

                                    catch (Exception ex)
                                    {
                                        LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));

                                        return Ok(new OperationResponse
                                        {
                                            HasSucceeded = false,
                                            Message = EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed),
                                            IsDomainValidationErrors = false
                                        });

                                    }

                                    if (premumResponse != null && premumResponse.Response!=null && premumResponse.Response.asset_details != null)
                                    {
                                        List<PremiumNumbers> premiumNumbers = bsshelper.GetPremiumNumbers(premumResponse, fee);

                                        BSSNumbers bssPremium = new BSSNumbers();

                                        bssPremium.FreeNumbers = bsshelper.GetFreeNumbers(premumResponse);

                                        //insert these number into database
                                        string json = bsshelper.GetJsonString(bssPremium.FreeNumbers); // json insert

                                        DatabaseResponse updateBssCallFeeNumbers = await _orderAccess.UpdateBSSCallNumbers(json, ((BSSAssetRequest)requestIdResForPremium.Results).userid, ((BSSAssetRequest)requestIdResForPremium.Results).BSSCallLogID);

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

                        if (request.Type == 1)
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

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// This will return usage history of mobilenumber passed
        /// </summary>
        /// <param name="token"></param>
        /// <param name="mobileNumber"></param>
        /// <returns>OperationsResponse</returns>
        [HttpGet("{mobileNumber}")]
        public async Task<IActionResult> GetUsageHistory([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] string mobileNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;

                        BSSAPIHelper bsshelper = new BSSAPIHelper();

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        DatabaseResponse systemConfigResponse = await _orderAccess.GetConfiguration(ConfiType.System.ToString());

                        DatabaseResponse bssConfigResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                        GridBSSConfi bssConfig = bsshelper.GetGridConfig((List<Dictionary<string, string>>)bssConfigResponse.Results);

                        GridSystemConfig systemConfig = bsshelper.GetGridSystemConfig((List<Dictionary<string, string>>)systemConfigResponse.Results);

                        DatabaseResponse serviceCAF = await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

                        DatabaseResponse requestIdRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Customer.ToString(), BSSApis.GetAssets.ToString(), customerID, (int)BSSCalls.NoSession, "");

                        BSSQueryPlanResponse numbers = new BSSQueryPlanResponse();

                        BSSQueryPlanResponseObject usageHistory = new BSSQueryPlanResponseObject();


                        try
                        {
                            usageHistory= await bsshelper.GetUsageHistory(bssConfig, mobileNumber, ((BSSAssetRequest)requestIdRes.Results).request_id);
                        }

                        catch (Exception ex)
                        {
                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));

                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed),
                                IsDomainValidationErrors = false
                            });

                        }                    

                        if (usageHistory != null && usageHistory.Response!=null && usageHistory.Response.result_code == "0")
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = true,
                                IsDomainValidationErrors = false,
                                ReturnedObject = usageHistory.Response.bundles,
                                Message= usageHistory.Response.bundles!=null ? EnumExtensions.GetDescription(CommonErrors.UsageHistoryNotAvailable) : "",
                            });

                        }

                        else
                        {
                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedToGetUsageHistory));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.FailedToGetUsageHistory),
                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// This will return Customer's BSS Invoice for the given date range, though optional
        /// </summary>
        /// <param name="token"></param>
        /// body{
        /// "Token":"Auth token"     
        /// }
        /// <returns>OperationsResponse</returns>
        [Route("GetCustomerInvoice")]
        [HttpPost]
        public async Task<IActionResult> GetCustomerInvoice([FromHeader(Name = "Grid-Authorization-Token")] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                           .SelectMany(x => x.Errors)
                                                           .Select(x => x.ErrorMessage))
                            });
                        }                       

                        BSSAPIHelper bsshelper = new BSSAPIHelper();

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        DatabaseResponse systemConfigResponse = await _orderAccess.GetConfiguration(ConfiType.System.ToString());

                        DatabaseResponse bssConfigResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                        GridBSSConfi bssConfig = bsshelper.GetGridConfig((List<Dictionary<string, string>>)bssConfigResponse.Results);

                        GridSystemConfig systemConfig = bsshelper.GetGridSystemConfig((List<Dictionary<string, string>>)systemConfigResponse.Results);

                        DatabaseResponse accountResponse = await _orderAccess.GetCustomerBSSAccountNumber(customerID);

                        if (accountResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                        {
                            if (!string.IsNullOrEmpty(((BSSAccount)accountResponse.Results).AccountNumber))
                            {
                                // Get default daterange in month from config by key - BSSInvoiceDefaultDateRangeInMonths
                                DatabaseResponse dateRangeResponse = ConfigHelper.GetValueByKey(ConfigKeys.BSSInvoiceDefaultDateRangeInMonths.ToString(), _iconfiguration);

                                int rangeInMonths = int.Parse(((string)dateRangeResponse.Results));

                                DatabaseResponse requestIdRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Customer.ToString(), BSSApis.GetInvoiceDetails.ToString(), customerID, 0, "");

                                BSSInvoiceResponseObject invoiceResponse = new BSSInvoiceResponseObject();

                                try
                                {
                                    invoiceResponse= await bsshelper.GetBSSCustomerInvoice(bssConfig, ((BSSAssetRequest)requestIdRes.Results).request_id, ((BSSAccount)accountResponse.Results).AccountNumber, rangeInMonths);
                                }

                                catch (Exception ex)
                                {
                                    LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));

                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = false,
                                        Message = EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed),
                                        IsDomainValidationErrors = false
                                    });

                                }                                
                                                             
                                if (invoiceResponse!=null && invoiceResponse.Response!=null &&  invoiceResponse.Response.result_code == "0")
                                {
                                    // Get download link prefix from config
                                    DatabaseResponse downloadLinkResponse = ConfigHelper.GetValueByKey(ConfigKeys.BSSInvoiceDownloadLink.ToString(), _iconfiguration);

                                    string downloadLinkPrefix = (string)downloadLinkResponse.Results;

                                    if(invoiceResponse.Response.invoice_details!=null && invoiceResponse.Response.invoice_details.recordcnt>0)
                                    {
                                        foreach (Recordset recordset in invoiceResponse.Response.invoice_details.recordset)
                                        {
                                            recordset.download_url = downloadLinkPrefix + recordset.bill_id;

                                        }
                                    }
                                   
                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = true,
                                        IsDomainValidationErrors = false,
                                        Message = invoiceResponse.Response.invoice_details.totalrecordcnt > 0 ? EnumExtensions.GetDescription(DbReturnValue.RecordExists) : EnumExtensions.GetDescription(DbReturnValue.NoRecords),
                                        ReturnedObject = invoiceResponse.Response.invoice_details
                                    });
                                }

                                else
                                {
                                    LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedToGetInvoice));

                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = false,
                                        IsDomainValidationErrors = false,
                                        Message = EnumExtensions.GetDescription(DbReturnValue.NoRecords),

                                    });
                                }
                            }

                            else
                            {
                                // Account Number is empty

                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.BillingAccountNumberEmpty));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    IsDomainValidationErrors = false,
                                    Message = EnumExtensions.GetDescription(CommonErrors.MandatoryFieldMissing),

                                });
                            }
                        }


                        else
                        {
                            // No customer records in accounts table

                            LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.FailedToGetCustomer));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.NoRecords),

                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// This will return customers latest outstanding payment
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("GetBSSOutStatndingPayment")]
        public async Task<IActionResult> GetBSSOutStatndingPayment([FromHeader(Name = "Grid-Authorization-Token")] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                           .SelectMany(x => x.Errors)
                                                           .Select(x => x.ErrorMessage))
                            });
                        }

                        BSSAPIHelper bsshelper = new BSSAPIHelper();

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        DatabaseResponse systemConfigResponse = await _orderAccess.GetConfiguration(ConfiType.System.ToString());

                        DatabaseResponse bssConfigResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                        GridBSSConfi bssConfig = bsshelper.GetGridConfig((List<Dictionary<string, string>>)bssConfigResponse.Results);

                        GridSystemConfig systemConfig = bsshelper.GetGridSystemConfig((List<Dictionary<string, string>>)systemConfigResponse.Results);

                        DatabaseResponse accountResponse = await _orderAccess.GetCustomerBSSAccountNumber(customerID);

                        if (accountResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                        {
                            if (!string.IsNullOrEmpty(((BSSAccount)accountResponse.Results).AccountNumber))
                            {

                                DatabaseResponse requestIdRes = await _orderAccess.GetBssApiRequestId(GridMicroservices.Customer.ToString(), BSSApis.GetInvoiceDetails.ToString(), customerID, 0, "");

                                BSSAccountQuerySubscriberResponse accountOutstandingResponse = new BSSAccountQuerySubscriberResponse();

                                try
                                {
                                    accountOutstandingResponse = await bsshelper.GetBSSOutstandingPayment(bssConfig, ((BSSAssetRequest)requestIdRes.Results).request_id, ((BSSAccount)accountResponse.Results).AccountNumber);
                                }

                                catch (Exception ex)
                                {
                                    LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));

                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = false,
                                        Message = EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed),
                                        IsDomainValidationErrors = false
                                    });

                                }

                                if (accountOutstandingResponse != null && accountOutstandingResponse.Response != null && accountOutstandingResponse.Response.result_code == "0")
                                {
                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = true,
                                        IsDomainValidationErrors = false,
                                        Message = accountOutstandingResponse.Response.dataSet.accountDetails != null ? accountOutstandingResponse.Response.dataSet.accountDetails.param != null ? accountOutstandingResponse.Response.dataSet.accountDetails.param.Count > 0 ? EnumExtensions.GetDescription(DbReturnValue.RecordExists) : EnumExtensions.GetDescription(DbReturnValue.NoRecords) : null : null,
                                        ReturnedObject = accountOutstandingResponse.Response.dataSet.accountDetails
                                    });
                                }

                                else
                                {
                                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.NoRecords));
                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = true,
                                        IsDomainValidationErrors = false,
                                        Message = EnumExtensions.GetDescription(DbReturnValue.NoRecords),

                                    });
                                }
                            }

                            else
                            {
                                // Account Number is empty
                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.BillingAccountNumberEmpty));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    IsDomainValidationErrors = false,
                                    Message = EnumExtensions.GetDescription(CommonErrors.MandatoryFieldMissing),

                                });
                            }
                        }

                        else
                        {
                            // No customer records in accounts table
                            LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.NoRecords));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.NoRecords),

                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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

        [HttpPost("GetCustomerOutStatndingPayment")]
        public async Task<IActionResult> GetCustomerOutStatndingPayment([FromHeader(Name = "Grid-Authorization-Token")] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                           .SelectMany(x => x.Errors)
                                                           .Select(x => x.ErrorMessage))
                            });
                        }
                                              

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        GridAPIHelper gridAPIHelper = new GridAPIHelper();

                        DatabaseResponse accountResponse = await _orderAccess.GetCustomerBSSAccountNumber(customerID);

                        GridOutstanding gridOutstanding = new GridOutstanding();

                        DatabaseResponse gridBillingApiResponse = ConfigHelper.GetValueByKey(ConfigKeys.GridBillingAPIEndPoint.ToString(), _iconfiguration);

                        if (accountResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                        {
                            if (!string.IsNullOrEmpty(((BSSAccount)accountResponse.Results).AccountNumber))
                            {
                                try
                                {
                                    gridOutstanding = await gridAPIHelper.GetOutstanding((string)gridBillingApiResponse.Results,((BSSAccount) accountResponse.Results).AccountNumber);
                                }

                                catch (Exception ex)
                                {
                                    LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.GridBillingAPIConnectionFailed));

                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = false,
                                        Message = EnumExtensions.GetDescription(CommonErrors.GridBillingAPIConnectionFailed),
                                        IsDomainValidationErrors = false
                                    });

                                }

                                if (gridOutstanding != null && gridOutstanding.BillingAccountNumber != null)
                                {
                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = true,
                                        IsDomainValidationErrors = false,
                                        Message = EnumExtensions.GetDescription(DbReturnValue.RecordExists),
                                        ReturnedObject = gridOutstanding
                                    });
                                }

                                else
                                {
                                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.NoRecords));
                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = true,
                                        IsDomainValidationErrors = false,
                                        Message = EnumExtensions.GetDescription(DbReturnValue.NoRecords),

                                    });
                                }
                            }

                            else
                            {
                                // Account Number is empty
                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.BillingAccountNumberEmpty));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    IsDomainValidationErrors = false,
                                    Message = EnumExtensions.GetDescription(CommonErrors.MandatoryFieldMissing),

                                });
                            }
                        }

                        else
                        {
                            // No customer records in accounts table
                            LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.NoRecords));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.NoRecords),

                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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

        [HttpPost("GetCustomerBillingHistoy")]
        public async Task<IActionResult> GetCustomerBillingHistoy([FromHeader(Name = "Grid-Authorization-Token")] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                           .SelectMany(x => x.Errors)
                                                           .Select(x => x.ErrorMessage))
                            });
                        }


                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                        GridAPIHelper gridAPIHelper = new GridAPIHelper();

                        DatabaseResponse accountResponse = await _orderAccess.GetCustomerBSSAccountNumber(customerID);

                        CustomerBillHistory customerBillHistory = new CustomerBillHistory();

                        DatabaseResponse gridBillingApiResponse = ConfigHelper.GetValueByKey(ConfigKeys.GridBillingAPIEndPoint.ToString(), _iconfiguration);

                        if (accountResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                        {
                            if (!string.IsNullOrEmpty(((BSSAccount)accountResponse.Results).AccountNumber))
                            {
                                try
                                {
                                    customerBillHistory = await gridAPIHelper.GetBillingHistory((string)gridBillingApiResponse.Results, ((BSSAccount)accountResponse.Results).AccountNumber);
                                }

                                catch (Exception ex)
                                {
                                    LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.GridBillingAPIConnectionFailed));

                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = false,
                                        Message = EnumExtensions.GetDescription(CommonErrors.GridBillingAPIConnectionFailed),
                                        IsDomainValidationErrors = false
                                    });

                                }

                                if (customerBillHistory != null && customerBillHistory.BillingAccountNumber != null)
                                {
                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = true,
                                        IsDomainValidationErrors = false,
                                        Message = customerBillHistory.BillHistory!=null&& customerBillHistory.BillHistory.Count>0? EnumExtensions.GetDescription(CommonErrors.BillsFound): EnumExtensions.GetDescription(CommonErrors.NoBillsFound),
                                        ReturnedObject = customerBillHistory
                                    });
                                }

                                else
                                {
                                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.NoRecords));
                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = true,
                                        IsDomainValidationErrors = false,
                                        Message = EnumExtensions.GetDescription(DbReturnValue.NoRecords),

                                    });
                                }
                            }

                            else
                            {
                                // Account Number is empty
                                LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.BillingAccountNumberEmpty));
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    IsDomainValidationErrors = false,
                                    Message = EnumExtensions.GetDescription(CommonErrors.MandatoryFieldMissing),

                                });
                            }
                        }

                        else
                        {
                            // No customer records in accounts table
                            LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.NoRecords));
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.NoRecords),

                            });
                        }
                    }

                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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
        /// This will return invoive pdf as base64 encoded bytearray
        /// </summary>
        /// <param name="token"></param>
        /// <param name="invoice_id"></param>
        /// <returns></returns>
        [HttpPost("DownLoadInvoice/{invoice_id}")]
        public async Task<IActionResult> DownLoadInvoice([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] int invoice_id)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    IsDomainValidationErrors = true,
                    Message = EnumExtensions.GetDescription(CommonErrors.TokenEmpty)

                });
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        if (!ModelState.IsValid)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                           .SelectMany(x => x.Errors)
                                                           .Select(x => x.ErrorMessage))
                            });
                        }

                        BSSAPIHelper bsshelper = new BSSAPIHelper();

                        OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);
                       
                        DatabaseResponse downloadLinkResponse = ConfigHelper.GetValueByKey(ConfigKeys.BSSInvoiceDownloadLink.ToString(), _iconfiguration);

                        string downloadLinkPrefix = (string)downloadLinkResponse.Results;

                        DatabaseResponse accountResponse = await _orderAccess.GetCustomerBSSAccountNumber(customerID);

                        byte[] responseObject = null;

                        try
                        {
                            responseObject= await bsshelper.GetInvoiceStream(downloadLinkPrefix + invoice_id);
                        }

                        catch (Exception ex)
                        {
                            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical) + EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed));

                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(CommonErrors.BSSConnectionFailed),
                                IsDomainValidationErrors = false
                            });

                        }                       

                        MiscHelper configHelper = new MiscHelper();

                        if (responseObject!=null && responseObject.Length>0)
                                {
                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = true,
                                        IsDomainValidationErrors = false,
                                        Message= responseObject == null ? "": "Invoice_"+((BSSAccount)accountResponse.Results).AccountNumber+ "_"+ invoice_id + "_" + DateTime.Now.DayOfWeek + "_" + DateTime.Now.ToString("MMMM_dd_yyyy_hh_mm_ss") + ".pdf",
                                        ReturnedObject = responseObject==null?null: configHelper.GetBase64StringFromByteArray(responseObject, ((BSSAccount)accountResponse.Results).AccountNumber + "_Invoice_" + invoice_id + "_" + DateTime.Now.DayOfWeek + "_" + DateTime.Now.ToString("MMMM_dd_yyyy_hh_mm_ss") + ".pdf")
                                    });
                                }

                                else
                                {

                                    LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.NoRecords));

                                    return Ok(new OperationResponse
                                    {
                                        HasSucceeded = true,
                                        IsDomainValidationErrors = false,
                                        Message = EnumExtensions.GetDescription(DbReturnValue.NoRecords),

                                    });
                                }
                     }
                    else
                    {
                        //Token expired

                        LogInfo.Warning(EnumExtensions.GetDescription(CommonErrors.ExpiredToken));

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
                   LogInfo.Warning(EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed));

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