using System.Linq;
using System;
using System.Collections.Generic;
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

                OrderDataAccess _orderAccess = new OrderDataAccess(_iconfiguration);

                DatabaseResponse configResponse = await _orderAccess.GetConfiguration(ConfiType.BSS.ToString());

                //GridBSSConfi config = LinqExtensions.GeObjectFromDictionary<GridBSSConfi>((Dictionary<string, string>)configResponse.Results);

                GridBSSConfi config = helper.GetGridConfig((List<Dictionary<string, string>>)configResponse.Results);

                DatabaseResponse serviceCAF= await _orderAccess.GetBSSServiceCategoryAndFee(ServiceTypes.Free.ToString());

                DatabaseResponse requestIdRes= await _orderAccess.GetBssApiRequestId(GridMicroservices.Customer.ToString(), BSSApis.GetAssets.ToString(),30); // need to pass customer_id here
                 
                ResponseObject res= await  helper.GetAssetInventory(config, ((ServiceFees) serviceCAF.Results).ServiceCode, ((BSSAssetRequest)requestIdRes.Results).request_id);

                return Ok(new OperationResponse
                {
                    HasSucceeded = true,                   
                    IsDomainValidationErrors = false,
                    ReturnedObject= res
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