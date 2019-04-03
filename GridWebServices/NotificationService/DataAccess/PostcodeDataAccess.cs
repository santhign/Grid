using Core.Enums;
using Core.Helpers;
using Core.Models;
using InfrastructureService;
using Microsoft.Extensions.Configuration;
using NotificationService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService.DataAccess
{
    public class PostcodeDataAccess
    {

        internal DataAccessHelper _DataHelper = null;

        private IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration"></param>
        public PostcodeDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ResponseObject> ValidatePostcode(Postcode _postcodedata)
        { 
            try
            {

                DatabaseResponse configResponse = ConfigHelper.GetValue("Postcode", _configuration);

                List<Dictionary<string, string>> _result = ((List<Dictionary<string, string>>)configResponse.Results);

                _postcodedata.APIKey = _result.Single(x => x["key"] == "PostcodeApiKey").Select(x => x.Value).ToString();
                _postcodedata.APISecret = _result.Single(x => x["key"] == "PostcodeSecret").Select(x => x.Value).ToString();
                string Postcodeurl = _result.Single(x => x["key"] == "Postcodeurl").Select(x => x.Value).ToString();

                _postcodedata.PostData = string.Format("APIKey={0}&APISecret={1}&Postcode={2}", _postcodedata.APIKey, _postcodedata.APISecret, _postcodedata.PostcodeNumber);
                 
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                ApiClient client = new ApiClient(new Uri(Postcodeurl));
                return await client.PostAsync<ResponseObject,Postcode >(new Uri(Postcodeurl), _postcodedata);

            }
            catch (Exception ex)
            {
                LogInfo.Error(new ExceptionHelper().GetLogString(ex, ErrorLevel.Critical));

                throw ex;
            }
            finally
            {
                _DataHelper.Dispose();
            }

        }
    }
}
