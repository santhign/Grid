using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Models
{
    public class GatewayApiClient : AbstractGatewayApiClient
    {
        private readonly GatewayApiConfig GatewayApiConfig;

        public GatewayApiClient(GatewayApiConfig gatewayApiConfig)
        {
            GatewayApiConfig = gatewayApiConfig;            
        }

        /// <summary>
        /// Sends the transaction.
        /// </summary>
        /// <returns>The transaction.</returns>
        /// <param name="gatewayApiRequest">Gateway API request.</param>
        public override string SendTransaction(GatewayApiRequest gatewayApiRequest)
        {
            return this.executeHTTPMethod(gatewayApiRequest);
        }

    }
}
