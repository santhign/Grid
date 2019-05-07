using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Core.Models
{
    public class ResultParam
    {
        public string resultCode { get; set; }
        public string resultDescription { get; set; }
    }

   
    public class AccountDetails
    {
        public List<RequestParam> param { get; set; }
    }

    public class DataSetAccountDetails
    {
        public ResultParam resultParam { get; set; }
        public AccountDetails accountDetails { get; set; }
    }

    public class Response
    {
        public string request_id { get; set; }
        public string request_timestamp { get; set; }
        public string source_node { get; set; }
        public string action { get; set; }
        public string response_id { get; set; }
        public string response_timestamp { get; set; }
        public string result_code { get; set; }
        public string result_desc { get; set; }
        public DataSetAccountDetails dataSet { get; set; }
    }

    public class BSSAccountQuerySubscriberResponse
    {
        public Response Response { get; set; }
    }
}
