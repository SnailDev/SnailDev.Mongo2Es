using NLog;
using System;

namespace Mongo2Es.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            //var config = new NLog.Config.LoggingConfiguration();

            //var logfile = new NLog.Targets.FileTarget() { FileName = "file.txt", Name = "logfile" };
            //var logconsole = new NLog.Targets.ConsoleTarget() { Name = "logconsole" };

            //config.LoggingRules.Add(new NLog.Config.LoggingRule("*", LogLevel.Info, logconsole));
            //config.LoggingRules.Add(new NLog.Config.LoggingRule("*", LogLevel.Debug, logfile));

            //NLog.LogManager.Configuration = config;

            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("Hello World");

            Console.ReadLine();
        }
    }
}
