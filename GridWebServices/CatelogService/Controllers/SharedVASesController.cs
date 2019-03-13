﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CatelogService.Models;
using CatelogService.DataAccess;

namespace CatelogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SharedVASesController : ControllerBase
    {
        private readonly VASContext _context;

        public SharedVASesController(VASContext context)
        {
            _context = context;
        }

        // GET: api/SharedVASes
        [HttpGet]
        public IEnumerable<VAS> GetVASes()
        {
            var SharedVASes = _context.VASes
                      .FromSql("Catelog_GetSharedVASListing")
                      .ToList();
            return SharedVASes;
        }

        // GET: api/SharedVASes/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVAS([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var SharedVAS = await _context.VASes.FromSql("Catelog_GetSharedVASListing").FirstAsync(p => p.VASID == id);

            if (SharedVAS == null)
            {
                return NotFound();
            }

            return Ok(SharedVAS);
        }

        // PUT: api/SharedVASes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVAS([FromRoute] int id, [FromBody] VAS vAS)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != vAS.VASID)
            {
                return BadRequest();
            }

            _context.Entry(vAS).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VASExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/SharedVASes
        [HttpPost]
        public async Task<IActionResult> PostVAS([FromBody] VAS vAS)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.VASes.Add(vAS);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVAS", new { id = vAS.VASID }, vAS);
        }

        // DELETE: api/SharedVASes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVAS([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var vAS = await _context.VASes.FindAsync(id);
            if (vAS == null)
            {
                return NotFound();
            }

            _context.VASes.Remove(vAS);
            await _context.SaveChangesAsync();

            return Ok(vAS);
        }

        private bool VASExists(int id)
        {
            return _context.VASes.Any(e => e.VASID == id);
        }
    }
}