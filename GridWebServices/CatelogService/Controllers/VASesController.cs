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
        /// <returns>VAS</returns>
        // GET: api/VASes
        [HttpGet]
        public async Task<IActionResult> GetVASes()
        {

            try
            {
                VASDataAccess _vasAccess = new VASDataAccess(_iconfiguration);

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage,
                    Result = await _vasAccess.GetVASes()

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
        /// Returns details of a value added service specified by the id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>VAS</returns>
        // GET: api/VASes/5
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

                VASDataAccess _vasAccess = new VASDataAccess(_iconfiguration);

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage,
                    Result = (await _vasAccess.GetVASes()).Where(p => p.VASID == id).FirstOrDefault()

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