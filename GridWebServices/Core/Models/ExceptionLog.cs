﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class ExceptionLog
    {
        public string ExceptionLogId { get; set; }
        public string ExceptionType { get; set; }       

        public int ExceptionLineNumber { get; set; }

        public int ExceptionColumnNumber { get; set; }

        public string ExceptionMessage { get; set; }

        public string ExceptionFileName { get; set; }

        public string ExceptionMethodName { get; set; }

        public string ExceptionInnerException { get; set; }

        public string ExceptionSeverity { get; set; }

        public string ExceptionStackTrace { get; set; }
        
    }

    public class UILog
    {      

        public string LogType { get; set; } 
     
        public string LogMessage { get; set; }

        public string LogSourceFileName { get; set; }

        public string LogSourcenMethodName { get; set; }

        public string LogSourcenUrl { get; set; }

        public string Message { get; set; }



    }
}
