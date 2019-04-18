using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class EmailTemplate
    {
        public int EmailTemplateID { get; set; }
        public string TemplateName { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
    }
}
