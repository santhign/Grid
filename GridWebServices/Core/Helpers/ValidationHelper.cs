using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using Core.Enums;
using System.Linq;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace Core.Helpers
{
    public class EmailValidationHelper
    {
        private static string Weights = "2765432";
        List<RequestParam> paramList = new List<RequestParam>();

        public string EmailValidation(EmailConfig config)
        {
            //SingleRequestModel _model = new SingleRequestModel();
            //_model.email = config.Email;
            //NeverBounceSdk _neverbounce = new NeverBounceSdk(config.key);
            //SingleResponseModel _response = await _neverbounce.Single.Check(_model);
            //return _response.result;
            ApiClient client = new ApiClient(new Uri(config.EmailAPIUrl));

            var requestUrl = GetRequestUrl(config.EmailAPIUrl, ref client);

            RequestObject req = new RequestObject();
            string html = string.Empty;
            string url = requestUrl + $"?key={config.key}&email={config.Email}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }
            NBEmailResponse _resp = JsonConvert.DeserializeObject<NBEmailResponse>(html);
            return _resp.Result;
        }

        private Uri GetRequestUrl(string url, ref ApiClient client)
        {
            return client.CreateRequestUri(
                string.Format(System.Globalization.CultureInfo.InvariantCulture, url)
                );
        }

        public async Task<bool> AllowSubscribers(int CustomerID, int Type, IConfiguration _configuration)
        {
            bool allowed = false;
            DataAccessHelper _DataHelper = null;
            try
            {
                SqlParameter[] parameters =
                    {
                    new SqlParameter("@CustomerID", SqlDbType.Int),
                    new SqlParameter("@Type", SqlDbType.Int)
                    };

                parameters[0].Value = CustomerID;
                parameters[1].Value = Type;
                _DataHelper = new DataAccessHelper("Customers_IsAdditionalSubscriberAllowed", parameters, _configuration);

                int response = await _DataHelper.RunAsync();

                if (response == 140)
                    allowed = true;
                else
                    allowed = false;
            }

            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                _DataHelper.Dispose();
            }
            return allowed;
        }

        public bool NRICValidation(string IDType, string NRIC, out string _warningmsg)
        {
            bool isvalid = false;
            _warningmsg = "";
            try
            {

                // Check any number is passed
                if (NRIC.Equals(string.Empty))
                {
                    _warningmsg = "Please give an NRIC number";
                    throw new Exception(_warningmsg);
                }

                // Check length
                if (NRIC.Length != 9)
                {
                    _warningmsg = "The length of NRIC should be 9";
                    throw new Exception(_warningmsg);
                }

                // Check the file letter
                if (!((NRIC[0].ToString().Equals("S"))
                    || (NRIC[0].ToString().Equals("T"))
                    || (NRIC[0].ToString().Equals("F"))
                    || (NRIC[0].ToString().Equals("G"))))
                {
                    _warningmsg = "First letter of NRIC should be S,T,F or G";
                    throw new Exception(_warningmsg);
                }

                // Check whether the NRIC is a number if first and last char are removed
                int NRIC_Internal_Number = 0;
                if (!int.TryParse(NRIC.Substring(1, 7), out NRIC_Internal_Number))
                {
                    _warningmsg = "NRIC should be a number excluding the first and last characters";
                    throw new Exception(_warningmsg);
                }

                // Check the CheckSumNumber
                if (!IsValidCheckSum(NRIC))
                {
                    _warningmsg = "Invalid NRIC checksum";
                    throw new Exception(_warningmsg);
                }
                if (IDType == "S" && (NRIC[0].ToString().Equals("S") || NRIC[0].ToString().Equals("T")))
                {
                    isvalid = true;
                    _warningmsg = "";
                }
                else if (IDType == "F" && (NRIC[0].ToString().Equals("F") || NRIC[0].ToString().Equals("G")))
                {
                    isvalid = true;
                    _warningmsg = "";
                }
                else if (IDType == null)
                {
                    isvalid = true;
                    _warningmsg = "";
                }
                else
                {
                    isvalid = false;
                    _warningmsg = StatusMessages.InvalidMessage;
                }
            }
            catch (Exception)
            {
                isvalid = false;
            }
            return isvalid;
        }
        private bool IsValidCheckSum(string NRIC)
        {
            string NRIC_Internal_Numbers = NRIC.Substring(1, 7);
            int CheckSum = 0;


            // Calcualte check sum
            for (int i = 0; i < 7; i++)
            {
                int Weight = Convert.ToInt32(Weights[i].ToString());
                int NRIC_Internal_Number = Convert.ToInt32(NRIC_Internal_Numbers[i].ToString());
                CheckSum += (Weight * NRIC_Internal_Number);
            }
            CheckSum = CheckSum % 11;

            // Get the series checksum letter
            Dictionary<int, string> Series = GetSeries(NRIC.Substring(0, 1));
            string ChecksumLetter = Series[CheckSum];

            // Check if the last char or NRIC and check sum is equal
            if (ChecksumLetter.Equals(NRIC[8].ToString()))
            {
                return true;
            }

            return false;
        }

        private Dictionary<int, string> GetSeries(string SeriesLetter)
        {
            Dictionary<int, string> Series = new Dictionary<int, string>();

            if (SeriesLetter.Equals("S"))
            {
                Series.Add(10, "A");
                Series.Add(9, "B");
                Series.Add(8, "C");
                Series.Add(7, "D");
                Series.Add(6, "E");
                Series.Add(5, "F");
                Series.Add(4, "G");
                Series.Add(3, "H");
                Series.Add(2, "I");
                Series.Add(1, "Z");
                Series.Add(0, "J");
            }
            else if (SeriesLetter.Equals("T"))
            {
                Series.Add(10, "H");
                Series.Add(9, "I");
                Series.Add(8, "Z");
                Series.Add(7, "J");
                Series.Add(6, "A");
                Series.Add(5, "B");
                Series.Add(4, "C");
                Series.Add(3, "D");
                Series.Add(2, "E");
                Series.Add(1, "F");
                Series.Add(0, "G");
            }
            else if (SeriesLetter.Equals("F"))
            {
                Series.Add(10, "K");
                Series.Add(9, "L");
                Series.Add(8, "M");
                Series.Add(7, "N");
                Series.Add(6, "P");
                Series.Add(5, "Q");
                Series.Add(4, "R");
                Series.Add(3, "T");
                Series.Add(2, "U");
                Series.Add(1, "W");
                Series.Add(0, "X");
            }
            else if (SeriesLetter.Equals("G"))
            {
                Series.Add(10, "T");
                Series.Add(9, "U");
                Series.Add(8, "W");
                Series.Add(7, "X");
                Series.Add(6, "K");
                Series.Add(5, "L");
                Series.Add(4, "M");
                Series.Add(3, "N");
                Series.Add(2, "P");
                Series.Add(1, "Q");
                Series.Add(0, "R");
            }
            else
            {
                return null;
            }

            return Series;
        }
    }
}
