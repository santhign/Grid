﻿using System;
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
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

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

                DatabaseResponse response = await _AdminUsersDataAccess.GetLoginAuthentication(userdetails);

                if (response.ResponseCode == ((int)DbReturnValue.EmailNotExists))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.EmailNotExists),
                        IsDomainValidationErrors = true
                    });
                }
                else if (response.ResponseCode == ((int)DbReturnValue.PasswordIncorrect))
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.PasswordIncorrect),
                        IsDomainValidationErrors = true
                    });
                }

                else if (response.ResponseCode == ((int)DbReturnValue.AuthSuccess))
                {
                    //Authentication success

                    var adminuser = new AdminUsers();

                    adminuser = (AdminUsers)response.Results;

                    var tokenHandler = new JwtSecurityTokenHandler();

                    var key = Encoding.ASCII.GetBytes("stratagile grid adminuser signin jwt hashing secret");

                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                             new Claim(ClaimTypes.Name, adminuser.AdminUserID.ToString())
                        }),
                        Expires = DateTime.UtcNow.AddDays(7), //  need to check with business needs
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };

                    var token = tokenHandler.CreateToken(tokenDescriptor);

                    var tokenString = tokenHandler.WriteToken(token);

                    DatabaseResponse tokenResponse = new DatabaseResponse();

                    tokenResponse = await _AdminUsersDataAccess.LogAdminUserToken(adminuser.AdminUserID, tokenString);

                    // return basic user info (without password) and token to store client side
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = true,
                        Message = EnumExtensions.GetDescription(DbReturnValue.AuthSuccess),
                        ReturnedObject = new LoggedInPrinciple
                        {
                            AdminUser = adminuser,
                            IsAuthenticated = true,
                            Token = tokenString
                        }
                    }
                    );
                }

                else
                {
                    return Ok(new OperationResponse
                    {
                        HasSucceeded = false,
                        Message = EnumExtensions.GetDescription(DbReturnValue.ReasonUnknown),
                        IsDomainValidationErrors = true
                    });
                }
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
        /// <param name="token"></param>
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
        public async Task<IActionResult> Create([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] RegisterAdminUser adminuser)
        {
            try
            {
                AdminUsersDataAccess _adminUsersDataAccess = new AdminUsersDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _adminUsersDataAccess.AuthenticateAdminUserToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int _AdminUserID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
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


                        DatabaseResponse response = await _adminUsersDataAccess.CreateAdminUser(adminuser, _AdminUserID);


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
        /// This will  get admin user details based on specific id supplied
        /// </summary>
        /// <param name="token"></param>
        /// <returns>get user details with specific id</returns>  
        /// 
        // GET: api/GetAdminUser/1
        [HttpGet]
        public async Task<IActionResult> GetAdminUser([FromHeader(Name = "Grid-Authorization-Token")] string token)
        {
            try
            {
                AdminUsersDataAccess _adminUsersDataAccess = new AdminUsersDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _adminUsersDataAccess.AuthenticateAdminUserToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int _AdminUserID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
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


                        AdminUsers adminusers = await _adminUsersDataAccess.GetAdminUser(_AdminUserID);

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
        /// This will get all admin users
        /// </summary>
        /// <param name="token"></param>
        /// <returns>get all user details</returns> 
        /// 
        // GET: api/GetAdminusers
        [HttpGet]
        public async Task<IActionResult> GetAdminusers([FromHeader(Name = "Grid-Authorization-Token")] string token)
        {
            try
            {
                AdminUsersDataAccess _adminUsersDataAccess = new AdminUsersDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _adminUsersDataAccess.AuthenticateAdminUserToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int _AdminUserID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;

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
        /// This will get all admin roles
        /// </summary>
        /// <param></param>
        /// <returns>get all role details</returns> 
        /// 
        // GET: api/GetAdminusers
        [HttpGet]
        [Route("roles")]
        public async Task<IActionResult> GetAdminRoles()
        {
            try
            {

                AdminUsersDataAccess _adminUsersDataAccess = new AdminUsersDataAccess(_iconfiguration);

                List<Roles> AdminUsersList = new List<Roles>();

                AdminUsersList = await _adminUsersDataAccess.GetAdminRoles();

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

        /// <summary>
        /// This will update admin user profile
        /// </summary>
        /// <param name="token"></param>
        /// <param name="adminuser"></param>
        /// <returns>get all role details</returns> 
        /// 
        [HttpPost]
        [Route("UpdateAdminUser")]
        public async Task<IActionResult> UpdateAdminUser([FromHeader(Name = "Grid-Authorization-Token")] string token, [FromBody] AdminProfile adminuser)
        {
            try
            {
                AdminUsersDataAccess _adminUsersDataAccess = new AdminUsersDataAccess(_iconfiguration);

                DatabaseResponse tokenAuthResponse = await _adminUsersDataAccess.AuthenticateAdminUserToken(token);

                if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                {
                    if (!((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                    {
                        int _AdminUserID = ((AuthTokenResponse)tokenAuthResponse.Results).CustomerID;
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
