using CatelogService.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatelogService.DataAccess
{
    public class VASContext : DbContext
    {
        public VASContext(DbContextOptions<VASContext> options) : base(options)
        {
        }

        public DbSet<VAS> VASes { get; set; }
    }
}
