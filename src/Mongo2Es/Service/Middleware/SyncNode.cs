using Mongo2Es.ElasticSearch;
using Mongo2Es.Mongo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        /// 2|运行，1|停止中，0|停止
        /// </summary>
        public SyncSwitch Switch { get; set; }

        /// <summary>
        /// 同步状态
        /// </summary>
        public SyncStatus Status { get; set; }

        /// <summary>
        /// 操作方式 0|scan, 1|tail
        /// </summary>
        public int OperType { get; set; }

        /// <summary>
        /// 操作标记
        /// </summary>
        public string OperScanSign { get; set; }
        public long OperTailSign { get; set; }
        public int OperTailSignExt { get; set; }

        public SyncNode()
        {
            CreateTime = DateTime.Now;
            Status = SyncStatus.WaitForScan;
        }

        public override string ToString()
        {
            return this.ToJson();
        }
    }

    /// <summary>
    /// 同步状态
    /// </summary>
    public enum SyncStatus
    {
        /// <summary>
        /// 等待全表扫描
        /// </summary>
        [Description("等待全表扫描")]
        WaitForScan,

        /// <summary>
        /// 执行全表扫描
        /// </summary>
        [Description("执行全表扫描")]
        ProcessScan,

        /// <summary>
        /// 全部扫描异常
        /// </summary>
        [Description("全部扫描异常")]
        ScanException,

        /// <summary>
        /// 完成全表扫描
        /// </summary>
        [Description("完成全表扫描")]
        CompletedScan,

        /// <summary>
        /// 等待增量同步
        /// </summary>
        [Description("等待增量同步")]
        WaitForTail,

        /// <summary>
        /// 增量同步中
        /// </summary>
        [Description("增量同步中")]
        ProcessTail,

        /// <summary>
        /// 增量同步失败
        /// </summary>
        [Description("增量同步失败")]
        TailException,
    }

    /// <summary>
    /// 同步开关  2|运行，1|停止中，0|停止
    /// </summary>
    public enum SyncSwitch
    {
        /// <summary>
        /// 停止
        /// </summary>
        [Description("停止")]
        Stop = 0,

        /// <summary>
        /// 停止中
        /// </summary>
        [Description("停止中")]
        Stoping =1,

        /// <summary>
        /// 运行
        /// </summary>
        [Description("运行")]
        Run = 2
    }
}
