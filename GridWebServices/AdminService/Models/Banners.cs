using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AdminService.Models
{
    public class Banners
    {
        [Key]
        public int BannerID { get; set; }
        public int LocationID { get; set; }
        public string BannerName { get; set; }
        public string BannerUrl { get; set; }
        public int UrlType { get; set; }
        public string BannerImage { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int Status { get; set; }       
    }
}
