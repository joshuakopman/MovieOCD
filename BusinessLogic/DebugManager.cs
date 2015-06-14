using System;
using System.Configuration;
using Elmah;

namespace MovieOCD.BusinessLogic
{
    public class DebugManager
    {
        public static void LogWarning(string msg)
        {
            if(ConfigurationManager.AppSettings["DebugMode"] == "On")
            {
                ErrorSignal.FromCurrentContext().Raise(new Exception(msg));
            }
        }

        public static void LogException(Exception ex)
        {
            ErrorSignal.FromCurrentContext().Raise(ex);
        }
    }
}