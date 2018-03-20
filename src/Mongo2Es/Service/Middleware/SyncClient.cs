using Mongo2Es.ElasticSearch;
using Mongo2Es.Log;
using Mongo2Es.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog;
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
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Mongo.MongoClient client;
        private readonly string database = "Mongo2Es";
        private readonly string collection = "SyncNode";
        private System.Timers.Timer nodesRefreshTimer;
        private ConcurrentDictionary<string, SyncNode> tailNodesDic = new ConcurrentDictionary<string, SyncNode>();

        public SyncClient(string mongoUrl)
        {
            this.client = new Mongo.MongoClient(mongoUrl);
        }

        public void Run()
        {
            nodesRefreshTimer = new System.Timers.Timer
            {
                Interval = 30 * 1000
            };
            nodesRefreshTimer.Elapsed += (sender, args) =>
            {
                var nodes = client.GetCollectionData<SyncNode>(database, collection);

                var scanNodes = nodes.Where(x => x.Status == SyncStatus.WaitForScan && x.Switch != SyncSwitch.Stop);
                foreach (var node in scanNodes)
                {
                    ThreadPool.QueueUserWorkItem(ExcuteScanProcess, node);
                }

                var tailNodes = nodes.Where(x => x.Status == SyncStatus.WaitForTail && x.Switch != SyncSwitch.Stop);
                var tailIds = tailNodes.Select(x => x.ID);

                foreach (var item in tailNodes)
                {
                    if (!tailNodesDic.ContainsKey(item.ID))
                    {
                        ThreadPool.QueueUserWorkItem(ExcuteTailProcess, item);
                    }

                    tailNodesDic.AddOrUpdate(item.ID, item, (key, oldValue) => oldValue = item);
                }

                foreach (var key in tailNodesDic.Keys.Except(tailIds))
                {
                    tailNodesDic.Remove(key, out SyncNode node);
                }
            };
            nodesRefreshTimer.Disposed += (sender, args) =>
            {
                logger.Info("nodes更新线程退出");
            };
            nodesRefreshTimer.Start();
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
                        LogUtil.LogInfo(logger, $"文档(count:{data.Count()})写入ES成功", node.ID);

                        node.Status = SyncStatus.ProcessScan;
                        node.OperScanSign = data.Last()["_id"].ToString();
                        node.OperTailSign = client.GetTimestampFromDateTime(DateTime.UtcNow);
                        client.UpdateCollectionData<SyncNode>(database, collection, node);

                        filter = $"{{'_id':{{ $gt:new ObjectId('{node.OperScanSign}')}}}}";
                        data = mongoClient.GetCollectionData<BsonDocument>(node.DataBase, node.Collection, filter, 1000);
                    }
                    else
                    {
                        LogUtil.LogInfo(logger, $"文档(count:{data.Count()})写入ES失败,需手动重置", node.ID);
                        node.Status = SyncStatus.ScanException;
                        node.Switch = SyncSwitch.Stop;
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
                node.Switch = SyncSwitch.Stop;
                client.UpdateCollectionData<SyncNode>(database, collection, node);

                LogUtil.LogError(logger, ex.ToString(), node.ID);
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

                while (true)
                {
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
                                        LogUtil.LogInfo(logger, $"文档（id:{iid}）写入ES成功", node.ID);
                                    else
                                        LogUtil.LogInfo(logger, $"文档（id:{iid}）写入ES失败", node.ID);
                                    break;
                                case "u":
                                    var uid = opLog["o2"]["_id"].ToString();
                                    var udoc = UDocuemntHandle(opLog["o"].AsBsonDocument, node.ProjectFields);
                                    if (udoc.Names.Count() > 0 && esClient.UpdateDocument(node.Index, node.Type, uid, udoc))
                                        LogUtil.LogInfo(logger, $"文档（id:{uid}）更新ES成功", node.ID);
                                    else
                                        LogUtil.LogInfo(logger, $"文档（id:{uid}）更新ES失败", node.ID);
                                    break;
                                case "d":
                                    var did = opLog["o"]["_id"].ToString();
                                    if (esClient.DeleteDocument(node.Index, node.Type, did))
                                        LogUtil.LogInfo(logger, $"文档（id:{did}）删除ES成功", node.ID);
                                    else
                                        LogUtil.LogInfo(logger, $"文档（id:{did}）删除ES失败", node.ID);
                                    break;
                                default:
                                    break;
                            }

                            node.OperTailSign = opLog["ts"].AsBsonTimestamp.Timestamp;
                            client.UpdateCollectionData<SyncNode>(database, collection, node);
                        }

                        if (!tailNodesDic.TryGetValue(node.ID, out node))
                        {
                            LogUtil.LogInfo(logger, $"同步节点({node.Name})已删除, tail线程停止", node.ID);
                            break;
                        }

                        if (node.Switch == SyncSwitch.Stoping)
                        {
                            node.Switch = SyncSwitch.Stop;
                            client.UpdateCollectionData<SyncNode>(database, collection, node);
                            LogUtil.LogInfo(logger, $"同步节点({node.Name})已停止, tail线程停止", node.ID);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                node.Status = SyncStatus.TailException;
                node.Switch = SyncSwitch.Stop;
                client.UpdateCollectionData<SyncNode>(database, collection, node);

                LogUtil.LogError(logger, $"同步({node.Name})节点异常：{ex}", node.ID);
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
