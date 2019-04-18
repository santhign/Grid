using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CatelogService.DataAccess;
using Microsoft.Extensions.Configuration;
using Core.Models;
using Core.Enums;
using Core.Helpers;
using InfrastructureService;
using Core.Extensions;

namespace CatelogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VASesController : ControllerBase
    {       
        IConfiguration _iconfiguration;

        public VASesController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }

        /// <summary>
        /// This will returns value added Services list
        /// </summary>
        /// <param name="token"></param>
        /// <returns>VAS</returns>
        // GET: api/VASes
        [HttpGet]
        public async Task<IActionResult> GetVASes([FromHeader(Name = "Grid-Authorization-Token")] string token)
        {

            try
            {
                AuthHelper helper = new AuthHelper(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await helper.AuthenticateCustomerToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int customerID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
                        VASDataAccess _vasAccess = new VASDataAccess(_iconfiguration);

                        return Ok(new ServerResponse
                        {
                            HasSucceeded = true,
                            Message = StatusMessages.SuccessMessage,
                            Result = await _vasAccess.GetVASes(customerID)

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
        /// Returns details of a value added service specified by the id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>VAS</returns>
        // GET: api/VASes/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVAS([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromRoute] int id)
        {
            try
            {
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
                                Message = StatusMessages.DomainValidationError,
                                IsDomainValidationErrors = true
                            });
                        }

                        VASDataAccess _vasAccess = new VASDataAccess(_iconfiguration);

                        return Ok(new ServerResponse
                        {
                            HasSucceeded = true,
                            Message = StatusMessages.SuccessMessage,
                            Result = (await _vasAccess.GetVASes(customerID)).Where(p => p.VASID == id).FirstOrDefault()

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
    }
}