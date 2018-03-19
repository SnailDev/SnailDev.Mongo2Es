using Mongo2Es.ElasticSearch;
using Mongo2Es.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;
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
        private Mongo.MongoClient client;
        private readonly string database = "Mongo2Es";
        private readonly string collection = "SyncNode";
        //private IEnumerable<SyncNode> nodes;
        //private System.Timers.Timer syncTimer;
        private System.Timers.Timer nodesRefreshTimer;
        private ConcurrentDictionary<string, SyncNode> tailNodesDic = new ConcurrentDictionary<string, SyncNode>();
        //private ConcurrentDictionary<string, EsClient> esClientDic = new ConcurrentDictionary<string, EsClient>();
        //private ConcurrentDictionary<string, MongoClient> mongoClientDic = new ConcurrentDictionary<string, MongoClient>();

        public SyncClient(string mongoUrl)
        {
            this.client = new Mongo.MongoClient(mongoUrl);
        }

        public void Run()
        {
            #region nodesRefreshTimer
            nodesRefreshTimer = new System.Timers.Timer
            {
                Interval = 10 * 1000 //; 1
            };
            nodesRefreshTimer.Elapsed += (sender, args) => AllocateTask();
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

            //// 分配工作
            //AllocateTask();
        }

        private void RefreshNodes()
        {
            // nodesRefreshTimer.Interval =  5 * 60 * 1000;

            //nodes = client.GetCollectionData<SyncNode>("Mongo2Es", "SyncNodes");

            //var group = nodes.GroupBy(x => x.MongoUrl);
            //foreach (var item in group)
            //{
            //    mongoClientDic.GetOrAdd(item.Key, new Mongo.MongoClient(item.Key));

            //    var nodesInGroup = item.ToList<SyncNode>();
            //    foreach (var node in nodesInGroup)
            //    {
            //        esClientDic.GetOrAdd(node.EsUrl, new EsClient(node.EsUrl));
            //    }
            //}
        }

        /// <summary>
        /// 分配任务
        /// </summary>
        private void AllocateTask()
        {
            var nodes = client.GetCollectionData<SyncNode>(database, collection);

            var scanNodes = nodes.Where(x => x.Status == SyncStatus.WaitForScan);
            foreach (var node in scanNodes)
            {
                ThreadPool.QueueUserWorkItem(ExcuteScanProcess, node);
            }

            var tailNodes = nodes.Where(x => x.Status == SyncStatus.WaitForTail);
            foreach (var item in tailNodes)
            {
                if (!tailNodesDic.ContainsKey(item.ID))
                {
                    ThreadPool.QueueUserWorkItem(ExcuteTailProcess, item);
                }

                tailNodesDic.AddOrUpdate(item.ID, item, (key, oldValue) => oldValue = item);
            }
        }

        /// <summary>
        /// 全表同步
        /// </summary>
        /// <param name="node"></param>
        private void ExcuteScanProcess(object obj)
        {
            var node = obj as SyncNode;
            var mongoClient = new Mongo.MongoClient(node.MongoUrl);
            var esClient = new EsClient(node.EsUrl);

            try
            {
                var filter = "{}";
                var data = mongoClient.GetCollectionData<BsonDocument>(node.DataBase, node.Collection, filter, 1);
                while (data.Count() > 0)
                {
                    if (esClient.InsertBatchDocument(node.Index, node.Type, IBatchDocuemntHandle(data, node.ProjectFields)))
                    {
                        Console.WriteLine("文档写入ES成功");

                        node.Status = SyncStatus.ProcessScan;
                        node.OperScanSign = data.Last()["_id"].ToString();
                        node.OperTailSign = client.GetTimestampFromDateTime(DateTime.UtcNow);
                        client.UpdateCollectionData<SyncNode>(database, collection, node);

                        filter = $"{{'_id':{{ $gt:new ObjectId('{node.OperScanSign}')}}}}";
                        data = mongoClient.GetCollectionData<BsonDocument>(node.DataBase, node.Collection, filter, 1000);
                    }
                    else
                    {
                        Console.WriteLine("文档写入ES失败");
                        node.Status = SyncStatus.ScanException;
                        client.UpdateCollectionData<SyncNode>(database, collection, node);
                        return;
                    }
                }

                node.Status = SyncStatus.WaitForTail;
                client.UpdateCollectionData<SyncNode>(database, collection, node);
            }
            catch (Exception ex)
            {
                node.Status = SyncStatus.ScanException;
                client.UpdateCollectionData<SyncNode>(database, collection, node);

                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// 增量同步
        /// </summary>
        /// <param name="obj"></param>
        private void ExcuteTailProcess(object obj)
        {
            var node = obj as SyncNode;
            var mongoClient = new Mongo.MongoClient(node.MongoUrl);
            var esClient = new EsClient(node.EsUrl);

            try
            {
                node.Status = SyncStatus.ProcessTail;
                client.UpdateCollectionData<SyncNode>(database, collection, node);

                while(true)
                {
                    if (!tailNodesDic.ContainsKey(node.ID))
                    {
                        break;
                    }

                    using (var cursor = mongoClient.TailMongoOpLogs(node.OperTailSign))
                    {
                        foreach (var opLog in cursor.ToEnumerable())
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

                            node.OperTailSign = opLog["ts"].AsBsonTimestamp.Value;
                            client.UpdateCollectionData<SyncNode>(database, collection, node);
                        }
                    }
                }

                #region 优化
                //while (opLogs.Count() > 0)
                //{
                //    var iopLogs = opLogs.Where(x => x["op"].AsString == "i");
                //    if (iopLogs.Count() > 0)
                //    {
                //        if (esClient.InsertBatchDocument(node.Index, node.Type, IBatchDocuemntHandle(iopLogs, node.ProjectFields)))
                //        {
                //            Console.WriteLine("文档写入ES成功");
                //        }
                //        else
                //        {
                //            Console.WriteLine("文档写入ES失败");
                //            node.Status = SyncStatus.TailException;
                //            client.UpdateCollectionData<SyncNode>(database, collection, node);
                //            return;
                //        }
                //    }

                //    var dopLogs = opLogs.Where(x => x["op"].AsString == "d");

                //    var uopLogs = opLogs.Where(x => x["op"].AsString == "u");
                //    foreach (var log in uopLogs)
                //    {

                //    }
                //} 
                #endregion         
            }
            catch (Exception ex)
            {
                node.Status = SyncStatus.TailException;
                client.UpdateCollectionData<SyncNode>(database, collection, node);

                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// 增量同步
        /// </summary>
        /// <param name="obj"></param>
        private void ExcuteTailProcessOther(object obj)
        {
            var node = obj as SyncNode;
            var mongoClient = new Mongo.MongoClient(node.MongoUrl);
            var esClient = new EsClient(node.EsUrl);

            try
            {
                var opLogs = mongoClient.GetMongoOpLogs($"'{node.DataBase}.{node.Collection}'", node.OperTailSign);
                node.Status = SyncStatus.ProcessTail;
                node.OperTailSign = client.GetTimestampFromDateTime(DateTime.UtcNow);
                client.UpdateCollectionData<SyncNode>(database, collection, node);

                #region 优化
                //while (opLogs.Count() > 0)
                //{
                //    var iopLogs = opLogs.Where(x => x["op"].AsString == "i");
                //    if (iopLogs.Count() > 0)
                //    {
                //        if (esClient.InsertBatchDocument(node.Index, node.Type, IBatchDocuemntHandle(iopLogs, node.ProjectFields)))
                //        {
                //            Console.WriteLine("文档写入ES成功");
                //        }
                //        else
                //        {
                //            Console.WriteLine("文档写入ES失败");
                //            node.Status = SyncStatus.TailException;
                //            client.UpdateCollectionData<SyncNode>(database, collection, node);
                //            return;
                //        }
                //    }

                //    var dopLogs = opLogs.Where(x => x["op"].AsString == "d");

                //    var uopLogs = opLogs.Where(x => x["op"].AsString == "u");
                //    foreach (var log in uopLogs)
                //    {

                //    }
                //} 
                #endregion

                foreach (var opLog in opLogs)
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

                node.Status = SyncStatus.WaitForTail;

                client.UpdateCollectionData<SyncNode>(database, collection, node);
            }
            catch (Exception ex)
            {
                node.Status = SyncStatus.TailException;
                client.UpdateCollectionData<SyncNode>(database, collection, node);

                Console.WriteLine(ex);
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
        /// 插入文档处理
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="projectFields"></param>
        /// <returns></returns>
        private List<string> IDocuemntHandle(IEnumerable<BsonDocument> docs, string projectFields)
        {
            var handDocs = new List<string>();
            var fieldsArr = projectFields.Split(",");
            foreach (var doc in docs)
            {
                var _doc = doc["o"].AsBsonDocument;
                handDocs.Add(new
                {
                    index = new
                    {
                        _id = _doc["_id"].ToString()
                    }
                }.ToJson());

                var names = _doc.Names.ToList();
                foreach (var name in names)
                {
                    if (!fieldsArr.Contains(name))
                        _doc.Remove(name);
                }

                if (_doc.Names.Count() > 0)
                    handDocs.Add(_doc.ToJson());
            }


            return handDocs;
        }

        /// <summary>
        /// 批量插入文档处理
        /// </summary>
        /// <param name="docs"></param>
        /// <param name="projectFields"></param>
        /// <returns></returns>
        private List<string> IBatchDocuemntHandle(IEnumerable<BsonDocument> docs, string projectFields)
        {
            var handDocs = new List<string>();
            var fieldsArr = projectFields.Split(",");
            foreach (var doc in docs)
            {
                handDocs.Add(new
                {
                    index = new
                    {
                        _id = doc["_id"].ToString()
                    }
                }.ToJson());

                var names = doc.Names.ToList();
                foreach (var name in names)
                {
                    if (!fieldsArr.Contains(name))
                        doc.Remove(name);
                }

                handDocs.Add(doc.ToJson());
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
