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

namespace CatelogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SharedVASesController : ControllerBase
    {
        IConfiguration _iconfiguration;

        public SharedVASesController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }

        /// <summary>
        /// Returns list of shared value added services
        /// </summary>
        /// <returns>VAS List</returns>
        /// 
        // GET: api/SharedVASes
        [HttpGet]
        public async Task<IActionResult> GetVASes()
        {
            try
            {
                SharedVASDataAccess _vasSharedAccess = new SharedVASDataAccess(_iconfiguration);

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage,
                    Result = await _vasSharedAccess.GetVASes()

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
        /// Returns details of a shared value added service specified by the id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>VAS</returns>

        // GET: api/SharedVASes/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVAS([FromRoute] int id)
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

                SharedVASDataAccess _vasSharedAccess = new SharedVASDataAccess(_iconfiguration);

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage,
                    Result = (await _vasSharedAccess.GetVASes()).Where(p => p.VASID == id).FirstOrDefault()

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
    }
}