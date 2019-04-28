using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CustomerService.Models;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Core.Extensions;
using Core.DataAccess;
using InfrastructureService;
using Microsoft.Extensions.Configuration;
using CustomerService.DataAccess;
using Serilog;
using System.Net.Mail;
using InfrastructureService.MessageQueue;
using Newtonsoft.Json;

namespace CustomerService.Controllers
{
    /// <summary>
    /// Customers Controller class
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        /// <summary>
        /// The iconfiguration
        /// </summary>
        IConfiguration _iconfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomersController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public CustomersController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }       

        // GET: api/Customers/5
        /// <summary>
        /// Gets the customer.
        /// </summary>
        /// <param name="token" in="Header"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetCustomer([FromHeader(Name = "Grid-Authorization-Token")] string token)
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

                        CustomerDataAccess _customerAccess = new CustomerDataAccess(_iconfiguration);

                        Customer customer = await _customerAccess.GetCustomer(customerID);

                        if (customer == null)
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.NotExists)

                            });
                        }
                        else
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = StatusMessages.SuccessMessage,
                                Result = customer

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
        /// Updates the customer profile.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="_profile">The profile details.</param>
        /// <returns></returns>
        [HttpPost("UpdateCustomerProfile")]
        public async Task<IActionResult> UpdateCustomerProfile([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] CustomerProfile _profile)
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

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.Customer_UpdateProfile);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {

                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
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

                        var customerAccess = new CustomerDataAccess(_iconfiguration);

                        var statusResponse = await customerAccess.UpdateCustomerProfile(((AuthTokenResponse)tokenAuthResponse.Results).CustomerID, new CustomerProfile
                        {
                            MobileNumber = _profile.MobileNumber,
                            Password = _profile.Password,
                            Email = _profile.Email
                        });

                        if (statusResponse.ResponseCode == (int)DbReturnValue.UpdateSuccess)
                        {
                            ProfileMQ msgBody = new ProfileMQ();
                            Dictionary<string, string> attribute = new Dictionary<string, string>();
                            string topicName = string.Empty, subject = string.Empty;
                            MQDataAccess _MQDataAccess = new MQDataAccess(_iconfiguration);
                            try
                            {
                                msgBody = await _MQDataAccess.GetProfileUpdateMessageBody(((AuthTokenResponse)tokenAuthResponse.Results).CustomerID);

                                topicName = ConfigHelper.GetValueByKey(ConfigKey.SNS_Topic_ChangeRequest.GetDescription(), _iconfiguration).Results.ToString().Trim();
                                attribute.Add(EventTypeString.EventType, Core.Enums.RequestType.EditContact.GetDescription());
                                var pushResult = await _MQDataAccess.PublishMessageToMessageQueue(topicName, msgBody, attribute);
                                if (pushResult.Trim().ToUpper() == "OK")
                                {


                                    MessageQueueRequest queueRequest = new MessageQueueRequest
                                    {
                                        Source = Source.ChangeRequest,
                                        NumberOfRetries = 1,
                                        SNSTopic = topicName,
                                        CreatedOn = DateTime.Now,
                                        LastTriedOn = DateTime.Now,
                                        PublishedOn = DateTime.Now,
                                        MessageAttribute = Core.Enums.RequestType.Terminate.GetDescription().ToString(),
                                        MessageBody = JsonConvert.SerializeObject(msgBody),
                                        Status = 1
                                    };
                                    await _MQDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                                }
                                else
                                {
                                    MessageQueueRequest queueRequest = new MessageQueueRequest
                                    {
                                        Source = Source.ChangeRequest,
                                        NumberOfRetries = 1,
                                        SNSTopic = topicName,
                                        CreatedOn = DateTime.Now,
                                        LastTriedOn = DateTime.Now,
                                        PublishedOn = DateTime.Now,
                                        MessageAttribute = Core.Enums.RequestType.Terminate.GetDescription().ToString(),
                                        MessageBody = JsonConvert.SerializeObject(msgBody),
                                        Status = 0
                                    };
                                    await _MQDataAccess.InsertMessageInMessageQueueRequest(queueRequest);
                                }
                            }
                            catch (Exception ex)
                            {
                                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                            }
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = StatusMessages.SuccessMessage,
                                Result = statusResponse
                            });
                        }
                        else
                        {
                            LogInfo.Error(DbReturnValue.NoRecords.GetDescription());

                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = DbReturnValue.UpdationFailed.GetDescription(),
                                IsDomainValidationErrors = false
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
        

        // GET: api/Customers/5/6532432/1
        /// <summary>
        /// This method will return all associated plans for that customer.
        /// </summary>   
        /// <param name="token" in="Header"></param>
        /// <param name="mobileNumber">Mobile Number</param>
        /// <param name="planType">Plan Type</param>  
        /// <returns></returns>
        /// <exception cref="Exception">Customer record not found for " + token + " token</exception>
        [HttpGet("CustomerPlans")]
        public async Task<IActionResult> GetCustomerPlans([FromHeader(Name = "Grid-Authorization-Token")] string token, string mobileNumber, int ? planType)
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

                        var customerAccess = new CustomerDataAccess(_iconfiguration);

                        var customerPlans =  await customerAccess.GetCustomerPlans(((AuthTokenResponse)tokenAuthResponse.Results).CustomerID, mobileNumber, planType);

                        if (customerPlans == null)
                        {
                            return Ok(new OperationResponse
                            {
                                // I am marking HasSucceeded as true assuming that Customer can have 0 plan so its not an error.
                                HasSucceeded = true,
                                Message = DbReturnValue.NoRecords.GetDescription(),
                                IsDomainValidationErrors = false
                            });
                        }
                        else
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = StatusMessages.SuccessMessage,
                                Result = customerPlans
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
        /// This method will return all associated shared plans for that customer.
        /// </summary>   
        /// <param name="token" in="Header"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Customer record not found for " + token + " token</exception>
        [HttpGet("CustomerSharedPlans")]
        public async Task<IActionResult> GetCustomerSharedPlans([FromHeader(Name = "Grid-Authorization-Token")] string token)
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

                        var customerAccess = new CustomerDataAccess(_iconfiguration);

                        var customerPlans = await customerAccess.GetCustomerSharedPlans(((AuthTokenResponse)tokenAuthResponse.Results).CustomerID);

                        if (customerPlans == null)
                        {
                            return Ok(new OperationResponse
                            {
                                // I am marking HasSucceeded as true assuming that Customer can have 0 plan so its not an error.
                                HasSucceeded = true,
                                Message = DbReturnValue.NoRecords.GetDescription(),
                                IsDomainValidationErrors = false
                            });
                        }
                        else
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = StatusMessages.SuccessMessage,
                                Result = customerPlans
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


        // POST: api/Customers
        /// <summary>
        /// Creates the specified customer.
        /// </summary>     
        /// <param name="customer">The customer.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RegisterCustomer customer)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                                 .SelectMany(x => x.Errors)
                                                 .Select(x => x.ErrorMessage))
                    });
                }

                CustomerDataAccess _customerAccess = new CustomerDataAccess(_iconfiguration);

                DatabaseResponse response = await _customerAccess.CreateCustomer(customer);


                if (response.ResponseCode == ((int)DbReturnValue.EmailExists))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.EmailExists),
                        IsDomainValidationErrors = true
                    });
                }
                else
                {
                    //Pushed to message queue
                    var publisher = new InfrastructureService.MessageQueue.Publisher(_iconfiguration, ConfigHelper.GetValueByKey("SNS_Topic_CreateCustomer", _iconfiguration).Results.ToString().Trim());
                    Dictionary<string, string> attr = new Dictionary<string, string>();
                    attr.Add("evet_type", "NewCustomer");
                    await publisher.PublishAsync(response.Results, attr);

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = true,
                        Message = EnumExtensions.GetDescription(DbReturnValue.CreateSuccess),
                        IsDomainValidationErrors = false,
                        ReturnedObject = response.Results
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
        /// Validate customer's referral code.
        /// Return success or failure flag with message
        /// </summary>
        /// <param name="token" in="Header"></param>
        /// <param name="request">The request.</param>
        /// <returns>
        /// LoggedInPrinciple
        /// </returns>
        [HttpPost("ValidateReferralCode")]
        public async Task<IActionResult> ValidateReferralCode([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody]ValidateReferralCodeRequest request)
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
                        CustomerDataAccess _customerAccess = new CustomerDataAccess(_iconfiguration);
                        var validationResponse = await _customerAccess.ValidateReferralCode(((AuthTokenResponse)tokenAuthResponse.Results).CustomerID, request.ReferralCode);
                        if (validationResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = true,
                                Message = EnumExtensions.GetDescription(DbReturnValue.RecordExists),
                                IsDomainValidationErrors = false,
                                ReturnedObject = validationResponse.Results
                            });
                        }
                        else
                        {
                            //Unable to validate the referral code
                            LogInfo.Error(EnumExtensions.GetDescription(DbReturnValue.NoRecords));

                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.NoRecords),
                                IsDomainValidationErrors = false
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
        /// Return Subscribers api with MobileNumber, DisplayName, SIMID, PremiumType, ActivatedOn, IsPrimary
        /// </summary>
        /// <param name="token" in="Header"></param>
        /// <returns>
        /// OperationResponse
        /// </returns>
        [HttpGet("Subscribers")]
        public async Task<IActionResult> Subscribers([FromHeader(Name = "Grid-Authorization-Token")] string token)
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

                        var customerAccess = new CustomerDataAccess(_iconfiguration);                     
                       
                            var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;
                            var getSubscriber = await customerAccess.GetSubscribers(aTokenResp.CustomerID);
                            if (getSubscriber.ResponseCode == (int)DbReturnValue.RecordExists)
                            {
                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = true,
                                    Message = DbReturnValue.RecordExists.GetDescription(),
                                    IsDomainValidationErrors = false,
                                    ReturnedObject = getSubscriber.Results
                                });
                            }
                            else
                            {
                                //Unable to validate the referral code
                                LogInfo.Error(DbReturnValue.NoRecords.GetDescription());

                                return Ok(new OperationResponse
                                {
                                    HasSucceeded = false,
                                    Message = DbReturnValue.NoRecords.GetDescription(),
                                    IsDomainValidationErrors = false
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

        // GET: api/Customers/SearchCustomer/abc@gmail.com
        /// <summary>
        /// Searches the customer.
        /// </summary>
        /// <param name="token" in="Header"></param>
        /// <param name="SearchValue">The search value.</param>
        /// <returns></returns>
        [HttpGet("SearchCustomer/{SearchValue}")]
        public async Task<IActionResult> SearchCustomer([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] string SearchValue)
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
                        if (!ModelState.IsValid)
                        {
                            return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                    .SelectMany(x => x.Errors)
                                                    .Select(x => x.ErrorMessage))
                            });
                        }

                        CustomerDataAccess _customerAccess = new CustomerDataAccess(_iconfiguration);

                        List<CustomerSearch> customersearchlist = await _customerAccess.GetSearchCustomers(SearchValue);

                        if (customersearchlist == null || customersearchlist.Count == 0)
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.NotExists)

                            });
                        }
                        else
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = StatusMessages.SuccessMessage,
                                Result = customersearchlist

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
        /// This will send forget password mail
        /// </summary>      
        /// <param name="email">abcd@gmail.com</param>
        /// <returns>
        /// Customer Id and Token key
        /// </returns>
        [HttpGet]
        [Route("ForgetPassword/{email}")]
        public async Task<IActionResult> ForgetPassword([FromRoute] string email)
        {
            try
            {
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
                try

                {
                    MailAddress m = new MailAddress(email);
                }
                catch
                {
                    Log.Error(StatusMessages.DomainValidationError);
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = EnumExtensions.GetDescription(CommonErrors.InvalidEmail),
                    });

                }

                EmailDataAccess _emailDataAccess = new EmailDataAccess(_iconfiguration);

                DatabaseResponse _forgetPassword = await _emailDataAccess.GetForgetPassword(email);

                if (_forgetPassword.ResponseCode == (int)DbReturnValue.CreateSuccess)
                {

                    ForgetPassword passwordTokenDetails = new ForgetPassword();

                    ConfigDataAccess _configAccess = new ConfigDataAccess(_iconfiguration);

                    //Get password reset Url and aws notification topic name

                    DatabaseResponse forgotPasswordMsgConfig = await _configAccess.GetConfiguration(ConfiType.ForgotPasswordMsg.ToString());

                    DatabaseResponse forgotPasswordMsgTemplate = await _configAccess.GetEmailNotificationTemplate(NotificationEvent.ForgetPassword.ToString());

                    MiscHelper parser = new MiscHelper();

                    ForgotPasswordMsgConfig forgotPasswordConfig = parser.GetResetPasswordNotificationConfig((List<Dictionary<string, string>>)forgotPasswordMsgConfig.Results);

                    if (_forgetPassword.Results != null)
                    {
                        passwordTokenDetails = (ForgetPassword)_forgetPassword.Results;

                        NotificationMessage notificationMessage = new NotificationMessage();

                        List<NotificationParams> msgParamsList = new List<NotificationParams>();

                        NotificationParams msgParams = new NotificationParams();

                        msgParams.emailaddress = passwordTokenDetails.Email;

                        msgParams.name = passwordTokenDetails.Name;

                        msgParams.param1 = passwordTokenDetails.Token;

                        msgParams.param2 = forgotPasswordConfig.PasswordResetUrl;

                        msgParamsList.Add(msgParams);

                        notificationMessage = new NotificationMessage
                        {

                            MessageType = NotificationMsgType.Email.ToString(),

                            MessageName = NotificationEvent.ForgetPassword.ToString(),

                            Message = new MessageObject { parameters = msgParamsList, messagetemplate = ((EmailTemplate)forgotPasswordMsgTemplate.Results).TemplateName }

                        };

                        // Publish notification to topic

                        Publisher forgotPassNotificationPublisher = new Publisher(_iconfiguration, forgotPasswordConfig.ForgotPasswordSNSTopic);

                        await forgotPassNotificationPublisher.PublishAsync(notificationMessage);


                        return Ok(new ServerResponse
                        {
                            HasSucceeded = true,
                            Message = StatusMessages.ResetPassowrdLinkSent

                        });
                    }

                    else
                    {
                        // unable to retrieve password token details

                        return Ok(new ServerResponse
                        {
                            HasSucceeded = false,
                            Message = EnumExtensions.GetDescription(CommonErrors.UnableToRetrievePasswordToken)
                        });
                    }

                }
                else if (_forgetPassword.ResponseCode == (int)DbReturnValue.CreationFailed)
                {
                    return Ok(new ServerResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(CommonErrors.TokenGenerationFailed)

                    });
                }

                else
                {
                    return Ok(new ServerResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.EmailNotExists)

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
        /// Updates the referral code.
        /// </summary>
        /// <param name="token" in="Header"></param>
        /// <param name="customerReferralCode">The customer referral code.</param>
        /// <returns></returns>
        [HttpPost("UpdateReferralCode")]
        public async Task<IActionResult> UpdateReferralCode([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody]CustomerNewReferralCode customerReferralCode)
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

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.Customer_ReferralCodeUpdate);               
              
                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        if (!ModelState.IsValid)
                        {
                            return StatusCode((int)HttpStatusCode.OK, new OperationResponse
                            {
                                HasSucceeded = false,
                                IsDomainValidationErrors = true,
                                Message = string.Join("; ", ModelState.Values
                                                    .SelectMany(x => x.Errors)
                                                    .Select(x => x.ErrorMessage))
                            });
                        }

                        CustomerDataAccess _customerAccess = new CustomerDataAccess(_iconfiguration);

                        AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;

                        var validationResponse = await _customerAccess.UpdateReferralCode(aTokenResp.CustomerID, customerReferralCode.ReferralCode);

                        if (validationResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                        {
                            return Ok(new OperationResponse
                            {
                                HasSucceeded = true,
                                Message = EnumExtensions.GetDescription(DbReturnValue.RecordExists),
                                IsDomainValidationErrors = false,
                                ReturnedObject = validationResponse.Results
                            });
                        }
                        else
                        {
                            //Unable to validate the referral code
                            LogInfo.Error(EnumExtensions.GetDescription(DbReturnValue.NoRecords));

                            return Ok(new OperationResponse
                            {
                                HasSucceeded = false,
                                Message = EnumExtensions.GetDescription(DbReturnValue.NoRecords),
                                IsDomainValidationErrors = false
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
        /// 
        /// </summary>
        /// <param name="token" in="Header"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetBillingAddress")]
        public async Task<IActionResult> GetBillingAddress([FromHeader(Name = "Grid-Authorization-Token")] string token)
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
                        var customerAccess = new CustomerDataAccess(_iconfiguration);

                        var customerBilling = await customerAccess.GetCustomerBillingDetails(customerID);

                        if (customerBilling == null)
                        {
                            return Ok(new OperationResponse
                            {
                                // I am marking HasSucceeded as true assuming that Customer can have 0 plan so its not an error.
                                HasSucceeded = true,
                                Message = DbReturnValue.NoRecords.GetDescription(),
                                IsDomainValidationErrors = false
                            });
                        }
                        else
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = StatusMessages.SuccessMessage,
                                Result = customerBilling
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
        /// 
        /// </summary>
        /// <param name="token" in="Header"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetPaymentMethod")]
        public async Task<IActionResult> GetPaymentMethod([FromHeader(Name = "Grid-Authorization-Token")] string token)
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
                        var customerAccess = new CustomerDataAccess(_iconfiguration);

                        var customerPaymentMethod = await customerAccess.GetPaymentMethod(customerID);

                        if (customerPaymentMethod == null)
                        {
                            return Ok(new OperationResponse
                            {
                                // I am marking HasSucceeded as true assuming that Customer can have 0 plan so its not an error.
                                HasSucceeded = true,
                                Message = DbReturnValue.NoRecords.GetDescription(),
                                IsDomainValidationErrors = false
                            });
                        }
                        else
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = StatusMessages.SuccessMessage,
                                Result = customerPaymentMethod
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
        /// 
        /// </summary>
        /// <param name="token" in="Header"></param>
        /// <param name="_subscription"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateSubscription")]
        public async Task<IActionResult> UpdateSubscription([FromHeader(Name = "Grid-Authorization-Token")] string token, customerSubscription _subscription)
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

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.Orders_UpdateSubscription);

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
                        var customerAccess = new CustomerDataAccess(_iconfiguration);

                        var customer = await customerAccess.UpdateSubscriptionDetails(customerID, _subscription);

                        if (customer == null)
                        {
                            return Ok(new OperationResponse
                            {
                                // I am marking HasSucceeded as true assuming that Customer can have 0 plan so its not an error.
                                HasSucceeded = true,
                                Message = DbReturnValue.NoRecords.GetDescription(),
                                IsDomainValidationErrors = false
                            });
                        }
                        else
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = StatusMessages.SuccessMessage,
                                Result = customer
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


        // GET: api/Customers/orders/5
        /// <summary>
        /// Gets the customer orders.
        /// </summary>
        /// <param name="token" in="Header"></param>
        /// <returns></returns>
        [HttpGet("Orders")]
        public async Task<IActionResult> GetCustomerOrders([FromHeader(Name = "Grid-Authorization-Token")] string token)
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

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token, APISources.Dashboard_CustomerOrders);

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

                        CustomerDataAccess _customerAccess = new CustomerDataAccess(_iconfiguration);

                        DatabaseResponse orderDetailsResponse = await _customerAccess.GetCustomerOrders(customerID);

                        if (orderDetailsResponse.Results == null)
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = EnumExtensions.GetDescription(DbReturnValue.NotExists)

                            });
                        }
                        else
                        {
                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = StatusMessages.SuccessMessage,
                                Result = orderDetailsResponse.Results

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