using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Repository.IEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mongo2Es.DataChecker
{
    /// <summary>
    /// 卡券信息实体
    /// </summary>
    [BsonIgnoreExtraElements]
    public class CardVoucherInfo : IAutoIncr<long>
    {
        /// <summary>
        /// 卡券ID
        /// </summary>
        [BsonId]
        public long ID { get; set; }

    }
}
