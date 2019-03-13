using System;
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
    public class BundlesController : ControllerBase
    {
        private readonly BundleContext _context;

        public BundlesController(BundleContext context)
        {
            _context = context;
        }

        // GET: api/Bundles
        [HttpGet]
        public IEnumerable<Bundle> GetBundles()
        {
            var bundles = _context.Bundles
                      .FromSql("Catelog_GetBundlesListing")
                      .ToList();
            return bundles;
        }

        // GET: api/Bundles/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBundle([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var bundle = await _context.Bundles.FromSql("Catelog_GetBundlesListing").FirstAsync(p => p.BundleID == id);

            if (bundle == null)
            {
                return NotFound();
            }

            return Ok(bundle);
        }

        // GET: api/Bundles/5
        [HttpGet("{id}/{promocode}")]
        public async Task<IActionResult> GetBundle([FromRoute] int id, string promocode)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bundle = await _context.Bundles
                      .FromSql($"Catelog_GetPromotionalBundle {id}, {promocode}").FirstAsync();

            if (bundle == null)
            {
                return NotFound();
            }

            return Ok(bundle);
        }

        // PUT: api/Bundles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBundle([FromRoute] int id, [FromBody] Bundle bundle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != bundle.BundleID)
            {
                return BadRequest();
            }

            _context.Entry(bundle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BundleExists(id))
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

        // POST: api/Bundles
        [HttpPost]
        public async Task<IActionResult> PostBundle([FromBody] Bundle bundle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Bundles.Add(bundle);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBundle", new { id = bundle.BundleID }, bundle);
        }

        // DELETE: api/Bundles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBundle([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bundle = await _context.Bundles.FindAsync(id);
            if (bundle == null)
            {
                return NotFound();
            }

            _context.Bundles.Remove(bundle);
            await _context.SaveChangesAsync();

            return Ok(bundle);
        }

        private bool BundleExists(int id)
        {
            return _context.Bundles.Any(e => e.BundleID == id);
        }
    }
}