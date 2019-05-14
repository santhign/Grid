using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Core.Models;
using Core.Enums;
using Core.Extensions;
using System.IO;



namespace Core.Helpers
{
    public class ExceptionHelper
    {
        public string GetLogString(Exception ex, ErrorLevel level)
        {
            try
            {   
                StackTrace st = new StackTrace(ex, true);
                //Get the first stack frame
                StackFrame frame = st.GetFrame(0);

                //JsonConvert.SerializeObject();

                ExceptionLog exLog = new ExceptionLog
                {
                    ExceptionLogId = Guid.NewGuid().ToString().ToLower(),
                    ExceptionType = ex.GetType().FullName.ToString(),
                    ExceptionInnerException = ex.InnerException == null ? "" : ex.InnerException.ToString(),
                    ExceptionMessage = ex.Message,
                    ExceptionSeverity = EnumExtensions.GetDescription(level),
                    ExceptionFileName = frame.GetFileName()!=null? Path.GetFileName((frame.GetFileName())):"", //Get the file name
                    ExceptionLineNumber = frame.GetFileLineNumber(),  //Get the line number
                    ExceptionColumnNumber = frame.GetFileColumnNumber(), //Get the column number                      
                    ExceptionMethodName = ex.TargetSite.ReflectedType.FullName, // Get the method name
                    ExceptionStackTrace = ex.StackTrace

                }; 
                    string excep = $"ExceptionLogId:{exLog.ExceptionLogId}, ExceptionType:{exLog.ExceptionType}, " +
                    $"ExceptionInnerException:{exLog.ExceptionInnerException}, ExceptionMessage:{exLog.ExceptionMessage}, " +
                    $"ExceptionSeverity:{exLog.ExceptionSeverity}, ExceptionFileName:{exLog.ExceptionFileName}, ExceptionMethodName:{exLog.ExceptionMethodName}, " +
                    $"ExceptionLineNumber:{exLog.ExceptionLineNumber}, ExceptionColumnNumber:{exLog.ExceptionColumnNumber} , ExceptionStackTrace:{exLog.ExceptionStackTrace}" ;

                return excep; 


            }

            catch(Exception e)
            {
                throw e;
            }

        }

    }
}
