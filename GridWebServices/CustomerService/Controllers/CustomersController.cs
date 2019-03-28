using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<IActionResult> ValidateReferralCode([FromBody]ValidateReferralCodeRequest request)
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
                DatabaseResponse tokenAuthResponse = await _customerAccess.AuthenticateCustomerToken(request.Token);
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
        [HttpGet("GetSubscribers/{token}")]
        public async Task<IActionResult> GetSubscribers([FromRoute]string token)
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


    }
}