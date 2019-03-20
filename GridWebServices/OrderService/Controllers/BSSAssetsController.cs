using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using OrderService.DataAccess;
using Core.Models;
using Core.Enums;
using Core.Extensions;
using InfrastructureService;
using Core.Helpers;


namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BSSAssetsController : ControllerBase
    {
        IConfiguration _iconfiguration;

        public BSSAssetsController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }


        [HttpGet]
        public async Task<IActionResult> GetAssets()
        {
            try
            {
                BSSAPIHelper helper = new BSSAPIHelper();

                ResponseObject response = await helper.GetAssetInventory();

                return Ok(new OperationResponse
                {
                    HasSucceeded = true,                   
                    IsDomainValidationErrors = false,
                    ReturnedObject= response
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