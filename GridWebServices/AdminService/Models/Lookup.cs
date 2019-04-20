using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AdminService.Models
{
    public class Lookup
    {
        public string LookupText { get; set; }
    }
    public class EmailValidationResponse
    {
        public bool IsValid { get; set; }
        public string Status { get; set; }
    }
}
