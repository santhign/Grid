using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailChimp;
using MailChimp.Types;
using NotificationService.Models;
using Core.Helpers;
using Microsoft.Extensions.Configuration;
using InfrastructureService;
using Core.Enums;
using Core.Models;

namespace NotificationService.DataAccess
{
    public class EmailNotificationDataAccess
    {

        internal DataAccessHelper _DataHelper = null;

        private IConfiguration _configuration;

        /// <summary>
        /// Constructor setting configuration
        /// </summary>
        /// <param name="configuration"></param>
        public EmailNotificationDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<List<Mandrill.Messages.SendResult>> SendEmail(NotificationEmail emailSubscribers)
        {
            try
            {

                DatabaseResponse configResponse = ConfigHelper.GetValue("Email", _configuration);

                List <Dictionary<string, string>> _result = ((List<Dictionary<string, string>>)configResponse.Results);

                string _apiKey = _result.Single(x => x["key"] == "MandrillKey").Select(x => x.Value).ToString ();
                string _fromEmail = _result.Single(x => x["key"] == "FromEmail").Select(x => x.Value).ToString();
                string _fromName = _result.Single(x => x["key"] == "FromName").Select(x => x.Value).ToString();
                string _replyEmail = _result.Single(x => x["key"] == "ReplyEmail").Select(x => x.Value).ToString();



                MailChimp.MandrillApi api = new MandrillApi(_apiKey);

                Mandrill.Messages.Message eMsg = new Mandrill.Messages.Message();
                eMsg.Html = emailSubscribers.Content;
                eMsg.Subject = emailSubscribers.Subject;
                eMsg.FromEmail = _fromEmail;
                eMsg.FromName = _fromName;
                eMsg.BccAddress = emailSubscribers.BccAddress;
                Mandrill.Messages.Header.Create("Reply-To");
                MCDict<Mandrill.Messages.Header> mc = new MCDict<Mandrill.Messages.Header>();
                mc.Add("Reply-To", _replyEmail);
                eMsg.Headers = mc; 
                eMsg.PreserveRecipients = false; 
                eMsg.TrackClicks = true;           
                eMsg.Merge = true;
                int recipientCount = emailSubscribers.EmailDetails.Count();
                Mandrill.Messages.Recipient[] recipientList = new Mandrill.Messages.Recipient[recipientCount];  
                Mandrill.Messages.MergeVars[] mergeVarList = new Mandrill.Messages.MergeVars[recipientCount];    
                Mandrill.NameContentList<string> content = new Mandrill.NameContentList<string>();

                for (int counter = 0; counter < recipientCount; counter++)
                {
                     
                    recipientList[counter] = new Mandrill.Messages.Recipient(emailSubscribers.EmailDetails[counter].EMAIL, emailSubscribers.EmailDetails[counter].FName);

                     
                    var mergeVars = new Mandrill.NameContentList<string>();
                    mergeVars.Add("EMAIL", emailSubscribers.EmailDetails[counter].EMAIL.ToString());
                    mergeVars.Add("NAME", emailSubscribers.EmailDetails[counter].FName.ToString());
                    mergeVars.Add("PARAM1", emailSubscribers.EmailDetails[counter].Param1.ToString());
                    mergeVars.Add("PARAM2", emailSubscribers.EmailDetails[counter].Param2.ToString());
                    mergeVars.Add("PARAM3", emailSubscribers.EmailDetails[counter].Param3.ToString());
                    mergeVars.Add("PARAM4", emailSubscribers.EmailDetails[counter].Param4.ToString());
                    mergeVars.Add("PARAM5", emailSubscribers.EmailDetails[counter].Param5.ToString());
                    mergeVars.Add("PARAM6", emailSubscribers.EmailDetails[counter].Param6.ToString());
                    mergeVars.Add("PARAM7", emailSubscribers.EmailDetails[counter].Param7.ToString());
                    mergeVars.Add("PARAM8", emailSubscribers.EmailDetails[counter].Param8.ToString());
                    mergeVars.Add("PARAM9", emailSubscribers.EmailDetails[counter].Param9.ToString());
                    mergeVars.Add("PARAM10", emailSubscribers.EmailDetails[counter].Param10.ToString());
                    mergeVars.Add("UNIQUEID", emailSubscribers.EmailDetails[counter].Userid.ToString());
                    mergeVarList[counter] = new Mandrill.Messages.MergeVars(emailSubscribers.EmailDetails[counter].EMAIL, mergeVars);
                }

                eMsg.To = recipientList;
                eMsg.MergeVars = mergeVarList;
                MVList<Mandrill.Messages.SendResult> result = new MVList<Mandrill.Messages.SendResult>();
                result = api.Send(eMsg);
                return result;
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
