using NLog;
using System;
using System.Collections.Generic;
using System.IO;

namespace Mongo2Es.Tests
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            //string assemblyFolder = Directory.GetCurrentDirectory();
            //NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(Path.Combine(assemblyFolder, "NLog.config"), true);
            //logger.Info("Hello World");

            // 这种延迟方法会占用cpu
            DateTime startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalSeconds < 10) { Console.WriteLine("Hello"); }

            Console.ReadLine();
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
