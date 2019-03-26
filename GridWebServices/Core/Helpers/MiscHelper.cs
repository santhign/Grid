﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using Core.Enums;
using System.Linq;

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
            return config;
        }
    }
}
