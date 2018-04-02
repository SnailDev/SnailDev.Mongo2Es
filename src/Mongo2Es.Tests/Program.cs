using MongoDB.Bson;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

            //// 这种延迟方法会占用cpu
            //DateTime startTime = DateTime.Now;
            //while ((DateTime.Now - startTime).TotalSeconds < 10) { Console.WriteLine("Hello"); }

            //var bson = BsonDocument.Parse(@"{}");
            //string project = "";
            //var doc = HandleDoc(bson, project);

            Console.ReadLine();
        }

        static void Test()
        {
            //var client = new Mongo.MongoClient("mongodb://192.168.110.6:27000/?slaveOk=true");
            //var esClient = new Mongo2Es.ElasticSearch.EsClient("http://localhost:9200");
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

            //var nodes = new List<Middleware.SyncNode>()
            //    {
            //        new Middleware.SyncNode()
            //        {
            //            ID = "1",
            //            MongoUrl = "mongodb://192.168.110.6:27000/?slaveOk=true",
            //            DataBase = "test",
            //            Collection = "xiaoming",
            //            ProjectFields = "Haha,haha,age", //mark
            //            EsUrl = "http://localhost:9200",
            //            Index =  "test.xiaoming1",
            //            Type =  "test.xiaoming"
            //        },
            //        new Middleware.SyncNode()
            //        {
            //            ID = "2",
            //            MongoUrl = "mongodb://192.168.110.6:27001/?slaveOk=true",
            //            DataBase = "test",
            //            Collection = "xiaoming",
            //            ProjectFields = "Haha,haha,age", //mark
            //            EsUrl = "http://localhost:9200",
            //            Index =  "test.xiaoming2",
            //            Type =  "test.xiaoming"
            //        }
            //    };
        }

        /// <summary>
        /// 处理id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        static BsonValue HandleID(BsonValue id)
        {
            return id.IsObjectId ? id.ToString() : id;
        }

        /// <summary>
        /// 处理文档（key转小写）
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="projectFields"></param>
        /// <returns></returns>
        static BsonDocument HandleDoc(BsonDocument doc, string projectFields)
        {
            var fieldsArr = projectFields.Split(",").ToList().ConvertAll(x => x.Split('.')[0]);
            var subProjectFields = string.Join(',', projectFields.Split(",").ToList().ConvertAll(x => x.Contains(".") ? x.Substring(x.IndexOf(".") + 1) : null));
            var names = doc.Names.ToList();

            BsonDocument newDoc = new BsonDocument();
            if (doc.Contains("_id"))
            {
                newDoc.Add(new BsonElement("id", HandleID(doc["_id"])));
            }

            foreach (var name in names)
            {
                if (fieldsArr.Contains(name))
                {
                    if (doc[name].IsBsonArray)
                    {
                        newDoc.AddRange(new BsonDocument(name.ToLower(), HandleDocs(doc[name].AsBsonArray, subProjectFields)));
                    }
                    else if (doc[name].IsBsonDocument)
                    {
                        newDoc.AddRange(new BsonDocument(name.ToLower(), HandleDoc(doc[name].AsBsonDocument, subProjectFields)));
                    }
                    else
                    {
                        newDoc.Add(new BsonElement(name.ToLower(), doc[name]));
                    }
                }
            }

            return newDoc;
        }

        /// <summary>
        /// 处理文档（key转小写）
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="projectFields"></param>
        /// <returns></returns>
        static BsonArray HandleDocs(BsonArray docs, string projectFields)
        {
            BsonArray newDoc = new BsonArray();
            foreach (var doc in docs)
            {
                if (doc.IsBsonArray)
                {
                    newDoc.Add(HandleDocs(doc.AsBsonArray, projectFields));
                }
                else if (doc.IsBsonDocument)
                {
                    newDoc.Add(HandleDoc(doc.AsBsonDocument, projectFields));
                }
                else
                {
                    newDoc.AddRange(docs);
                    break;
                }
            }

            return newDoc;
        }
    }
}
