using Mongo2Es.ElasticSearch;
using Mongo2Es.Mongo;
using MongoDB.Bson;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Mongo2Es.Middleware
{
    public class SyncClient
    {
        private MongoClient client;
        private System.Timers.Timer syncTimer;
        private System.Timers.Timer nodesRefreshTimer;
        private ConcurrentDictionary<string, EsClient> esClientDic = new ConcurrentDictionary<string, EsClient>();
        private ConcurrentDictionary<string, List<SyncNode>> nodeGroupsDic = new ConcurrentDictionary<string, List<SyncNode>>();
        private ConcurrentDictionary<string, MongoClient> mongoClientDic = new ConcurrentDictionary<string, MongoClient>();

        public SyncClient(string mongoUrl)
        {
            this.client = new MongoClient(mongoUrl);
        }

        public void Run()
        {
            #region nodesRefreshTimer
            nodesRefreshTimer = new System.Timers.Timer
            {
                Interval = 10// 5 * 60 * 1000 //; 1
            };
            nodesRefreshTimer.Elapsed += (sender, args) => GetNodes();
            nodesRefreshTimer.Disposed += (sender, args) =>
            {
                Console.WriteLine("nodes更新线程退出");
            };
            nodesRefreshTimer.Start();
            #endregion

            #region syncTimer
            //syncTimer = new System.Timers.Timer
            //{
            //    Interval = 5 * 1000 //; 1
            //};
            //syncTimer.Elapsed += (sender, args) =>
            //{
            //    AllocateTask();
            //};

            //syncTimer.Disposed += (sender, args) =>
            //{
            //    Console.WriteLine("同步更新线程退出");
            //};
            //syncTimer.Start();
            #endregion
        }

        private void GetNodes()
        {
            // nodesRefreshTimer.Interval =  5 * 60 * 1000;

            var nodes = client.GetCollectionData<SyncNode>("Mongo2Es", "SyncNodes");

            var group = nodes.GroupBy(x => x.MongoUrl);
            foreach (var item in group)
            {
                mongoClientDic.GetOrAdd(item.Key, new MongoClient(item.Key));

                var nodesInGroup = item.ToList<SyncNode>();
                foreach (var node in nodesInGroup)
                {
                    esClientDic.GetOrAdd(node.EsUrl, new EsClient(node.EsUrl));
                }

                nodeGroupsDic.AddOrUpdate(item.Key, nodesInGroup, (key, oldValue) => oldValue = nodesInGroup);
            }
        }

        private void AllocateTask()
        {
            //syncTimer.Interval = 5 * 1000;

            foreach (var client in mongoClientDic)
            {
                //ThreadPool.QueueUserWorkItem(ExcuteProcess, client.Value);
                ExcuteProcess(client.Value); // 单线程

                //Thread thread = new Thread(()=> ExcuteProcess(client.Value));
                //thread.Name = client.Key;
            }
        }

        private void ExcuteProcess(object client)
        {
            var mongoClient = client as MongoClient;
            if (nodeGroupsDic.ContainsKey(mongoClient.Url))
            {
                var nodes = nodeGroupsDic[mongoClient.Url];
                var opLogs = mongoClient.GetMongoOpLogs(mongoClient.GetTimestampFromDateTime(DateTime.UtcNow.AddDays(-1)));

                foreach (var node in nodes)
                {
                    var esClient = esClientDic[node.EsUrl];
                    var nodeOplogs = opLogs.Where(x => x["ns"] == $"{node.DataBase}.{node.Collection}");

                    foreach (var opLog in nodeOplogs)
                    {
                        switch (opLog["op"].AsString)
                        {
                            case "i":
                                var iid = opLog["o"]["_id"].ToString();
                                var idoc = IDocuemntHandle(opLog["o"].AsBsonDocument, node.ProjectFields);
                                if (idoc.Names.Count() > 0 && esClient.InsertDocument(node.Index, node.Type, iid, idoc))
                                    Console.WriteLine("文档写入ES成功");
                                else
                                    Console.WriteLine("文档写入ES失败");
                                break;
                            case "u":
                                var uid = opLog["o2"]["_id"].ToString();
                                var udoc = UDocuemntHandle(opLog["o"].AsBsonDocument, node.ProjectFields);
                                if (udoc.Names.Count() > 0 && esClient.UpdateDocument(node.Index, node.Type, uid, udoc))
                                    Console.WriteLine("文档更新ES成功");
                                else
                                    Console.WriteLine("文档更新ES失败");
                                break;
                            case "d":
                                var did = opLog["o"]["_id"].ToString();
                                if (esClient.DeleteDocument(node.Index, node.Type, did))
                                    Console.WriteLine("文档删除ES成功");
                                else
                                    Console.WriteLine("文档删除ES失败");
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        #region Doc Handle
        /// <summary>
        /// 插入文档处理
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="projectFields"></param>
        /// <returns></returns>
        private BsonDocument IDocuemntHandle(BsonDocument doc, string projectFields)
        {
            var fieldsArr = projectFields.Split(",");
            var names = doc.Names.ToList();
            foreach (var name in names)
            {
                if (!fieldsArr.Contains(name))
                    doc.Remove(name);
            }

            return doc;
        }

        /// <summary>
        /// 批量插入文档处理
        /// </summary>
        /// <param name="docs"></param>
        /// <param name="projectFields"></param>
        /// <returns></returns>
        private IEnumerable<BsonDocument> IBatchDocuemntHandle(IEnumerable<BsonDocument> docs, string projectFields)
        {
            var handDocs = new List<BsonDocument>();
            var fieldsArr = projectFields.Split(",");
            foreach (var doc in docs)
            {
                handDocs.Add(BsonDocument.Parse($"{{'index':{BsonDocument.Parse($"{{'_id':{doc["_id"].ToString()}}}")}}}"));

                var names = doc.Names.ToList();
                foreach (var name in names)
                {
                    if (!fieldsArr.Contains(name))
                        doc.Remove(name);
                }

                handDocs.Add(doc);
            }


            return handDocs;
        }

        /// <summary>
        /// 更新文档处理
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="projectFields"></param>
        /// <returns></returns>
        private BsonDocument UDocuemntHandle(BsonDocument doc, string projectFields)
        {
            if (doc.Contains("$set"))
            {
                doc = doc["$set"].AsBsonDocument;
            }

            var fieldsArr = projectFields.Split(",");
            var names = doc.Names.ToList();
            foreach (var name in names)
            {
                if (!fieldsArr.Contains(name))
                    doc.Remove(name);
            }

            return doc;
        }
        #endregion
    }
}
