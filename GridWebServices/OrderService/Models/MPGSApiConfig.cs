using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace OrderService.Models
{  
        public class GatewayApiConfig
        {
            public GatewayApiConfig(GridMPGSConfig mPGSConfig)
            {
            Version = mPGSConfig.Version;
            GatewayUrl = mPGSConfig.GatewayUrl;
            GatewayUrlCertificate =  mPGSConfig.GatewayUrlCertificate;
            Currency =  mPGSConfig.Currency;
            MerchantId =  mPGSConfig.MerchantId;
            GridMerchantName = mPGSConfig.GridMerchantName;
            GridMerchantPostCode = mPGSConfig.GridMerchantPostCode;
            GridMerchantContactNumber = mPGSConfig.GridMerchantContactNumber;
            GridMerchantAddress1 = mPGSConfig.GridMerchantAddress1;
            GridMerchantAddress2 = mPGSConfig.GridMerchantAddress2;
            Password = mPGSConfig.Password;
            Username = "merchant." + mPGSConfig.MerchantId; ;
            CertificateLocation = mPGSConfig.CertificateLocation;
            CertificatePassword = mPGSConfig.CertificatePassword;
            AuthenticationByCertificate = mPGSConfig.AuthenticationByCertificate;
            WebhooksNotificationSecret = mPGSConfig.WebhooksNotificationSecret;

            }

            public static string WEBHOOKS_NOTIFICATION_FOLDER = "webhooks-notifications";

            public Boolean Debug { get; set; }

            public Boolean UseSsl { get; set; }
            public Boolean IgnoreSslErrors { get; set; }

        //proxy configuration
        public Boolean UseProxy { get; set; }
        public String ProxyHost { get; set; }
        public String ProxyUser { get; set; }
        public String ProxyPassword { get; set; }
        public String ProxyDomain { get; set; }
        
        public string Version { get; set; }  
            public string GatewayUrl { get; set; } 
            public string GatewayUrlCertificate { get; set; }
            public string Currency { get; set; }
            public string MerchantId { get; set; }
            public string Password { get; set; }
            public string Username { get; set; }
            public string CertificateLocation { get; set; }
            public string CertificatePassword { get; set; }
            public bool AuthenticationByCertificate { get; set; }
            public string WebhooksNotificationSecret { get; set; }
            public string GridMerchantName { get; set; }        
            public string GridMerchantPostCode { get; set; }       
            public string GridMerchantAddress1 { get; set; }       
            public string GridMerchantAddress2 { get; set; }       
            public string GridMerchantContactNumber { get; set; }
    }

    [DataContract]
    public class GridMPGSConfig
    {
        [DataMember(Name = "Version")]
        public string Version { get; set; }

        [DataMember(Name = "GatewayUrl")]
        public string GatewayUrl { get; set; }

        [DataMember(Name = "GatewayUrlCertificate")]
        public string GatewayUrlCertificate { get; set; }

        [DataMember(Name = "Currency")]
        public string Currency { get; set; }

        [DataMember(Name = "MerchantId")]
        public string MerchantId { get; set; }

        [DataMember(Name = "Password")]
        public string Password { get; set; }

        [DataMember(Name = "Username")]
        public string Username { get; set; }

        [DataMember(Name = "GridMerchantName")]
        public string GridMerchantName { get; set; }

        [DataMember(Name = "GridMerchantPostCode")]
        public string GridMerchantPostCode { get; set; }

        [DataMember(Name = "GridMerchantAddress1")]
        public string GridMerchantAddress1 { get; set; }

        [DataMember(Name = "GridMerchantAddress2")]
        public string GridMerchantAddress2 { get; set; }

        [DataMember(Name = "GridMerchantContactNumber")]
        public string GridMerchantContactNumber { get; set; }        

        [DataMember(Name = "CertificateLocation")]
        public string CertificateLocation { get; set; }

        [DataMember(Name = "CertificatePassword")]
        public string CertificatePassword { get; set; }

        [DataMember(Name = "AuthenticationByCertificate")]
        public bool AuthenticationByCertificate { get; set; }

        [DataMember(Name = "WebhooksNotificationSecret")]
        public string WebhooksNotificationSecret { get; set; }
    }
}
