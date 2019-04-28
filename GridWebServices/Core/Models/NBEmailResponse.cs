using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class NBEmailResponse
    {
        public string Result { get; set; }
        public string Status { get; set; }
        public string suggested_correction { get; set; }
    }
}
