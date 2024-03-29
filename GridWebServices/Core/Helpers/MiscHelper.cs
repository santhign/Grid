﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using Core.Enums;
using System.Linq;
using System.IO;

namespace Core.Helpers
{
    public class MiscHelper
    {
        public GridAWSS3Config GetGridAwsConfig(List<Dictionary<string, string>> configDict)
        {
            GridAWSS3Config config = new GridAWSS3Config();
            config.AWSAccessKey = configDict.Single(x => x["key"] == "AWSAccessKey")["value"];
            config.AWSSecretKey = configDict.Single(x => x["key"] == "AWSSecretKey")["value"];
            config.AWSBucketName = configDict.Single(x => x["key"] == "AWSBucketName")["value"];
            config.AWSUser = configDict.Single(x => x["key"] == "AWSUser")["value"];
            config.AWSEndPoint = configDict.Single(x => x["key"] == "AWSEndPoint")["value"];
            return config;
        }

        public ForgotPasswordMsgConfig GetResetPasswordNotificationConfig(List<Dictionary<string, string>> configDict)
        {
            ForgotPasswordMsgConfig config = new ForgotPasswordMsgConfig();
            config.ForgotPasswordSNSTopic = configDict.Single(x => x["key"] == "ForgotPasswordSNSTopic")["value"];
            config.PasswordResetUrl = configDict.Single(x => x["key"] == "PasswordResetUrl")["value"];            
            return config;
        }

        public NotificationConfig GetNotificationConfig(List<Dictionary<string, string>> configDict)
        {
            NotificationConfig config = new NotificationConfig();
            config.SNSTopic = configDict.Single(x => x["key"] == "SNSTopic")["value"];
            config.SQS = configDict.Single(x => x["key"] == "SQS")["value"];
            return config;
        }

        public string GetBase64StringFromByteArray(byte[] byteArray, string fileName)
        {            
            string base64String = Convert.ToBase64String(byteArray, 0, byteArray.Length);

           return "data:" + GetContentTypeFromExtension(fileName) + ";base64," + base64String;
        }

        public string GetContentTypeFromExtension(string filename)
        {
            return MimeTypes.GetMimeType(filename);
        }
        public static string GetFileExtension(string base64String)
        {
            var data = base64String.Substring(0, 5);

            switch (data.ToUpper())
            {
                case "IVBOR":
                    return "png";
                case "/9J/4":
                    return "jpg";
                case "AAAAF":
                    return "mp4";
                case "JVBER":
                    return "pdf";
                case "AAABA":
                    return "ico";
                case "UMFYI":
                    return "rar";
                case "E1XYD":
                    return "rtf";
                case "U1PKC":
                    return "txt";
                case "MQOWM":
                case "77U/M":
                    return "srt";
                default:
                    return "bmp";
            }
        }

    }
}
