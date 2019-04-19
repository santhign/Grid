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

                        ResponseObject res = await bsshelper.GetAssetInventory(config, ((List<ServiceFees>)serviceCAF.Results).FirstOrDefault().ServiceCode, (BSSAssetRequest)requestIdRes.Results);

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

                        //Getting FreeNumbers
                        ResponseObject res = await bsshelper.GetAssetInventory(bssConfig, ((List<ServiceFees>)serviceCAF.Results).FirstOrDefault().ServiceCode, (BSSAssetRequest)requestIdResForFreeNumber.Results, systemConfig.FreeNumberListCount);

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

                                        DatabaseResponse requestIdResForPremium = await _orderAccess.GetBssApiRequestId(GridMicroservices.Order.ToString(), BSSApis.GetAssets.ToString(), customerID, (int)BSSCalls.ExistingSession, numbers.FreeNumbers.FirstOrDefault().MobileNumber);

                                        ResponseObject premumResponse = await bsshelper.GetAssetInventory(bssConfig, fee.ServiceCode, (BSSAssetRequest)requestIdResForPremium.Results, countPerPremium);

                                        if (premumResponse != null && premumResponse.Response.asset_details != null)
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
                            ResponseObject res = await bsshelper.GetAssetInventory(bssConfig, ((List<ServiceFees>)serviceCAF.Results).FirstOrDefault().ServiceCode, (BSSAssetRequest)requestIdResForFreeNumber.Results, systemConfig.FreeNumberListCount);

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

                                    ResponseObject premumResponse = await bsshelper.GetAssetInventory(bssConfig, fee.ServiceCode, (BSSAssetRequest)requestIdResForPremium.Results, countPerPremium);

                                    if (premumResponse != null && premumResponse.Response.asset_details != null)
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

                        object usageHistory = await bsshelper.GetUsageHistory(bssConfig, mobileNumber, ((BSSAssetRequest)requestIdRes.Results).request_id);

                        BSSQueryPlanResponse res = bsshelper.GetQueryPlan(usageHistory);

                        //  if (helper.GetResponseCode(usageHistory) == "0")

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
        /// This will return Customer's BSS Invoice for the given date range, though optional
        /// </summary>
        /// <param name="token"></param>
        /// <param name="request">
        /// body{
        /// "Token":"Auth token"
        /// "StartDate" :"1/1/2019", //dd/MM/yyyy - optional
        /// "EndDate" :"15/12/2019", //dd/MM/yyyy  - optional      
        /// }
        /// </param>
        /// <returns>OperationsResponse</returns>
        [Route("GetCustomerInvoice")]
        [HttpPost]
        public async Task<IActionResult> GetCustomerInvoice([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] CustomerBSSInvoiceRequest request)
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
                        // clarify date range -- preferably need to send from UI

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

                                BSSInvoiceResponseObject invoiceResponse = await bsshelper.GetBSSCustomerInvoice(bssConfig, ((BSSAssetRequest)requestIdRes.Results).request_id, ((BSSAccount)accountResponse.Results).AccountNumber);

                                if (invoiceResponse.Response.result_code == "0")
                                {
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
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = true,
                                    IsDomainValidationErrors = false,
                                    Message = EnumExtensions.GetDescription(CommonErrors.MandatoryRecordEmpty),

                                });
                            }
                        }

                        else
                        {
                            // No customer records in accounts table
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