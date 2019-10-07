using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using Core.Enums;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Serilog;

namespace Core.Helpers
{
    public class GridAPIHelper
    {
       
        public async Task<GridOutstanding> GetOutstanding(string gridBillingAPIEndpoint, string billingAccountNumber )
        {
            try
            {
                ApiClient client = new ApiClient();             

                return  await client.GetAsync<GridOutstanding>(new Uri(gridBillingAPIEndpoint + "balance/" + billingAccountNumber));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<CustomerBillHistory> GetBillingHistory(string gridBillingAPIEndpoint, string billingAccountNumber)
        {
            try
            {
                ApiClient client = new ApiClient();

                return await client.GetAsync<CustomerBillHistory>(new Uri(gridBillingAPIEndpoint + "history/" + billingAccountNumber));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
