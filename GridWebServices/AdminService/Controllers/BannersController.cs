using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminService.Models;
using AdminService.DataAccess;
using Microsoft.Extensions.Configuration;

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
        public async Task<List<Banners>> BannerDetails([FromBody] BannerDetailsRequest request)
        {
            BannerDataAccess _bannerAccess = new BannerDataAccess(_iconfiguration);

            return  await _bannerAccess.GetBannerDetails(request);           
        }
    }
}