using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService.Models
{
    public class Postcode
    {

        public string APIKey { get; set; }

        public string APISecret { get; set; }

        public string PostcodeNumber { get; set; }

        public string PostData { get; set; }
    }
}
