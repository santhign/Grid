using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomerService.Models;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Core.Extensions;
using InfrastructureService;
using Microsoft.Extensions.Configuration;
using CustomerService.DataAccess;
using Newtonsoft.Json;
using Serilog;

namespace CustomerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        IConfiguration _iconfiguration;

        public CustomersController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }
        // GET: api/Customers
        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            try
            {
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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer([FromRoute] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                            .SelectMany(x => x.Errors)
                                            .Select(x => x.ErrorMessage))
                    };
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
        
        [HttpPut("UpdateCustomerProfile/{token}/{password}/{mobileNumber}")]
        public async Task<IActionResult> UpdateCustomerProfile(string token, string password, string mobileNumber)
        {
            try
            {
                if (!ModelState.IsValid)
                {

                    return StatusCode((int) HttpStatusCode.BadRequest,
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
                        return StatusCode((int)HttpStatusCode.InternalServerError, DbReturnValue.UpdationFailed.GetDescription());
                    }
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.NotFound, "Customer does not exist!");
                }
                


            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                return StatusCode((int) HttpStatusCode.InternalServerError,
                    "An error occurred, please try again or contact the administrator.");

            }
        }

        // GET: api/Customers/5/6532432/1
        /// <summary>
        /// This method will return all associated plans for that customer.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="mobileNumber">Mobile Number</param>
        /// <param name="planType">Plan Type</param>
        /// <returns></returns>
        [HttpGet("CustomerPlans/{token}")]
        public async Task<IActionResult> GetCustomerPlans([FromRoute] string token, string mobileNumber, int ? planType)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest,
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
                        return StatusCode((int) HttpStatusCode.NotFound, "Record not found");
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
                    return StatusCode((int) HttpStatusCode.NotFound, "Customer does not exist!");
                }


            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                return StatusCode((int) HttpStatusCode.InternalServerError,
                    "An error occurred, please try again or contact the administrator.");

            }
        }

        /// <summary>Gets the vas plans for customer.</summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <returns>List of VAS plan associated with Customers along with all subscribers</returns>
        [HttpGet("GetSharedVASPlansForCustomer/{token}")]
        public async Task<IActionResult> GetSharedVasPlansForCustomer([FromRoute] string token, string mobileNumber)
        {

            if (!ModelState.IsValid)
            {
                return StatusCode((int) HttpStatusCode.BadRequest,
                    new OperationResponse
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

        /// <summary>Gets the vas plans for customer.</summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="mobileNumber">The mobile number.</param>
        /// <returns>List of VAS plan asscociated with Customers along with all subscribers</returns>
        [HttpGet("GetVASPlansForCustomer/{token}")]
        public async Task<IActionResult> GetVasPlansForCustomer([FromRoute] string token, string mobileNumber)
        {

            if (!ModelState.IsValid)
            {
                return StatusCode((int) HttpStatusCode.BadRequest,
                    new OperationResponse
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

        //// PUT: api/Customers/5
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutCustomer([FromRoute] int id, [FromBody] Customer customer)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (id != customer.CustomerID)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(customer).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException ex)
        //    {
        //        if (!CustomerExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/Customers
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RegisterCustomer customer)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                                 .SelectMany(x => x.Errors)
                                                 .Select(x => x.ErrorMessage))
                    };
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

        //// DELETE: api/Customers/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteCustomer([FromRoute] int id)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var customer = await _context.Customers.FindAsync(id);
        //    if (customer == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Customers.Remove(customer);
        //    await _context.SaveChangesAsync();

        //    return Ok(customer);
        //}

        //private bool CustomerExists(int id)
        //{
        //    return _context.Customers.Any(e => e.CustomerID == id);
        //}  

        /// <summary>
        /// Validate customer's referral code.
        /// Return success or failure flag with message
        /// </summary>
        /// <param name="request"></param>
        /// <returns>LoggedInPrinciple</returns>
        [HttpPost("ValidateReferralCode")]
        public async Task<IActionResult> ValidateReferralCode([FromHeader]string Token, [FromBody]ValidateReferralCodeRequest request)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                            .SelectMany(x => x.Errors)
                                            .Select(x => x.ErrorMessage))
                    };
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
        /// <param name="token"></param>
        /// <returns>OperationResponse</returns>
        [HttpGet("Subscribers/{token}")]
        public async Task<IActionResult> Subscribers([FromRoute]string token)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                            .SelectMany(x => x.Errors)
                                            .Select(x => x.ErrorMessage))
                    };
                }

                CustomerDataAccess _customerAccess = new CustomerDataAccess(_iconfiguration);
                DatabaseResponse tokenAuthResponse = await _customerAccess.AuthenticateCustomerToken(token);
                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    AuthTokenResponse aTokenResp = (AuthTokenResponse)tokenAuthResponse.Results;
                    var getSubscriber = await _customerAccess.GetSubscribers(aTokenResp.CustomerID);
                    if (getSubscriber.ResponseCode == (int)DbReturnValue.RecordExists)
                    {
                        return Ok(new OperationResponse
                        {
                            HasSucceeded = true,
                            Message = EnumExtensions.GetDescription(DbReturnValue.RecordExists),
                            IsDomainValidationErrors = false,
                            ReturnedObject = getSubscriber.Results
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

        // GET: api/Customers/SearchCustomer/abc@gmail.com
        [HttpGet("SearchCustomer/{SearchValue}")]
        public async Task<IActionResult> SearchCustomer([FromRoute] string SearchValue)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                            .SelectMany(x => x.Errors)
                                            .Select(x => x.ErrorMessage))
                    };
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
        ///<param name="emailid">abcd@gmail.com</param>
        /// <returns>Customer Id and Token key</returns>
        [HttpGet]
        [Route("ForgetPassword/{emailid}")]
        public async Task<IActionResult> ForgetPassword([FromRoute] string emailid)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Log.Error(StatusMessages.DomainValidationError);
                    new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                            .SelectMany(x => x.Errors)
                                            .Select(x => x.ErrorMessage))
                    };
                }


                EmailDataAccess _emailDataAccess = new EmailDataAccess(_iconfiguration);

                Emails _emails = new Emails();
                _emails.EmailId = emailid;
                ForgetPassword _forgetPassword = await _emailDataAccess.GetForgetPassword(_emails);

                if (_forgetPassword == null)
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
                        Result = _forgetPassword

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

        [HttpPost("UpdateReferralCode")]
        public async Task<IActionResult> UpdateReferralCode([FromHeader] string Token, [FromBody]CustomerNewReferralCode customerReferralCode)
        {
            try
            { 
                if (!ModelState.IsValid)
                {
                    new OperationResponse
                    {
                        HasSucceeded = false,
                        IsDomainValidationErrors = true,
                        Message = string.Join("; ", ModelState.Values
                                            .SelectMany(x => x.Errors)
                                            .Select(x => x.ErrorMessage))
                    };
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