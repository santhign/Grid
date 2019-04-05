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
        public async Task<IActionResult> GetBundles()
        {
            try
            {
                BundleDataAccess _bundleAccess = new BundleDataAccess(_iconfiguration);
                List<Bundle> returnObj = await _bundleAccess.GetBundleList();
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
        /// <param name="id">Bundle ID</param>
        /// <returns>Bundle</returns>
        // GET: api/Bundles/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBundle([FromRoute] int id)
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

                BundleDataAccess _bundleAccess = new BundleDataAccess(_iconfiguration);
                List<Bundle> returnObj = await _bundleAccess.GetBundleById(id);
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
        /// Promotion Bundle for a bundle id and promotion code. Retuns the new bundle for the id passed which is matching the promotion code.
        /// </summary>
        /// <param name="id">Bundle ID</param>
        /// <param name="promocode">Promotion code</param>
        /// <returns>Bundle</returns>
        // GET: api/Bundles/5
        [HttpGet("{id}/{promocode}")]
        public async Task<IActionResult> GetBundle([FromRoute] int id, string promocode)
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
    }
}