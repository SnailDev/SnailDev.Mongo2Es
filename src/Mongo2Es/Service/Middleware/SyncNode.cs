using Mongo2Es.ElasticSearch;
using Mongo2Es.Mongo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mongo2Es.Middleware
{
    [Serializable]
    [BsonIgnoreExtraElements]
    public class SyncNode
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ID { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// MongoDB 节点
        /// </summary>
        public string MongoUrl { get; set; }

        /// <summary>
        /// 父数据库
        /// </summary>
        public string ParentDataBase { get; set; }

        /// <summary>
        /// 父集合
        /// </summary>
        public string ParentCollection { get; set; }

        /// <summary>
        /// 父集合链接字段
        /// </summary>
        public string ParentLinkField { get; set; }

        /// <summary>
        /// 当前数据库
        /// </summary>
        public string DataBase { get; set; }

        /// <summary>
        /// 当前集合
        /// </summary>
        public string Collection { get; set; }

        /// <summary>
        /// 与父集合对应的链接字段
        /// </summary>
        public string LinkField { get; set; }

        /// <summary>
        /// 需要同步的字段
        /// </summary>
        public string ProjectFields { get; set; }

        /// <summary>
        /// Es节点
        /// </summary>
        public string EsUrl { get; set; }

        /// <summary>
        /// 对应的索引
        /// </summary>
        public string Index { get; set; }

        /// <summary>
        /// 对应的分组
        /// </summary>
        public string Type { get; set; }

        ///// <summary>
        ///// 创建人
        ///// </summary>
        //public string Creator { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateTime { get; set; }

        ///// <summary>
        ///// 更新人
        ///// </summary>
        //public string Updator { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 操作方式 0|
        /// </summary>
        public int OperType { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? OperTime { get; set; }

        public SyncNode()
        {
            CreateTime = DateTime.Now;
        }
    }
}
