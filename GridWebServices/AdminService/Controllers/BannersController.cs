using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminService.Models;
using AdminService.DataAccess;
using Microsoft.Extensions.Configuration;
using Core.Models;



namespace CatelogService.Controllers
{ 
   
    [Route("api/[controller]")]
    [ApiController]    
    public class BannersController : ControllerBase
    {
        IConfiguration _iconfiguration;
       
        public BannersController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }

        /// <summary>
        /// Get banner details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("BannerDetails")]
        public async Task<IActionResult> BannerDetails([FromBody] BannerDetailsRequest request)
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

                BannerDataAccess _bannerAccess = new BannerDataAccess(_iconfiguration);

                return Ok(new ServerResponse
                {
                    HasSucceeded = true,
                    Message = StatusMessages.SuccessMessage,
                    Result = await _bannerAccess.GetBannerDetails(request)

                });
            }
            catch(Exception ex)
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
    }
}