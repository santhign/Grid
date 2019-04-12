using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using Core.Enums;
using System.Linq;
using NeverBounce;
using NeverBounce.Models;

namespace Core.Helpers
{
    public class EmailValidationHelper
    {
        List<RequestParam> paramList = new List<RequestParam>();
        public async Task<string> EmailValidation(EmailConfig config)
        {
            SingleRequestModel _model = new SingleRequestModel();
            _model.email = config.Email;
            NeverBounceSdk _neverbounce = new NeverBounceSdk(config.key);
            SingleResponseModel _response = await _neverbounce.Single.Check(_model);
            return _response.result;
        }

        private Uri GetRequestUrl(string url, ref ApiClient client)
        {
            return client.CreateRequestUri(
                string.Format(System.Globalization.CultureInfo.InvariantCulture, url)
                );
        }

    }
}
