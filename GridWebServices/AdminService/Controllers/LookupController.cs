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

namespace AdminService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupController : ControllerBase
    {
        IConfiguration _iconfiguration;

        public LookupController(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }

        [HttpGet("{lookupType}")]
        public async Task<List<Lookup>> GetLookup([FromRoute] string lookupType)
        {
            LookupDataAccess _lookupAccess = new LookupDataAccess(_iconfiguration);

            return await _lookupAccess.GetLookupList(lookupType);         
        }
    }
}
