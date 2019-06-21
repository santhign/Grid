using System;
using System.Collections.Generic;
using Core.Enums;
using Core.Models;
using Microsoft.Extensions.Configuration;
using Core.Helpers;
using System.Threading.Tasks;
using Serilog;


namespace Core.Helpers
{
    public class MessageHelper
    {
        // Push message
        public static NotificationMessage GetMessage(            
            string Email, 
            string Name, 
            string messageName, 
            string msgTemplate,
            IConfiguration _configuration, 
            string param1 = null, 
            string param2 = null, 
            string param3 = null,  
            string param4 = null,  
            string param5 = null,  
            string param6 = null,  
            string param7 = null, 
            string param8 = null, 
            string param9 = null, 
            string param10 = null,
            string param11 = null,
            string param12 = null,
            string param13 = null,
            string param14 = null,
            string param15 = null,
            string param16 = null,
            string param17 = null,
            string param18 = null,
            string param19 = null,           
            string param20 = null,
            string param21 = null,
            string param22 = null,
            string param23 = null,
            string param24 = null,
            string param25 = null,
            string param26 = null,
            string param27 = null,
            string param28 = null,
            string param29 = null,
            string param30 = null
            )
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
            if (!string.IsNullOrEmpty(param2))
            {
                msgParams.param2 = param2;
            }
            if (!string.IsNullOrEmpty(param3))
            {
                msgParams.param3 = param3;
            }
            if (!string.IsNullOrEmpty(param4))
            {
                msgParams.param4 = param4;
            }
            if (!string.IsNullOrEmpty(param5))
            {
                msgParams.param5 = param5;
            }
            if (!string.IsNullOrEmpty(param6))
            {
                msgParams.param6 = param6;
            }
            if (!string.IsNullOrEmpty(param7))
            {
                msgParams.param7 = param7;
            }
            if (!string.IsNullOrEmpty(param8))
            {
                msgParams.param8 = param8;
            }
            if (!string.IsNullOrEmpty(param9))
            {
                msgParams.param9 = param9;
            }
            if (!string.IsNullOrEmpty(param10))
            {
                msgParams.param10 = param10;
            }

            if (!string.IsNullOrEmpty(param11))
            {
                msgParams.param11 = param11;
            }

            if (!string.IsNullOrEmpty(param12))
            {
                msgParams.param12 = param12;
            }

            if (!string.IsNullOrEmpty(param13))
            {
                msgParams.param13 = param13;
            }

            if (!string.IsNullOrEmpty(param14))
            {
                msgParams.param14 = param14;
            }

            if (!string.IsNullOrEmpty(param15))
            {
                msgParams.param15 = param15;
            }

            if (!string.IsNullOrEmpty(param16))
            {
                msgParams.param16 = param16;
            }

            if (!string.IsNullOrEmpty(param17))
            {
                msgParams.param17 = param17;
            }

            if (!string.IsNullOrEmpty(param18))
            {
                msgParams.param18 = param18;
            }

            if (!string.IsNullOrEmpty(param19))
            {
                msgParams.param19 = param19;
            }

            if (!string.IsNullOrEmpty(param20))
            {
                msgParams.param20 = param20;
            }

            if (!string.IsNullOrEmpty(param21))
            {
                msgParams.param21 = param21;
            }

            if (!string.IsNullOrEmpty(param22))
            {
                msgParams.param22 = param22;
            }

            if (!string.IsNullOrEmpty(param23))
            {
                msgParams.param23 = param23;
            }

            if (!string.IsNullOrEmpty(param24))
            {
                msgParams.param24 = param24;
            }

            if (!string.IsNullOrEmpty(param25))
            {
                msgParams.param25 = param25;
            }

            if (!string.IsNullOrEmpty(param26))
            {
                msgParams.param26 = param26;
            }

            if (!string.IsNullOrEmpty(param27))
            {
                msgParams.param27 = param27;
            }

            if (!string.IsNullOrEmpty(param28))
            {
                msgParams.param28 = param28;
            }

            if (!string.IsNullOrEmpty(param29))
            {
                msgParams.param29 = param29;
            }

            if (!string.IsNullOrEmpty(param30))
            {
                msgParams.param30 = param30;
            }

            msgParamsList.Add(msgParams);
            

            notificationMessage = new NotificationMessage
            {

                MessageType = NotificationMsgType.Email.ToString(),

                MessageName = messageName,

                Message = new MessageObject { parameters = msgParamsList, messagetemplate = msgTemplate }

            };

            return notificationMessage;
        }



        public static NotificationMessage GetSMSMessage( string messageName,
           string msgTemplate, string name, string email, string mobilenumber,           
           string param1 = null,
           string param2 = null,
           string param3 = null,
           string param4 = null,
           string param5 = null,
           string param6 = null,
           string param7 = null,
           string param8 = null,
           string param9 = null,
           string param10 = null          
           )
        {
            NotificationMessage notificationMessage = new NotificationMessage();

            List<NotificationParams> msgParamsList = new List<NotificationParams>();

            NotificationParams msgParams = new NotificationParams();

            msgParams.emailaddress = email;

            msgParams.name = name;

            msgParams.mobilenumber = mobilenumber;

            if (!string.IsNullOrEmpty(param1))
            {
                msgParams.param1 = param1;
            }
            if (!string.IsNullOrEmpty(param2))
            {
                msgParams.param2 = param2;
            }
            if (!string.IsNullOrEmpty(param3))
            {
                msgParams.param3 = param3;
            }
            if (!string.IsNullOrEmpty(param4))
            {
                msgParams.param4 = param4;
            }
            if (!string.IsNullOrEmpty(param5))
            {
                msgParams.param5 = param5;
            }
            if (!string.IsNullOrEmpty(param6))
            {
                msgParams.param6 = param6;
            }
            if (!string.IsNullOrEmpty(param7))
            {
                msgParams.param7 = param7;
            }
            if (!string.IsNullOrEmpty(param8))
            {
                msgParams.param8 = param8;
            }
            if (!string.IsNullOrEmpty(param9))
            {
                msgParams.param9 = param9;
            }
            if (!string.IsNullOrEmpty(param10))
            {
                msgParams.param10 = param10;
            }          

            msgParamsList.Add(msgParams);


            notificationMessage = new NotificationMessage
            {

                MessageType = NotificationMsgType.SMS.ToString(),

                MessageName = messageName,

                Message = new MessageObject { parameters = msgParamsList, messagetemplate = msgTemplate }

            };

            return notificationMessage;
        }
        // Prepair Message
    }
}
