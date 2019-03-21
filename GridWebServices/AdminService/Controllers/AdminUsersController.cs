using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AdminService.DataAccess;
using Microsoft.Extensions.Configuration;
using Core.Models;
using Core.Helpers;
using Core.Enums;
using Serilog;
using AdminService.Models;


namespace AdminService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AdminUsersController : ControllerBase
    {
        IConfiguration _iconfiguration;

        public AdminUsersController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }

        [HttpPost]
        [Route("GetAdminLoginAuthentication")]
        public async Task<IActionResult> GetAdminLoginAuthentication([FromBody]AdminUserLoginRequest userdetails)
        {
            try
            { 

                if ((string.IsNullOrEmpty(userdetails.Email)) ||(string.IsNullOrEmpty(userdetails.Password)))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.DomainValidationError,
                        IsDomainValidationErrors = true
                    });

                }


                AdminUsersDataAccess _AdminUsersDataAccess = new AdminUsersDataAccess(_iconfiguration);
                

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage,
                    Result = await _AdminUsersDataAccess.GetLoginAuthentication(userdetails)

                });
            }
            catch (Exception ex)
            {
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

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
