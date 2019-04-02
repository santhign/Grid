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
using System.Linq;
using Core.Extensions;
using InfrastructureService;
using System.Collections.Generic;

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

        /// <summary>
        /// This will check user authentication against email and password
        /// </summary>
        /// <param name="userdetails"></param>
        /// <returns>LoggedInPrinciple</returns>
        /// POST: api/GetAdminLoginAuthentication
        ///Body: 
        ///{
        ///	"Email" : "abcd@gmail.com",
        ///	"Password" : "xyz" 
        ///}
        [HttpPost]
        [Route("GetAdminLoginAuthentication")]
        public async Task<IActionResult> GetAdminLoginAuthentication([FromBody]AdminUserLoginRequest userdetails)
        {
            try
            {

                if ((string.IsNullOrEmpty(userdetails.Email)) || (string.IsNullOrEmpty(userdetails.Password)))
                {
                    Log.Error(StatusMessages.MissingRequiredFields);
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = StatusMessages.MissingRequiredFields,
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


        /// <summary>
        /// This will create new admin user
        /// </summary>
        /// <param name="adminuser"></param>
        /// <returns>created user details</returns>
        /// POST: api/Create
        ///Body: 
        ///{
        ///	"Email" : "abcd@gmail.com",
        ///	"Password" : "xyz",
        ///	"DepartmentID" : 1,
        ///	"OfficeID" : 1,
        ///	"RoleID" : 1
        ///}
        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] RegisterAdminUser adminuser)
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

                AdminUsersDataAccess _adminUsersDataAccess = new AdminUsersDataAccess(_iconfiguration);

                DatabaseResponse response = await _adminUsersDataAccess.CreateAdminUser(adminuser);


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
        /// This will  get admin user details based on specific id supplied
        /// </summary>
        /// <param name="id">1</param>
        /// <returns>get user details with specific id</returns>  
        /// 
        // GET: api/GetAdminUser/1
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetAdminUser([FromRoute] int id)
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


                AdminUsersDataAccess _adminUsersDataAccess = new AdminUsersDataAccess(_iconfiguration);


                AdminUsers adminusers = await _adminUsersDataAccess.GetAdminUser(id);

                if (adminusers == null)
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
                        Result = adminusers

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
        /// This will get all admin users
        /// </summary>
        /// <param></param>
        /// <returns>get all user details</returns> 
        /// 
        // GET: api/GetAdminusers
        [HttpGet]
        public async Task<IActionResult> GetAdminusers()
        {
            try
            {

                AdminUsersDataAccess _adminUsersDataAccess = new AdminUsersDataAccess(_iconfiguration);

                List<AdminUsers> AdminUsersList = new List<AdminUsers>();

                AdminUsersList = await _adminUsersDataAccess.GetAdminusers();

                if (AdminUsersList == null || AdminUsersList.Count == 0)
                {
                    Log.Error(EnumExtensions.GetDescription(DbReturnValue.NotExists));

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
                        Result = AdminUsersList

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


        [HttpPost]
        [Route("UpdateAdminUser")]
        public async Task<IActionResult> UpdateAdminUser([FromBody] AdminProfile adminuser)
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

                AdminUsersDataAccess _adminUsersDataAccess = new AdminUsersDataAccess(_iconfiguration);

                DatabaseResponse response = await _adminUsersDataAccess.UpdateAdminUser(adminuser);

                if (response.ResponseCode == ((int)DbReturnValue.EmailNotExists))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.EmailNotExists),
                        IsDomainValidationErrors = true
                    });
                }
                else
                {
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
    }
}
