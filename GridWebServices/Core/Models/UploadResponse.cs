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

    public class DownloadResponse
    {
        public bool HasSucceed { get; set; }        

        public byte[] FileObject { get; set; }

        public string Message { get; set; }
    }

    public class DownloadNRIC
    {       
        public byte[] FrontImage { get; set; }

        public byte[] BackImage { get; set; }
    }

}
