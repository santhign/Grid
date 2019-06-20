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
                string customer = "Customer";
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

                recipients = (from param in notificationMessage.Message.parameters select new Mandrill.Model.MandrillMailAddress { Email = param.emailaddress, Name = (param.name == "" || param.name == null ? customer : param.name) }).ToList();

                foreach(NotificationParams param in notificationMessage.Message.parameters)
                {                   
                    List<Mandrill.Model.MandrillMergeVar> mergeVars = new List<Mandrill.Model.MandrillMergeVar>();
                    Mandrill.Model.MandrillMergeVar mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "EMAIL";
                    mergevar.Content = param.emailaddress;
                    mergeVars.Add(mergevar);
                    mergevar = new Mandrill.Model.MandrillMergeVar();
                    mergevar.Name = "NAME";
                    mergevar.Content = (param.name == "" || param.name == null ? customer : param.name);
                    mergeVars.Add(mergevar);
                  
                   
                    if (param.param1 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM1";
                        mergevar.Content = param.param1;
                        mergeVars.Add(mergevar);                        
                    }

                    if (param.param2 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM2";
                        mergevar.Content = param.param2;
                        mergeVars.Add(mergevar);                        
                    }

                    if (param.param3 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM3";
                        mergevar.Content = param.param3;
                        mergeVars.Add(mergevar);                       
                    }

                    if (param.param4 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM4";
                        mergevar.Content = param.param4;
                        mergeVars.Add(mergevar);                       
                    }
                    if (param.param5 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM5";
                        mergevar.Content = param.param5;
                        mergeVars.Add(mergevar);
                        
                    }
                    if (param.param6 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM6";
                        mergevar.Content = param.param6;
                        mergeVars.Add(mergevar);                        
                    }


                    if (param.param7 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM7";
                        mergevar.Content = param.param7;
                        mergeVars.Add(mergevar);                       

                    }

                    if (param.param8 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM8";
                        mergevar.Content = param.param8;
                        mergeVars.Add(mergevar);                       
                    }

                    if (param.param9 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM9";
                        mergevar.Content = param.param9;
                        mergeVars.Add(mergevar);                       
                    }

                    if (param.param10 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM10";
                        mergevar.Content = param.param10;
                        mergeVars.Add(mergevar);                       
                    }

                    if (param.param11 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM11";
                        mergevar.Content = param.param11;
                        mergeVars.Add(mergevar);                        
                    }

                    if (param.param12 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM12";
                        mergevar.Content = param.param12;
                        mergeVars.Add(mergevar);                        
                    }

                    if (param.param13 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM13";
                        mergevar.Content = param.param13;
                        mergeVars.Add(mergevar);                      
                    }

                    if (param.param14 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM14";
                        mergevar.Content = param.param14;
                        mergeVars.Add(mergevar);                      
                    }

                    if (param.param15 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM15";
                        mergevar.Content = param.param15;
                        mergeVars.Add(mergevar);                       
                    }

                    if (param.param16 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM16";
                        mergevar.Content = param.param16;
                        mergeVars.Add(mergevar);                       
                    }

                    if (param.param17 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM17";
                        mergevar.Content = param.param17;
                        mergeVars.Add(mergevar);                       
                    }

                    if (param.param18 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM18";
                        mergevar.Content = param.param18;
                        mergeVars.Add(mergevar);                        
                    }

                    if (param.param19 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM19";
                        mergevar.Content = param.param19;
                        mergeVars.Add(mergevar);                        
                    }

                    if (param.param20 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM20";
                        mergevar.Content = param.param20;
                        mergeVars.Add(mergevar);                       
                    }

                    if (param.param21 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM21";
                        mergevar.Content = param.param21;
                        mergeVars.Add(mergevar);                       
                    }

                    if (param.param22 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM22";
                        mergevar.Content = param.param22;
                        mergeVars.Add(mergevar);                        
                    }

                    if (param.param23 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM23";
                        mergevar.Content = param.param23;
                        mergeVars.Add(mergevar);                        
                    }

                    if (param.param24 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM24";
                        mergevar.Content = param.param24;
                        mergeVars.Add(mergevar);                       
                    }

                    if (param.param25 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM25";
                        mergevar.Content = param.param25;
                        mergeVars.Add(mergevar);                       
                    }

                    if (param.param26 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM26";
                        mergevar.Content = param.param26;
                        mergeVars.Add(mergevar);                        
                    }

                    if (param.param27 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM27";
                        mergevar.Content = param.param27;
                        mergeVars.Add(mergevar);                        
                    }

                    if (param.param28 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM28";
                        mergevar.Content = param.param28;
                        mergeVars.Add(mergevar);                        
                    }

                    if (param.param29 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM29";
                        mergevar.Content = param.param29;
                        mergeVars.Add(mergevar);
                       
                    }

                    if (param.param30 != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "PARAM30";
                        mergevar.Content = param.param30;
                        mergeVars.Add(mergevar);
                       
                    }

                    if (param.mobilenumber != null)
                    {
                        mergevar = new Mandrill.Model.MandrillMergeVar();
                        mergevar.Name = "UNIQUEID";
                        mergevar.Content = param.mobilenumber;
                        mergeVars.Add(mergevar);                       
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
    }
   
}
