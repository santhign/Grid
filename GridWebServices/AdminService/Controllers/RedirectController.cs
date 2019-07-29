using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Enums;
using Core.Models;
using Core.Extensions;
using Microsoft.Extensions.Configuration;

namespace AdminService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedirectController : ControllerBase
    {   

        private readonly IConfiguration _iconfiguration;

        public RedirectController(IConfiguration configuration)
        {
            _iconfiguration = configuration;           
        }

        [HttpGet]
        public async Task<IActionResult> Forbidden()
        {
            return await Task.Run(() =>
            {
                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = EnumExtensions.GetDescription(CommonErrors.Forbidden),
                    IsDomainValidationErrors = true,
                    StatusCode= ((int) CommonErrors.Forbidden).ToString()
                });

            });           
               
        }     
    }
}