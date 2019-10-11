using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CatelogService.Models;
using CatelogService.DataAccess;
using Core.Models;
using Core.Helpers;
using Core.Enums;
using Core.Extensions;
using InfrastructureService;


namespace CatelogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BundlesController : ControllerBase
    {
        IConfiguration _iconfiguration;

        public BundlesController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }

        /// <summary>
        /// This will provide the list of all Customer selectable flag enabled Bundles.
        /// </summary>        
        /// <returns>Bundles</returns>
        // GET: api/Bundles
        [HttpGet]
        public async Task<IActionResult> GetBundles([FromHeader(Name = "Grid-General-Token")] string Token)
        {
            try
            {
                TokenValidationHelper tokenValidationHelper = new TokenValidationHelper();
                if (!tokenValidationHelper.ValidateGenericToken(Token, _iconfiguration))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = Core.Extensions.EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = true
                    });
                }

                BundleDataAccess _bundleAccess = new BundleDataAccess(_iconfiguration);
                BundleFilter _filter = new BundleFilter();
                List<Bundle> returnObj = await _bundleAccess.GetBundleList(_filter);
                if (returnObj.Count > 0)
                {
                    return Ok(new ServerResponse
                    {
                        HasSucceeded = true,
                        Message = StatusMessages.SuccessMessage,
                        Result = returnObj
                    });
                }
                else
                {
                    return Ok(new ServerResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.NoRecordsFound,
                        Result = returnObj
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
        /// This will provide the list of all Customer selectable flag enabled Bundles.
        /// </summary>        
        /// <returns>Bundles</returns>
        // POST: api/Bundles
        [HttpPost]
        public async Task<IActionResult> GetBundles([FromHeader(Name = "Grid-General-Token")] string Token, [FromBody] BundleFilter filter)
        {
            try
            {
                TokenValidationHelper tokenValidationHelper = new TokenValidationHelper();
                if (!tokenValidationHelper.ValidateGenericToken(Token, _iconfiguration))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = Core.Extensions.EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = true
                    });
                }

                BundleDataAccess _bundleAccess = new BundleDataAccess(_iconfiguration);
                List<Bundle> returnObj = await _bundleAccess.GetBundleList(filter);
                if (returnObj.Count > 0)
                {
                    return Ok(new ServerResponse
                    {
                        HasSucceeded = true,
                        Message = StatusMessages.SuccessMessage,
                        Result = returnObj
                    });
                }
                else
                {
                    return Ok(new ServerResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.NoRecordsFound,
                        Result = returnObj
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
        /// This will provide Bundle details for specific ID passed 
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="id">Bundle ID</param>
        /// <returns>Bundle</returns>
        // GET: api/Bundles/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBundle([FromHeader(Name = "Grid-General-Token")] string Token, [FromRoute] int id)
        {           
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.DomainValidationError,
                        IsDomainValidationErrors = true
                    });
                }

                TokenValidationHelper tokenValidationHelper = new TokenValidationHelper();
                if (!tokenValidationHelper.ValidateGenericToken(Token, _iconfiguration))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = Core.Extensions.EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = true
                    });
                }

                BundleDataAccess _bundleAccess = new BundleDataAccess(_iconfiguration);
                BundleDetails details = new BundleDetails { BundleID = id };
                List<Bundle> returnObj = await _bundleAccess.GetBundleById(details);
                if (returnObj.Count > 0)
                {
                    return Ok(new ServerResponse
                    {
                        HasSucceeded = true,
                        Message = StatusMessages.SuccessMessage,
                        Result = returnObj.FirstOrDefault()
                    });
                }
                else
                {
                    return Ok(new ServerResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.InvalidMessage,
                        Result = returnObj.FirstOrDefault()
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
        /// This will provide the list of all Customer selectable flag enabled Bundles.
        /// </summary>        
        /// <returns>Bundles</returns>
        // POST: api/Bundles
        [HttpPost("GetBundlesByID")]
        public async Task<IActionResult> GetBundlesByID([FromHeader(Name = "Grid-General-Token")] string Token, [FromBody] BundleDetails details)
        {
            try
            {
                TokenValidationHelper tokenValidationHelper = new TokenValidationHelper();
                if (!tokenValidationHelper.ValidateGenericToken(Token, _iconfiguration))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = Core.Extensions.EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = true
                    });
                }

                BundleDataAccess _bundleAccess = new BundleDataAccess(_iconfiguration);
                List<Bundle> returnObj = await _bundleAccess.GetBundleById(details);
                if (returnObj.Count > 0)
                {
                    return Ok(new ServerResponse
                    {
                        HasSucceeded = true,
                        Message = StatusMessages.SuccessMessage,
                        Result = returnObj.FirstOrDefault()
                    });
                }
                else
                {
                    return Ok(new ServerResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.NoRecordsFound,
                        Result = returnObj.FirstOrDefault()
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
        /// Promotion Bundle for a bundle id and promotion code. Retuns the new bundle for the id passed which is matching the promotion code.
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="id">Bundle ID</param>
        /// <param name="promocode">Promotion code</param>
        /// <returns>Bundle</returns>
        // GET: api/Bundles/5
        [HttpGet("{id}/{promocode}")]
        public async Task<IActionResult> GetBundle([FromHeader(Name = "Grid-General-Token")] string Token, [FromRoute] int id, string promocode)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.DomainValidationError,
                        IsDomainValidationErrors = true
                    });
                }

                TokenValidationHelper tokenValidationHelper = new TokenValidationHelper();
                if (!tokenValidationHelper.ValidateGenericToken(Token, _iconfiguration))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = Core.Extensions.EnumExtensions.GetDescription(DbReturnValue.TokenAuthFailed),
                        IsDomainValidationErrors = true
                    });
                }

                BundleDataAccess _bundleAccess = new BundleDataAccess(_iconfiguration);
                List<Bundle> returnObj = await _bundleAccess.GetBundleByPromocode(id, promocode);
                if (returnObj.Count > 0)
                {
                    return Ok(new ServerResponse
                    {
                        HasSucceeded = true,
                        Message = StatusMessages.SuccessMessage,
                        Result = returnObj.FirstOrDefault()
                    });
                }
                else
                {
                    return Ok(new ServerResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.InvalidMessage,
                        Result = returnObj.FirstOrDefault()
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
        /// <param name="MobileNumber"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCustomerBundleListing/{MobileNumber}")]
        public async Task<IActionResult> GetCustomerBundleListing([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] string MobileNumber)
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
                        var customerAccess = new BundleDataAccess(_iconfiguration);

                        DatabaseResponse _basePlan = await customerAccess.GetCustomerBundleListing(customerID, MobileNumber);

                        if (_basePlan.Results == null)
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
                                Result = _basePlan.Results

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