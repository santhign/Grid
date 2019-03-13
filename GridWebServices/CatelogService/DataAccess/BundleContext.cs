using CatelogService.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatelogService.DataAccess
{
    public class BundleContext : DbContext
    {
        public BundleContext(DbContextOptions<BundleContext> options) : base(options)
        {
        }

        public DbSet<Bundle> Bundles { get; set; }
    }
}
