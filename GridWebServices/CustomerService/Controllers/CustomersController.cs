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
using InfrastructureService;
using Microsoft.Extensions.Configuration;
using CustomerService.DataAccess;
using Serilog;
using System.Net.Mail;


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
        // GET: api/Customers
        /// <summary>
        /// Gets the customers.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK,
                        new OperationResponse
                        {
                            HasSucceeded = false,
                            IsDomainValidationErrors = true,
                            Message = string.Join("; ", ModelState.Values
                                            .SelectMany(x => x.Errors)
                                            .Select(x => x.ErrorMessage))
                        });
                }
                // throw new Exception("test");
                CustomerDataAccess _customerAccess = new CustomerDataAccess(_iconfiguration);

                List<Customer> customerList = new List<Customer>();

                customerList = await _customerAccess.GetCustomers();

                if (customerList == null || customerList.Count == 0)
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
                        Result = customerList

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

        // GET: api/Customers/5
        /// <summary>
        /// Gets the customer.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer([FromRoute] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.OK, 
                        new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                            .SelectMany(x => x.Errors)
                                            .Select(x => x.ErrorMessage))
                    });
                }

                CustomerDataAccess _customerAccess = new CustomerDataAccess(_iconfiguration);

                Customer customer = await _customerAccess.GetCustomer(id);

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
        /// <param name="password">The password.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <returns>
        /// Success or Failure status code
        /// </returns>
        [HttpPut("UpdateCustomerProfile/{token}/{password}/{mobileNumber}")]
        public async Task<IActionResult> UpdateCustomerProfile(string token, string password, string mobileNumber)
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
                var customerAccess = new CustomerDataAccess(_iconfiguration);
                var tokenAuthResponse = await customerAccess.AuthenticateCustomerToken(token);
                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;
                    var statusResponse = await customerAccess.UpdateCustomerProfile(new CustomerProfile
                        { CustomerId = aTokenResp.CustomerID, MobileNumber = mobileNumber, Password = password });

                    if (statusResponse.ResponseCode == (int)DbReturnValue.UpdateSuccess)
                    {
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
                    LogInfo.Error(DbReturnValue.TokenAuthFailed.GetDescription());

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = DbReturnValue.TokenAuthFailed.GetDescription(),
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
        /// <param name="token">The token.</param>
        /// <param name="mobileNumber">Mobile Number</param>
        /// <param name="planType">Plan Type</param>
        /// <returns></returns>
        /// <exception cref="Exception">Customer record not found for " + token + " token</exception>
        [HttpGet("CustomerPlans/{token}")]
        public async Task<IActionResult> GetCustomerPlans([FromRoute] string token, string mobileNumber, int ? planType)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(
                        new OperationResponse
                        {
                            HasSucceeded = false,
                            IsDomainValidationErrors = true,
                            Message = string.Join("; ", ModelState.Values
                                .SelectMany(x => x.Errors)
                                .Select(x => x.ErrorMessage))
                        });
                }

                var customerAccess = new CustomerDataAccess(_iconfiguration);
                var tokenAuthResponse = await customerAccess.AuthenticateCustomerToken(token);
                if (tokenAuthResponse.ResponseCode == (int) DbReturnValue.AuthSuccess)
                {
                    var aTokenResp = (AuthTokenResponse) tokenAuthResponse.Results;
                    var customerPlans =
                        await customerAccess.GetCustomerPlans(aTokenResp.CustomerID, mobileNumber, planType);

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
                    // Raising an exception as customer record was not found. So it is critical error.
                    throw new Exception("Customer record not found for " + token + " token");
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
        /// Gets the vas plans for customer.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <returns>
        /// List of Shared VAS plan associated with Customers along with all subscribers
        /// </returns>
        [HttpGet("GetSharedVASPlansForCustomer/{token}")]
        public async Task<IActionResult> GetSharedVasPlansForCustomer([FromRoute] string token, string mobileNumber)
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

            return await GetCustomerPlans(token, mobileNumber, Convert.ToInt32(Core.Enums.PlanType.Shared_VAS));
        }

        /// <summary>
        /// Gets the vas plans for customer.
        /// </summary>
        /// <param name="token">The customer identifier.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <returns>
        /// List of VAS plan associated with Customers along with all subscribers
        /// </returns>
        [HttpGet("GetVASPlansForCustomer/{token}")]
        public async Task<IActionResult> GetVasPlansForCustomer([FromRoute] string token, string mobileNumber)
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

            return await GetCustomerPlans(token, mobileNumber, Convert.ToInt32(Core.Enums.PlanType.VAS));
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
                    await publisher.PublishAsync(response.Results, ConfigHelper.GetValueByKey("SNS_Subject_CreateCustomer", _iconfiguration).Results.ToString().Trim());

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
        /// <param name="Token">The token.</param>
        /// <param name="request">The request.</param>
        /// <returns>
        /// LoggedInPrinciple
        /// </returns>
        [HttpPost("ValidateReferralCode")]
        public async Task<IActionResult> ValidateReferralCode([FromHeader]string Token, [FromBody]ValidateReferralCodeRequest request)
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
                DatabaseResponse tokenAuthResponse = await _customerAccess.AuthenticateCustomerToken(Token);
                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;
                    var validationResponse = await _customerAccess.ValidateReferralCode(aTokenResp.CustomerID, request.ReferralCode);
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
        /// <param name="token">Customer token</param>
        /// <returns>
        /// OperationResponse
        /// </returns>
        [HttpGet("Subscribers/{token}")]
        public async Task<IActionResult> Subscribers([FromRoute]string token)
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

                var customerAccess = new CustomerDataAccess(_iconfiguration);
                var tokenAuthResponse = await customerAccess.AuthenticateCustomerToken(token);
                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
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
                    // token auth failure
                    LogInfo.Error(DbReturnValue.TokenAuthFailed.GetDescription());

                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = DbReturnValue.TokenAuthFailed.GetDescription(),
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
        /// <param name="SearchValue">The search value.</param>
        /// <returns></returns>
        [HttpGet("SearchCustomer/{SearchValue}")]
        public async Task<IActionResult> SearchCustomer([FromRoute] string SearchValue)
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
        public async Task<IActionResult> ForgetPassword([FromHeader(Name = "Grid-Authorization-Token")] string token,[FromRoute] string email)
        {
            try
            {
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
                            // get system config- reset password url and email config form db and send a plain email with the reset url



                            return Ok(new ServerResponse
                            {
                                HasSucceeded = true,
                                Message = StatusMessages.SuccessMessage,
                                Result = _forgetPassword

                            });

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
        /// Updates the referral code.
        /// </summary>
        /// <param name="Token">The token.</param>
        /// <param name="customerReferralCode">The customer referral code.</param>
        /// <returns></returns>
        [HttpPost("UpdateReferralCode")]
        public async Task<IActionResult> UpdateReferralCode([FromHeader] string Token, [FromBody]CustomerNewReferralCode customerReferralCode)
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
                DatabaseResponse tokenAuthResponse = await _customerAccess.AuthenticateCustomerToken(Token);
                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;
                    var validationResponse = await _customerAccess.UpdateReferralCode (aTokenResp.CustomerID, customerReferralCode.ReferralCode);
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