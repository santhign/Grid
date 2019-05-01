using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrderService.Models
{
    public class TokenResponse
    {
        public string GatewayCode { get; set; }
        public string Result { get; set; }
        public string Status { get; set; }
        public string Token { get; set; }
        public string Type { get; set; }
        public Card Card { get; set; }
        /// <summary>
        /// Convert response JSON to TokenResponse object
        /// </summary>
        /// <returns>The Token response.</returns>
        /// <param name="response">Response.</param>
        public static TokenResponse ToTokenResponse(string response)
        {
            var model = new TokenResponse();

            JObject jObject = JObject.Parse(response);
            model.Result = jObject["result"] != null ? jObject["result"].ToString() : null;
            if (model.Result != "ERROR")
            {
                model.GatewayCode = jObject["response"]["gatewayCode"] != null ? jObject["response"]["gatewayCode"].ToString() : null;

                model.Status = jObject["status"] != null ? jObject["status"].ToString() : null;
                model.Token = jObject["token"] != null ? jObject["token"].ToString() : null;
                model.Type = jObject["sourceOfFunds"]["type"] != null ? jObject["sourceOfFunds"]["type"].ToString() : null;
                model.Card = jObject["sourceOfFunds"]["provided"]["card"] != null ?

                  new Card
                  {
                      brand = jObject["sourceOfFunds"]["provided"]["card"]["brand"].ToString(),
                      number = jObject["sourceOfFunds"]["provided"]["card"]["number"].ToString(),
                      scheme = jObject["sourceOfFunds"]["provided"]["card"]["scheme"].ToString(),
                      fundingMethod = jObject["sourceOfFunds"]["provided"]["card"]["fundingMethod"].ToString(),
                      expiry = new Expiry { month = jObject["sourceOfFunds"]["provided"]["card"]["expiry"].ToString().Substring(0, 2), year = jObject["sourceOfFunds"]["provided"]["card"]["expiry"].ToString().Substring(2, 2) }
                  }

                    : null;
            }
            return model;
        }

        public static string GetResponseResult(string response)
        {
            JObject jObject = JObject.Parse(response);
            return jObject["result"] != null ? jObject["result"].ToString() : null;
        }
    }

   
}
