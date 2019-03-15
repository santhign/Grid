using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AdminService.Models
{
    public class Banners
    {
        public string BannerImage { get; set; }
        public string BannerUrl { get; set; }
        public string UrlType { get; set; }
    }

    public class BannerDetailsRequest
    {
        public string LocationName { get; set; }
        public string PageName { get; set; }


    }
}
