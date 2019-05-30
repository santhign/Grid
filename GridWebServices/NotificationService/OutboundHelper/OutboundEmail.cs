using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Helpers;
using Core.Models;
using Core.DataAccess;
using Core.Enums;
using System.Web;
using Mandrill;
using Microsoft.Extensions.Configuration;
using NotificationService.Models;
using System.Text.Encodings.Web;

namespace NotificationService.OutboundHelper
{
    public class OutboundEmail
    {
        public async Task<IList<Mandrill.Model.MandrillSendMessageResponse>> SendEmail(NotificationMessage notificationEmail, IConfiguration _iconfiguration, EmailTemplate template)
        {
            MandrilConfig mandrilConfig = new MandrilConfig();

            mandrilConfig = GetMandrillConfig(_iconfiguration);   

            MandrillApi api = new MandrillApi(mandrilConfig.MandrillKey);

            var result = await ((MandrillApi)GetMandrillApi(mandrilConfig)).Messages.SendAsync(GenerateMandrillMessage(notificationEmail, mandrilConfig,template, true));

            return result;
        }

        public MandrillApi GetMandrillApi(MandrilConfig config)
        {          
            return   new MandrillApi(config.MandrillKey);
        }

        public MandrilConfig  GetMandrillConfig(IConfiguration _iconfiguration)
        {
            DatabaseResponse configResponse = ConfigHelper.GetValue("Email", _iconfiguration);

            MandrilConfig emailConfig = new MandrilConfig();

            List<Dictionary<string, string>> _result = ((List<Dictionary<string, string>>)configResponse.Results);
            emailConfig.FromEmail = _result.Single(x => x["key"] == "FromEmail")["value"];
            emailConfig.FromName = _result.Single(x => x["key"] == "FromName")["value"];
            emailConfig.MandrillKey = _result.Single(x => x["key"] == "MandrillKey")["value"];
            emailConfig.ReplyEmail = _result.Single(x => x["key"] == "ReplyEmail")["value"];

            return emailConfig;
        }

        private static Mandrill.Model.MandrillMessage GenerateMandrillMessage(NotificationMessage notificationMessage, MandrilConfig config, EmailTemplate template,  bool useTemplate = true)
        {
           try
            {               

                Mandrill.Model.MandrillMessage eMsg = new Mandrill.Model.MandrillMessage();
                eMsg.Html = HttpUtility.HtmlDecode(template.EmailBody); 
                eMsg.Subject = template.EmailSubject;
                eMsg.FromEmail = config.FromEmail;
                eMsg.FromName = config.FromName;
                eMsg.BccAddress = notificationMessage.Message.bccAddress != null ? notificationMessage.Message.bccAddress : "";
                Dictionary<string, object> mailHeader = new Dictionary<string, object>();
                mailHeader.Add("Reply-To", config.ReplyEmail);
                eMsg.Headers = mailHeader;
                eMsg.PreserveRecipients = false;
                eMsg.TrackClicks = true;
                eMsg.Merge = true;

                List < Mandrill.Model.MandrillMailAddress> recipients = new List<Mandrill.Model.MandrillMailAddress>();

                List<Mandrill.Model.MandrillRcptMergeVar> recepientMergeVarList = new List<Mandrill.Model.MandrillRcptMergeVar>();
               // Mandrill.NameContentList<string> content = new Mandrill.NameContentList<string>();

                recipients = (from param in notificationMessage.Message.parameters select new Mandrill.Model.MandrillMailAddress { Email = param.emailaddress, Name = param.name }).ToList();

                foreach(NotificationParams param in notificationMessage.Message.parameters)
                {                   
                    List<Mandrill.Model.MandrillMergeVar> mergeVars = new List<Mandrill.Model.MandrillMergeVar>();
                    Mandrill.Model.MandrillMergeVar mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "EMAIL";
                    mergevar.Content = param.emailaddress;
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "NAME";
                    mergevar.Content = param.name;
                    mergeVars.Add(mergevar);
                    //eMsg.Html=eMsg.Html.Replace("[NAME]", param.name);
                   
                    if (param.param1 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM1";
                        mergevar.Content = param.param1;
                        mergeVars.Add(mergevar);

                        //eMsg.Html= eMsg.Html.Replace("[PARAM1]", param.param1);
                    }

                    if (param.param2 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM2";
                        mergevar.Content = param.param2;
                        mergeVars.Add(mergevar);

                        //eMsg.Html= eMsg.Html.Replace("[PARAM2]", param.param2);
                    }

                    if (param.param3 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM3";
                        mergevar.Content = param.param3;
                        mergeVars.Add(mergevar);

                        //eMsg.Html=eMsg.Html.Replace("[PARAM3]", param.param3);
                    }

                    if (param.param4 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM4";
                        mergevar.Content = param.param4;
                        mergeVars.Add(mergevar);
                        //eMsg.Html= eMsg.Html.Replace("[PARAM4]", param.param4);
                    }
                    if (param.param5 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM5";
                        mergevar.Content = param.param5;
                        mergeVars.Add(mergevar);

                        //eMsg.Html= eMsg.Html.Replace("[PARAM5]", param.param5);
                    }
                    if (param.param7 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM7";
                        mergevar.Content = param.param7;
                        mergeVars.Add(mergevar);
                        //eMsg.Html= eMsg.Html.Replace("[PARAM6]", param.param6);
                    }


                    if (param.param7 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM7";
                        mergevar.Content = param.param7;
                        mergeVars.Add(mergevar);
                        //eMsg.Html= eMsg.Html.Replace("[PARAM7]", param.param7);

                    }

                    if (param.param8 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM8";
                        mergevar.Content = param.param8;
                        mergeVars.Add(mergevar);
                        //eMsg.Html= eMsg.Html.Replace("[PARAM8]", param.param8);
                    }

                    if (param.param9 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM9";
                        mergevar.Content = param.param9;
                        mergeVars.Add(mergevar);
                        //eMsg.Html= eMsg.Html.Replace("[PARAM9]", param.param9);
                    }

                    if (param.param10 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM10";
                        mergevar.Content = param.param10;
                        mergeVars.Add(mergevar);
                        //eMsg.Html= eMsg.Html.Replace("[PARAM10]", param.param10);
                    }

                    if (param.mobilenumber != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "UNIQUEID";
                        mergevar.Content = param.mobilenumber;
                        mergeVars.Add(mergevar);

                        //eMsg.Html= eMsg.Html.Replace("[UNIQUEID]", param.mobilenumber);
                    }
                   
                    Mandrill.Model.MandrillRcptMergeVar rcptMergeVar = new Mandrill.Model.MandrillRcptMergeVar();
                    rcptMergeVar.Rcpt = param.emailaddress;
                    rcptMergeVar.Vars = mergeVars;
                    recepientMergeVarList.Add(rcptMergeVar);

                }

                eMsg.To = recipients;

                eMsg.MergeVars = recepientMergeVarList;

                return eMsg;

            }

            catch (Exception ex)
            {
                throw ex;
            } 
           
        }  
        
        public string GetMergedTemplate (NotificationParams param, EmailTemplate template)
        {
            string messageBody = template.EmailBody;

            if (param.param1 != null)
            {

                messageBody = messageBody.Replace("[PARAM1]", param.param1);
            }

            if (param.param2 != null)
            {


                messageBody = messageBody.Replace("[PARAM2]", param.param2);
            }

            if (param.param3 != null)
            {


                messageBody = messageBody.Replace("[PARAM3]", param.param3);
            }

            if (param.param4 != null)
            {

                messageBody = messageBody.Replace("[PARAM4]", param.param4);
            }
            if (param.param5 != null)
            {


                messageBody = messageBody.Replace("[PARAM5]", param.param5);
            }
            if (param.param7 != null)
            {

                messageBody = messageBody.Replace("[PARAM6]", param.param6);
            }


            if (param.param7 != null)
            {

                messageBody = messageBody.Replace("[PARAM7]", param.param7);

            }

            if (param.param8 != null)
            {

                messageBody = messageBody.Replace("[PARAM8]", param.param8);
            }

            if (param.param9 != null)
            {

                messageBody = messageBody.Replace("[PARAM9]", param.param9);
            }

            if (param.param10 != null)
            {

                messageBody = messageBody.Replace("[PARAM10]", param.param10);
            }

            if (param.mobilenumber != null)
            {

                messageBody = messageBody.Replace("[UNIQUEID]", param.mobilenumber);
            }

            return messageBody;
        }
    }
   
}
