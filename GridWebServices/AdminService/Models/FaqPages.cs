using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminService.Models
{
    public class FaqPages
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string FAQCategory { get; set; }
        public int SortOrder { get; set; } 
    }

    public class FaqPageRequest
    {
        public string PageName { get; set; }
    }
}
