using System;
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

                //include token empty scenario
                // include token validation scenario - expiry
                string per = _permission;

                AdminUsersDataAccess _adminAccess = new AdminUsersDataAccess(_configuration);

                DatabaseResponse permissionResponse =  _adminAccess.GetAdminUserPermissionsByToken(adminToken);

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
                // include permissions list null scenario
            }
            catch(Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
            }            
        }
    }
}
