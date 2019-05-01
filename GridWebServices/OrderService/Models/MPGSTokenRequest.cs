using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Models
{   

    public class Customer
    {
        public string browser { get; set; }
        public string ipAddress { get; set; }
    }

    public class Session
    {
        public string id { get; set; }
        public string updateStatus { get; set; }
        public string version { get; set; }
    }

    public class Expiry
    {
        public string month { get; set; }
        public string year { get; set; }
    }

    public class Card
    {
        public string brand { get; set; }
        public Expiry expiry { get; set; }
        public string fundingMethod { get; set; }
        public string number { get; set; }
        public string scheme { get; set; }
        public string securityCode { get; set; }
        public string issuer { get; set; }
    }

    public class Provided
    {
        public Card card { get; set; }
    }

    public class SourceOfFunds
    {
        public Provided provided { get; set; }
        public string type { get; set; }
    }

    public class MPGSResponse
    {
        public Customer customer { get; set; }
        public string merchant { get; set; }
        public Session session { get; set; }
        public SourceOfFunds sourceOfFunds { get; set; }
        public string status { get; set; }
        public string version { get; set; }
    }

    public class CreateTokenResponse
    {      
        public string MPGSOrderID { get; set; }

        public string TransactionID { get; set; }

        public MPGSResponse MPGSResponse { get; set; }

    }

    public class CreateTokenUpdatedDetails
    {
        public string MPGSOrderID { get; set; }

        public string TransactionID { get; set; }

        public string SessionID { get; set; }

        public int OrderID { get; set; }
        public int CustomerID { get; set; }        

        public double Amount { get; set; }

    }
}
