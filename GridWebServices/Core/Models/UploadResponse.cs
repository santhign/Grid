using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class UploadResponse
    {
        public bool HasSucceed { get; set; }

        public string FileName { get; set; }

        public string Message { get; set; }
    }
}
