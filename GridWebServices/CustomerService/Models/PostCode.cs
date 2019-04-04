using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace CustomerService.Models
{
    public class PostCode
    {
    }
    [DataContract]
    public class PostCodeRequest
    {
        [DataMember(Name = "APIKey")] //
        public string APIKey { get; set; }

        [DataMember(Name = "APISecret")]
        public string APISecret { get; set; }

        [DataMember(Name = "Postcode")]
        public string Postcode { get; set; }
    }
}
