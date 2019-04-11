using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Core.Extensions;
using InfrastructureService;
using AdminService.DataAccess;
using AdminService.Models;
using Microsoft.Extensions.Configuration;


namespace AdminService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        /// <summary>
        /// The iconfiguration
        /// </summary>
        IConfiguration _iconfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public CustomerController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }
        // GET: api/Customers
        /// <summary>
        /// Gets the customers.
        /// </summary>
        /// <param name="token" in="Header"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetCustomers([FromHeader(Name = "Grid-Authorization-Token")] string token)
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
    }
}