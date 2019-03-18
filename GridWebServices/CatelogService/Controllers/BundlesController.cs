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
                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage,
                    Result = await _bundleAccess.GetBundleList()

                });
              
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

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage,
                    Result = (await _bundleAccess.GetBundleById(id)).FirstOrDefault()

                });
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

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage,
                    Result = (await _bundleAccess.GetBundleByPromocode(id,promocode)).FirstOrDefault()

                });
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
        /// To update details of a promotion bundle specified by the bundleID in database. 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bundle"></param>
        /// <returns>Bundle</returns>
        // PUT: api/Bundles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBundle([FromRoute] int id, [FromBody] UpdateBundleRequest bundle)
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

                DatabaseResponse dbResponse = await _bundleAccess.UpdateBundle(bundle);

                return Ok(new ServerResponse
                {
                    HasSucceeded = dbResponse.ResponseCode == 1?true: false,
                    Message =  EnumExtensions.GetDescription(dbResponse.ResponseCode == 1 ?DbReturnValue.UpdateSuccess:(DbReturnValue) dbResponse.ResponseCode),
                    Result = dbResponse.Results
                    
                });
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
        ///  To create a promotion bundle in database. 
        /// </summary>
        /// <param name="bundle"></param>
        /// <returns>Bundle</returns>

        // POST: api/Bundles
        [HttpPost]
        public async Task<IActionResult> PostBundle([FromBody] CreateBundleRequest bundle)
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

                DatabaseResponse dbResponse  = await _bundleAccess.CreateBundle(bundle);

                return Ok(new ServerResponse
                {
                    HasSucceeded = (dbResponse.ResponseCode==(int) DbReturnValue.CreateSuccess),
                    Message = EnumExtensions.GetDescription((DbReturnValue)dbResponse.ResponseCode),
                    Result = dbResponse.Results

                });
            }

            catch (Exception ex)
            {
                //to do Logging

                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });
            }           
        }

        /// <summary>
        ///  To delete a promotion bundle specified by the id from database. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Bundle</returns>

        // DELETE: api/Bundles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBundle([FromRoute] int id)
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

                int result = await _bundleAccess.DeleteBundle(id);

                return Ok(new OperationResponse
                {
                    HasSucceeded = (result == ((int) DbReturnValue.DeleteSuccess)),

                    Message = EnumExtensions.GetDescription((DbReturnValue) result),                  

                });
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
        /// To check a promotion bundle specified by  id exists in database
        /// </summary>
        /// <param name="id"></param>
        /// <returns>bool</returns>
        private async Task<bool> BundleExists(int id)
        {  
            try
            {
                BundleDataAccess _bundleAccess = new BundleDataAccess(_iconfiguration);

                return (await _bundleAccess.BundleExists(id) == ((int)DbReturnValue.DeleteSuccess));
            }

            catch(Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return false;
            }
            
        }
    }
}