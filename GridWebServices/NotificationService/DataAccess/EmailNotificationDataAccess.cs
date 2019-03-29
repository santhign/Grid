using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mandrill;
//using MailChimp.Types;
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
        public async Task<IList<Mandrill.Model.MandrillSendMessageResponse>> SendEmail(NotificationEmail emailSubscribers)
        {
            try
            {

                DatabaseResponse configResponse = ConfigHelper.GetValue("Email", _configuration);

                List <Dictionary<string, string>> _result = ((List<Dictionary<string, string>>)configResponse.Results);

                string _apiKey = _result.Single(x => x["key"] == "MandrillKey").Select(x => x.Value).ToString ();
                string _fromEmail = _result.Single(x => x["key"] == "FromEmail").Select(x => x.Value).ToString();
                string _fromName = _result.Single(x => x["key"] == "FromName").Select(x => x.Value).ToString();
                string _replyEmail = _result.Single(x => x["key"] == "ReplyEmail").Select(x => x.Value).ToString();



                MandrillApi api = new MandrillApi(_apiKey);

                Mandrill.Model.MandrillMessage eMsg = new Mandrill.Model.MandrillMessage();
                eMsg.Html = emailSubscribers.Content;
                eMsg.Subject = emailSubscribers.Subject;
                eMsg.FromEmail = _fromEmail;
                eMsg.FromName = _fromName;
                eMsg.BccAddress = emailSubscribers.BccAddress;
                Dictionary<string, object> mc = new Dictionary<string, object>();
                mc.Add("Reply-To", _replyEmail);
                eMsg.Headers = mc; 
                eMsg.PreserveRecipients = false; 
                eMsg.TrackClicks = true;           
                eMsg.Merge = true;
                int recipientCount = emailSubscribers.EmailDetails.Count();
                List<Mandrill.Model.MandrillMailAddress> recipientList = new List<Mandrill.Model.MandrillMailAddress>();
                List < Mandrill.Model.MandrillRcptMergeVar> mergeVarList = new List<Mandrill.Model.MandrillRcptMergeVar>();    
                //Mandrill.NameContentList<string> content = new Mandrill.NameContentList<string>();

                for (int counter = 0; counter < recipientCount; counter++)
                {
                   recipientList.Add(new Mandrill.Model.MandrillMailAddress(emailSubscribers.EmailDetails[counter].EMAIL, emailSubscribers.EmailDetails[counter].FName));

                    //mergeVarList.Add()
                    List<Mandrill.Model.MandrillMergeVar> mergeVars = new List<Mandrill.Model.MandrillMergeVar>();
                    Mandrill.Model.MandrillMergeVar mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "EMAIL";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].EMAIL.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "NAME";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].FName.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "PARAM1";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Param1.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "PARAM2";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Param2.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "PARAM3";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Param3.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "PARAM4";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Param4.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "PARAM5";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Param5.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "PARAM6";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Param6.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "PARAM7";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Param7.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "PARAM8";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Param8.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "PARAM9";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Param9.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "PARAM10";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Param10.ToString();
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "UNIQUEID";
                    mergevar.Content = emailSubscribers.EmailDetails[counter].Userid.ToString();
                    mergeVars.Add(mergevar);
                    Mandrill.Model.MandrillRcptMergeVar rcptMergeVar = new Mandrill.Model.MandrillRcptMergeVar();
                    rcptMergeVar.Rcpt = emailSubscribers.EmailDetails[counter].EMAIL;
                    rcptMergeVar.Vars = mergeVars;
                    mergeVarList.Add(rcptMergeVar);
                }

                eMsg.To = recipientList;
                eMsg.MergeVars = mergeVarList;
                Task<IList<Mandrill.Model.MandrillSendMessageResponse>> result;
                result = api.Messages.SendAsync(eMsg);
                return await result;
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
