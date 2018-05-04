using MongoDB.Bson;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace Mongo2Es.Tests
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            //Console.WriteLine(Math.Round((decimal)2.135, 2, MidpointRounding.AwayFromZero));
            //string assemblyFolder = Directory.GetCurrentDirectory();
            //NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(Path.Combine(assemblyFolder, "NLog.config"), true);
            //logger.Info("Hello World");

            //// 这种延迟方法会占用cpu
            //DateTime startTime = DateTime.Now;
            //while ((DateTime.Now - startTime).TotalSeconds < 10) { Console.WriteLine("Hello"); }

            //var bson = BsonDocument.Parse(@"{}");
            //string project = "";
            //var doc = HandleDoc(bson, project);
            //// PostData(Encoding.Default.GetBytes("123"), Encoding.Default.GetBytes("123"));

            //var ac = new AC();
            //ac.AB = new List<AB>() { new AB() { Text = "123", Value = "123" }, new AB() { Text = "234", Value = "234" } };
            //Type type = ac.GetType();
            //object obj = Activator.CreateInstance(type);
            //PropertyInfo[] properties = ac.GetType().GetProperties();

            //foreach (var proper in properties)
            //{
            //    if (proper.PropertyType.IsGenericType && proper.PropertyType.GetInterface("IEnumerable", false) != null)
            //    {
            //        var listObj = proper.GetValue(ac, null) as IEnumerable<object>;
            //        foreach (object item in listObj)
            //        {
            //            // do reflex
            //        }
            //    }

            //}



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

        //public class AC
        //{
        //    public int ID { get; set; }
        //    public string Name { get; set; }
        //    public virtual List<AB> AB { get; set; }
        //}

        //public class AB
        //{
        //    public string Text { get; set; }
        //    public string Value { get; set; }
        //}


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

        private static int PostData(byte[] aesKeyEnc, byte[] jsonEnc)
        {
            string url = "http://test.openrcv.baidu.com/1017/agent.gif";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "text/plain; charset=UTF-8";
            request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            //request.Proxy = WebProxy.GetDefaultProxy();
            //request.Proxy = new WebProxy("127.0.0.1:8888", true);
            Stream outstream = request.GetRequestStream();

            //MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(outstream);
            writer.Write(WriterInt(0x484D3031));
            writer.Write(WriterInt(1));
            writer.Write(WriterInt(1017));
            writer.Write(WriterInt((long)0));
            writer.Write(WriterInt((short)2));
            writer.Write(WriterInt((short)1));
            writer.Write(WriterInt(0x484D3031));
            writer.Write(WriterInt(aesKeyEnc.Length));
            writer.Write(aesKeyEnc);
            writer.Write(WriterInt(jsonEnc.Length));
            writer.Write(jsonEnc);
            writer.Flush();
            writer.Close();
            // byte[] data = stream.ToArray();
            //outstream.Write(data, 0, data.Length);
            //outstream.Flush();
            //outstream.Close();

            HttpWebResponse res;
            try
            {
                res = request.GetResponse() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                res = (HttpWebResponse)ex.Response;
            }

            return (int)res.StatusCode;
        }

        public static byte[] WriterInt(dynamic value)
        {
            byte[] bs = BitConverter.GetBytes(value);
            Array.Reverse(bs);
            return bs;
        }

    }
}
