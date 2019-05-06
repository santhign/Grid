using System.Collections.Generic;
using Core.Enums;
using Core.Models;
using Microsoft.Extensions.Configuration;

namespace Core.Helpers
{
    public class MessageHelper
    {
        // Push message
        public static NotificationMessage GetMessage(string Email, string Name, string msgTemplate,IConfiguration _configuration, string param1 = null, 
            string param2 = null, string param3 = null,  string param4 = null,  string param5 = null,  string param6 = null,  string param7 = null, 
            string param8 = null, string param9 = null, string param10 = null)
        {
            NotificationMessage notificationMessage = new NotificationMessage();

            List<NotificationParams> msgParamsList = new List<NotificationParams>();

            NotificationParams msgParams = new NotificationParams();

            msgParams.emailaddress = Email;

            msgParams.name = Name;

            if (!string.IsNullOrEmpty(param1))
            {
                msgParams.param1 = param1;
            }
            else if (!string.IsNullOrEmpty(param2))
            {
                msgParams.param2 = param2;
            }
            else if (!string.IsNullOrEmpty(param3))
            {
                msgParams.param3 = param3;
            }
            else if (!string.IsNullOrEmpty(param4))
            {
                msgParams.param4 = param4;
            }
            else if (!string.IsNullOrEmpty(param5))
            {
                msgParams.param5 = param5;
            }
            else if (!string.IsNullOrEmpty(param6))
            {
                msgParams.param6 = param6;
            }
            else if (!string.IsNullOrEmpty(param7))
            {
                msgParams.param7 = param7;
            }
            else if (!string.IsNullOrEmpty(param8))
            {
                msgParams.param8 = param8;
            }
            else if (!string.IsNullOrEmpty(param9))
            {
                msgParams.param9 = param9;
            }
            else if (!string.IsNullOrEmpty(param10))
            {
                msgParams.param10 = param10;
            }          

            msgParamsList.Add(msgParams);
            

            notificationMessage = new NotificationMessage
            {

                MessageType = NotificationMsgType.Email.ToString(),

                MessageName = NotificationEvent.ForgetPassword.ToString(),

                Message = new MessageObject { parameters = msgParamsList, messagetemplate = msgTemplate }

            };

            return notificationMessage;
        }
        // Prepair Message
    }
}
