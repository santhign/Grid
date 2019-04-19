using System;
using System.Collections.Generic;
using System.Text;
using Core.Models;
using Amazon.SQS.Model;

namespace InfrastructureService.Handlers
{
   public static class SQSHandler
    {
        public static void FinalProcessing(object message)
        {
            var queMessage = message;
        }       
    }
}
