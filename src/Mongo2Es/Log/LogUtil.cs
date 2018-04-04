using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mongo2Es.Log
{
    public static class LogUtil
    {
        /// <summary>
        /// LogInfo
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <param name="nodeId"></param>
        public static void LogInfo(Logger logger, string message, string nodeId)
        {
            try
            {
                LogEventInfo logEvent = new LogEventInfo(NLog.LogLevel.Info, logger.Name, message);
                logEvent.Properties["nodeid"] = nodeId;
                logger.Log(logEvent);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// LogError
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <param name="nodeId"></param>
        public static void LogError(Logger logger, string message, string nodeId)
        {
            try
            {
                LogEventInfo logEvent = new LogEventInfo(NLog.LogLevel.Error, logger.Name, message);
                logEvent.Properties["nodeid"] = nodeId;

                logger.Log(logEvent);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
