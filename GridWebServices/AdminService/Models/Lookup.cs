using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AdminService.Models
{
    public class Lookup
    {
        [Key]
        public int LookupID { get; set; }
        public string LookupText { get; set; }
    }
}
