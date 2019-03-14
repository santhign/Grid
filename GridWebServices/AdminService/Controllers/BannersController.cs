using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminService.Models;
using AdminService.DataAccess;

namespace CatelogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BannersController : ControllerBase
    {
        private readonly AdminContext _context;

        public BannersController(AdminContext context)
        {
            _context = context;
        }

        // POST: api/Banners
       
        [Route("BannerDetails")]
        public async Task<List<BannerDetails>> BannerDetails([FromBody] BannerDetailsRequest request)
        {

            var banners = await _context.Banners
                      .FromSql($"Admin_GetBannerDetails {request.LocationName}, {request.PageName} ").ToListAsync();
            
            return banners.Select(x => new BannerDetails { BannerImage = x.BannerImage, BannerUrl = x.BannerUrl, UrlType = x.UrlType == 0 ? "_self" : "_blank" }).ToList();
        }

        // GET: api/Banners/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBanner([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var banners = await _context.Banners.FromSql("Admin_GetBannerDetails").FirstAsync(p => p.BannerID == id);

            if (banners == null)
            {
                return NotFound();
            }

            return Ok(banners);
        }

      

        // GET: api/Banners/5
        [HttpGet("{id}/{promocode}")]
        public async Task<IActionResult> GetBanner([FromRoute] int id, string promocode)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var banner = await _context.Banners
                      .FromSql($"Catelog_GetPromotionalBundle {id}, {promocode}").FirstAsync();

            if (banner == null)
            {
                return NotFound();
            }

            return Ok(banner);
        }

        // PUT: api/Banners/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PuBanner([FromRoute] int id, [FromBody] Banners banner)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != banner.BannerID)
            {
                return BadRequest();
            }

            _context.Entry(banner).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BannerExists(id))
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

        // POST: api/Banners
        [HttpPost]
        public async Task<IActionResult> PostBanner([FromBody] Banners banner)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Banners.Add(banner);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBanner", new { id = banner.BannerID }, banner);
        }

        // DELETE: api/Banners/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBanner([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var banner = await _context.Banners.FindAsync(id);

            if (banner == null)
            {
                return NotFound();
            }

            _context.Banners.Remove(banner);

            await _context.SaveChangesAsync();

            return Ok(banner);
        }

        private bool BannerExists(int id)
        {
            return _context.Banners.Any(e => e.BannerID == id);
        }
    }
}