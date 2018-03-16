using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mongo2Es.Mongo
{
    public class MongoClient
    {
        public string Url;
        private MongoDB.Driver.MongoClient client;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="url"></param>
        public MongoClient(string url)
        {
            this.Url = url;
            this.client = new MongoDB.Driver.MongoClient(url);
        }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="configuration"></param>
        public MongoClient(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            this.Url = configuration["mongo"] ?? "mongodb://localhost:27017";
            this.client = new MongoDB.Driver.MongoClient(this.Url);
        }

        /// <summary>
        /// 获取DB集合
        /// </summary>
        public IEnumerable<BsonDocument> ListDataBases()
        {
            return client.ListDatabases().ToEnumerable();
        }

        /// <summary>
        /// 获取Collection集合
        /// </summary>
        /// <param name="dbName"></param>
        public IEnumerable<string> ListCollections(string dbName)
        {
            // db.ListCollections() 内部将 ReadPreference定死为 Primary , 故没有使用

            var db = client.GetDatabase(dbName);
            var doc = db.RunCommand<BsonDocument>(BsonDocument.Parse("{'listCollections': 1}"), ReadPreference.SecondaryPreferred);
            var enumerator = doc["cursor"]["firstBatch"].AsBsonArray.GetEnumerator();
            var colNames = new List<string>();

            while (enumerator.MoveNext())
            {
                colNames.Add(enumerator.Current["name"].AsString);
            }

            return colNames;
        }

        /// <summary>
        /// 获取Collection字段列表
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public IEnumerable<string> ListFields(string dbName, string collectionName)
        {
            var col = GetCollection<BsonDocument>(dbName, collectionName);
            var doc = col.Find(BsonDocument.Parse("{}")).Sort(BsonDocument.Parse("{'_id':-1}")).Limit(1).ToCursor();

            return doc.FirstOrDefault().Names;
        }

        /// <summary>
        /// 获取指定Collection
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        private IMongoCollection<TDocument> GetCollection<TDocument>(string dbName, string collectionName)
        {
            //MongoDatabaseSettings dbSettings = new MongoDatabaseSettings
            //{
            //    ReadPreference = ReadPreference.SecondaryPreferred
            //};
            var db = client.GetDatabase(dbName/*, dbSettings*/);

            MongoCollectionSettings colSettings = new MongoCollectionSettings
            {
                ReadPreference = ReadPreference.SecondaryPreferred
            };
            return db.GetCollection<TDocument>(collectionName, colSettings);
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="collectionName"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IEnumerable<TDocument> GetCollectionData<TDocument>(string dbName, string collectionName, string filter = "{}", int? limit = null)
        {
            var col = GetCollection<TDocument>(dbName, collectionName);
            return col.Find(BsonDocument.Parse(filter)).Limit(limit).ToEnumerable();
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="collectionName"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public void InsertCollectionData<TDocument>(string dbName, string collectionName, TDocument doc)
        {
            var col = GetCollection<TDocument>(dbName, collectionName);
            col.InsertOne(doc);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="collectionName"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public UpdateResult UpdateCollectionData<TDocument>(string dbName, string collectionName, TDocument doc)
        {
            var _doc = doc.ToBsonDocument();
            var _id = _doc["_id"].ToString();
            _doc.Remove("_id");

            _doc.Remove("CreateTime");

            var col = GetCollection<TDocument>(dbName, collectionName);
            return col.UpdateOne(BsonDocument.Parse($"{{'_id':new ObjectId('{_id}')}}"), new UpdateDocument("$set", _doc));
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="collectionName"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public DeleteResult DeleteCollectionData<TDocument>(string dbName, string collectionName, string id)
        {
            var col = GetCollection<TDocument>(dbName, collectionName);
            return col.DeleteOne(BsonDocument.Parse($"{{'_id':new ObjectId('{id}')}}"));
        }

        /// <summary>
        /// 获取Oplogs
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="collectionName"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public IEnumerable<BsonDocument> GetMongoOpLogs(string dbName, string collectionName, BsonTimestamp timestamp = null)
        {
            var db = client.GetDatabase("local");
            var collection = db.GetCollection<BsonDocument>("oplog.rs");

            var ns = $"{dbName}.{collectionName}";
            var op = BsonDocument.Parse($"{{$in: ['i','u','d']}}");
            timestamp = timestamp ?? new BsonTimestamp((int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds + 18000000);
            var ts = BsonDocument.Parse($"{{$gt: { timestamp } }}");
            var filterFunc = BsonDocument.Parse($"{{'ns':'{ns}','op':{op},'ts':{ts}}}");
            var sortFunc = BsonDocument.Parse("{$natural: 1}");

            return collection.Find(filterFunc).Sort(sortFunc).ToEnumerable();
        }

        /// <summary>
        /// 获取Oplogs
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public IEnumerable<BsonDocument> GetMongoOpLogs(string ns, long? timestamp = null)
        {
            var db = client.GetDatabase("local");
            var collection = db.GetCollection<BsonDocument>("oplog.rs");

            var op = BsonDocument.Parse($"{{$in: ['i','u','d']}}");
            timestamp = timestamp ?? GetTimestampFromDateTime(DateTime.UtcNow);
            var ts = BsonDocument.Parse($"{{$gt: new Timestamp({timestamp},1)}}");
            var filterFunc = BsonDocument.Parse($"{{'ns':{ns},'op':{op},'ts':{ts}}}");
            var sortFunc = BsonDocument.Parse("{$natural: 1}");

            return collection.Find(filterFunc).Sort(sortFunc).ToEnumerable();
        }

        /// <summary>
        /// 获取Bson时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public long GetTimestampFromDateTime(DateTime time)
        {
            return (long)(time - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }
    }
}
