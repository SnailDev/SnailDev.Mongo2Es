using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mongo2Es.ElasticSearch
{
    [Serializable]
    [BsonIgnoreExtraElements]
    public class EsNode
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ID { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 链接
        /// </summary>
        public string Url { get; set; }

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

        public EsNode()
        {
            CreateTime = DateTime.Now;
        }
    }
}
