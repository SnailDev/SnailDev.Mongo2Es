using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Mongo2Es
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            ThreadPool.GetMinThreads(out int workMinThreads, out int completionPortMinThreads);
            ThreadPool.GetMaxThreads(out int workMaxThreads, out int completionPortMaxThreads);

            var arguments = new ConfigurationBuilder().AddCommandLine(args).Build();
            var url = arguments["bindip"] ?? "http://localhost:5000";
            var mongoUrl = arguments["mongo"] ?? "mongodb://localhost:27017";

            //Log
            string assemblyFolder = Directory.GetCurrentDirectory();
            NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(Path.Combine(assemblyFolder, "NLog.config"), true);
            logger.Info("Log Setting Completed.");
            logger.Info($"workMinThreads:{workMinThreads}, completionPortMinThreads:{completionPortMinThreads}.");
            logger.Info($"workMaxThreads:{workMaxThreads}, completionPortMaxThreads:{completionPortMaxThreads}.");

            //ThreadPool.SetMinThreads(100, 100);

            // Service
            Thread syncSerivce = new Thread(() =>
            {
                var client = new Middleware.SyncClient(mongoUrl);
                client.Run();
                logger.Info("Service Start Completed.");
            });

            // Web
            Thread syncWeb = new Thread(() =>
            {
                var host =
                    WebHost.CreateDefaultBuilder(args)
                    .UseContentRoot(assemblyFolder)
                    .UseUrls(url)
                    .UseStartup<Startup>()
                    .Build();

                logger.Info("Web Start Completed.");
                host.Run();
            });

            syncSerivce.Start();
            syncWeb.Start();


            //var client = new Middleware.SyncClient(mongoUrl);
            //client.ExcuteTailProcess(new Middleware.SyncNode()
            //{
            //    ID = "5b7a7dd269445f2c1c48b776",
            //    Name = "User",
            //    MongoUrl = "mongodb://muser:1qaz2wsx@121.40.119.230:27017",
            //    DataBase = "WanMing",
            //    Collection = "User",
            //    IsLog = false,
            //    LinkField = null,
            //    ProjectFields = null,
            //    EsUrl = "http://localhost:9200",
            //    Index = "user",
            //    Type = "user",
            //    Switch = Middleware.SyncSwitch.Run,
            //    Status = Middleware.SyncStatus.ProcessTail,
            //    OperType = 0,
            //    OperScanSign = "99999",
            //    OperTailSign = 1534762767,
            //    OperTailSignExt = 17569
            //});
            //logger.Info("Service Start Completed.");
            //Console.ReadLine();
        }
    }
}
