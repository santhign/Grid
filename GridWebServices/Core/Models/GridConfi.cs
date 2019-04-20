using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;


namespace Core.Models
{
    [DataContract]
   
    public class GridBSSConfi
    {
       
        [DataMember(Name = "BSSAPIUrl")] 
        public string BSSAPIUrl { get; set; }

        [DataMember(Name = "GridDefaultAssetLimit")]
        public int GridDefaultAssetLimit { get; set; }

        [DataMember(Name = "GridEntityId")]
        public int GridEntityId { get; set; }

        [DataMember(Name = "GridProductId")]
        public int GridProductId { get; set; }

        [DataMember(Name = "GridId")]
        public int GridId { get; set; }

        [DataMember(Name = "GridSourceNode")]
        public string GridSourceNode { get; set; }

        [DataMember(Name = "GridUserName")]
        public string GridUserName { get; set; }

        [DataMember(Name = "GridDefaultOffset")]
        public int GridDefaultOffset { get; set; }

        [DataMember(Name = "GridDefaultLimit")]
        public int GridDefaultLimit { get; set; }

        [DataMember(Name = "GridInvoiceRecordLimit")]
        public int GridInvoiceRecordLimit { get; set; }

        [DataMember(Name = "BSSAPILocalUrl")]
        public string BSSAPILocalUrl { get; set; }

        
    }

    [DataContract]
    public class GridSystemConfig
    {
        [DataMember(Name = "DeliveryMarginInDays")]
        public int DeliveryMarginInDays { get; set; }

        [DataMember(Name = "FreeNumberListCount")]
        public int FreeNumberListCount { get; set; }

        [DataMember(Name = "PremiumNumberListCount")]
        public int PremiumNumberListCount { get; set; }

    }

    [DataContract]
    public class GridAWSS3Config
    {
        [DataMember(Name = "AWSAccessKey")]
        public string AWSAccessKey { get; set; }

        [DataMember(Name = "AWSSecretKey")]
        public string AWSSecretKey { get; set; }

        [DataMember(Name = "AWSBucketName")]
        public string AWSBucketName { get; set; }

        [DataMember(Name = "AWSUser")]
        public string AWSUser { get; set; }

        [DataMember(Name = "AWSEndPoint")]
        public string AWSEndPoint { get; set; }
        

    }


    [DataContract]
    public class ForgotPasswordMsgConfig
    {
        [DataMember(Name = "ForgotPasswordSNSTopic")]
        public string ForgotPasswordSNSTopic { get; set; }

        [DataMember(Name = "PasswordResetUrl")]
        public string PasswordResetUrl { get; set; }
       
    }

}
