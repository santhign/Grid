using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Helpers;
using Core.Models;
using Mandrill;
using Microsoft.Extensions.Configuration;
using NotificationService.Models;

namespace NotificationService.OutboundHelper
{
    public class OutboundEmail
    {
        public Task<IList<Mandrill.Model.MandrillSendMessageResponse>> SendEmail(NotificationEmail emailSubscribers, IConfiguration _iconfiguration)
        {
            DatabaseResponse configResponse = ConfigHelper.GetValue("Email", _iconfiguration);

            List<Dictionary<string, string>> _result = ((List<Dictionary<string, string>>)configResponse.Results);
            string _fromEmail = _result.Single(x => x["key"] == "FromEmail")["value"];
            string _fromName = _result.Single(x => x["key"] == "FromName")["value"];
            string _apiKey = _result.Single(x => x["key"] == "MandrillKey")["value"];
            string _replyEmail = _result.Single(x => x["key"] == "ReplyEmail")["value"];

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
            List<Mandrill.Model.MandrillRcptMergeVar> mergeVarList = new List<Mandrill.Model.MandrillRcptMergeVar>();
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
            var result = api.Messages.SendAsync(eMsg);
            IList<Mandrill.Model.MandrillSendMessageResponse> _emailresult = result.Result;
            string Status = _emailresult[0].Status.ToString();
            //register the outbound email in DB
            return result;
        }
    }
}
