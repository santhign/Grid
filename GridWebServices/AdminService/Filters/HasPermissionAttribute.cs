﻿using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System.IO;
using AdminService.DataAccess;
using Core.Models;
using System.Collections.Generic;
using Core.Enums;
using InfrastructureService;
using Core.Helpers;

namespace AdminService.Filters
{ 
    public class HasPermissionAttribute : ActionFilterAttribute
    {
        private string _permission;

        private static IConfiguration _configuration { get; } = new ConfigurationBuilder()
         .SetBasePath(Directory.GetCurrentDirectory())
         .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
         .Build();

        public HasPermissionAttribute(string permission)
        {
            this._permission = permission;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                Microsoft.Extensions.Primitives.StringValues adminToken;

                if (filterContext.HttpContext.Request.Headers.ContainsKey("Grid-Authorization-Token"))
                {
                    filterContext.HttpContext.Request.Headers.TryGetValue("Grid-Authorization-Token", out adminToken);
                }

                if(string.IsNullOrEmpty(adminToken))
                {
                    filterContext.Result = new StatusCodeResult(401);

                    filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary {{ "Controller", "Redirect" },
                                      { "Action", "TokenEmpty" } });
                }
                else
                {
                    // include token validation scenario - expiry
                    string per = _permission;

                    AdminUsersDataAccess _adminAccess = new AdminUsersDataAccess(_configuration);

                    DatabaseResponse tokenAuthResponse =   _adminAccess.AuthenticateAdminUserTokenPermission(adminToken);

                    if (tokenAuthResponse.ResponseCode == (int)DbReturnValue.AuthSuccess)
                    {
                        if (((AuthTokenResponse)tokenAuthResponse.Results).IsExpired)
                        {
                            filterContext.Result = new StatusCodeResult(401);

                            filterContext.Result = new RedirectToRouteResult(
                            new RouteValueDictionary {{ "Controller", "Redirect" },
                                      { "Action", "TokenExpired" } });
                        }
                    }
                    else
                    {
                        filterContext.Result = new StatusCodeResult(401);

                        filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary {{ "Controller", "Redirect" },
                                      { "Action", "InvalidToken" } });
                    }

                    DatabaseResponse permissionResponse = _adminAccess.GetAdminUserPermissionsByToken(adminToken);

                    if (permissionResponse != null && permissionResponse.ResponseCode == (int)DbReturnValue.RecordExists)
                    {
                        List<string> permissions = (List<string>)permissionResponse.Results;

                        if (!permissions.Contains(_permission))
                        {
                            filterContext.Result = new StatusCodeResult(403);

                            filterContext.Result = new RedirectToRouteResult(
                            new RouteValueDictionary {{ "Controller", "Redirect" },
                                      { "Action", "Forbidden" } });
                        }
                    }
                    else if (permissionResponse.ResponseCode == (int)DbReturnValue.NotExists)
                    {
                        filterContext.Result = new StatusCodeResult(403);

                        filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary {{ "Controller", "Redirect" },
                                      { "Action", "Forbidden" } });
                    }
                }              
              
            }
            catch(Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
            }            
        }
    }
}
