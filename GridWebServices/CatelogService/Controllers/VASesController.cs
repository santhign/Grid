using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CatelogService.DataAccess;
using Microsoft.Extensions.Configuration;
using Core.Models;
using Core.Enums;
using Core.Helpers;
using Serilog;

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
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));
                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });
            }
        }

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
                Log.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                return Ok(new OperationResponse
                {
                    HasSucceeded = false,
                    Message = StatusMessages.ServerError,
                    IsDomainValidationErrors = false
                });
            }

        }

        //// PUT: api/VASes/5
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutVAS([FromRoute] int id, [FromBody] VAS vAS)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (id != vAS.VASID)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(vAS).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!VASExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/VASes
        //[HttpPost]
        //public async Task<IActionResult> PostVAS([FromBody] VAS vAS)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    _context.VASes.Add(vAS);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetVAS", new { id = vAS.VASID }, vAS);
        //}

        // DELETE: api/VASes/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteVAS([FromRoute] int id)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var vAS = await _context.VASes.FindAsync(id);
        //    if (vAS == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.VASes.Remove(vAS);
        //    await _context.SaveChangesAsync();

        //    return Ok(vAS);
        //}

        //private bool VASExists(int id)
        //{
        //    return _context.VASes.Any(e => e.VASID == id);
        //}
    }
}