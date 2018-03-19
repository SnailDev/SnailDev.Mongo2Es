using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Mongo2Es
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {

            var arguments = new ConfigurationBuilder().AddCommandLine(args).Build();
            var port = arguments["port"] ?? "9300";
            var mongoUrl = arguments["mongo"] ?? "mongodb://localhost:27017";

            // Web
            Thread syncWeb = new Thread(() =>
            {
                var host =
                    WebHost.CreateDefaultBuilder(args)
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseUrls($"http://localhost:{port}")
                    .UseStartup<Startup>()
                    .Build();

                host.Run();
            });
            syncWeb.Start();

            // Service
            Thread syncSerivce = new Thread(() =>
            {
                //var servicesProvider = BuildDi();
                //var runner = servicesProvider.GetRequiredService<Middleware.SyncClient>();
                var client = new Middleware.SyncClient(mongoUrl);
                client.Run();
            });
            syncSerivce.Start();
        }

        private static IServiceProvider BuildDi()
        {
            var services = new ServiceCollection();

            //Runner is the custom class
            services.AddTransient<Mongo.MongoClient>();
            services.AddTransient<ElasticSearch.EsClient>();
            services.AddTransient<Middleware.SyncClient>();

            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging((builder) => builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace));

            var serviceProvider = services.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            //configure NLog
            loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
            loggerFactory.ConfigureNLog("NLog.config");

            return serviceProvider;
        }

        static void Test()
        {
            var client = new Mongo.MongoClient("mongodb://192.168.110.6:27000/?slaveOk=true");
            var esClient = new Mongo2Es.ElasticSearch.EsClient("http://localhost:9200");
            #region MongoDB Test
            //foreach (var doc in client.GetMongoOpLogs("test", "xiaoming"))
            //{
            //    Console.WriteLine(doc.ToString());
            //}

            //foreach (var doc in client.ListDataBases())
            //{
            //    Console.WriteLine(doc.ToString());

            //    var dbName = doc["name"].AsString;

            //    foreach (var doc1 in client.ListCollections(dbName))
            //    {
            //        Console.WriteLine(doc1.ToString());
            //    }
            //}

            //foreach (var doc in client.GetCollectionData("test", "xiaoming"))
            //{
            //    Console.WriteLine(doc.ToString());

            //    #region ElasticSearch Test
            //    // Console.WriteLine(esClient.InsertDocument("test.xiaoming", "test.xiaoming", doc));
            //    // Console.WriteLine(esClient.UpdateDocument("test.xiaoming", "test.xiaoming", doc.AddRange(MongoDB.Bson.BsonDocument.Parse("{'age':123}"))));
            //    // Console.WriteLine(esClient.DeleteDocument("test.xiaoming", "test.xiaoming", doc)); 
            //    #endregion
            //}

            //foreach (var doc in client.ListFields("test", "xiaoming"))
            //{
            //    Console.WriteLine(doc.ToString());
            //}
            #endregion

            var nodes = new List<Middleware.SyncNode>()
                {
                    new Middleware.SyncNode()
                    {
                        ID = "1",
                        MongoUrl = "mongodb://192.168.110.6:27000/?slaveOk=true",
                        DataBase = "test",
                        Collection = "xiaoming",
                        ProjectFields = "Haha,haha,age", //mark
                        EsUrl = "http://localhost:9200",
                        Index =  "test.xiaoming1",
                        Type =  "test.xiaoming"
                    },
                    new Middleware.SyncNode()
                    {
                        ID = "2",
                        MongoUrl = "mongodb://192.168.110.6:27001/?slaveOk=true",
                        DataBase = "test",
                        Collection = "xiaoming",
                        ProjectFields = "Haha,haha,age", //mark
                        EsUrl = "http://localhost:9200",
                        Index =  "test.xiaoming2",
                        Type =  "test.xiaoming"
                    }
                };
        }
    }
}
