using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminService.Models;
using AdminService.DataAccess;

namespace AdminService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupController : ControllerBase
    {
        private readonly AdminContext _context;

        public LookupController(AdminContext context)
        {
            _context = context;
        }

       [HttpGet]
        public async Task<List<Lookup>> GetLookup([FromRoute] int id, string lookupType)
        {
            return await _context.Lookup
                      .FromSql($"[Admin_GetLookup] @LookupType={lookupType} ").ToListAsync();            
        }
    }
}
