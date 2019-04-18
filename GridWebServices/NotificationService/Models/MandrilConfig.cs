using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService.Models
{
    public class MandrilConfig
    {
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string MandrillKey { get; set; }
        public string ReplyEmail { get; set; }
    }
}
