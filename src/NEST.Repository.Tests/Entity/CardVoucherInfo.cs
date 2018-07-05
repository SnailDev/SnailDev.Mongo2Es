using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Repository.IEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Repository.Tests
{
    /// <summary>
    /// 卡券信息实体
    /// </summary>
    [BsonIgnoreExtraElements]
    public class CardVoucherInfo : IAutoIncr<long>
    {
        #region 常量

        /// <summary>
        /// 卡券ID
        /// </summary>
        public const string CVIID = "CVIID"; 

        #endregion
        
        /// <summary>
        /// 卡券ID
        /// </summary>
        [BsonId]
        public long ID { get; set; }

        /// <summary>
        /// 撤销核销次数
        /// </summary>
        public long CancelCount { get; set; }

        /// <summary>
        /// 卡券名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 卡券名称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 是否第三方券
        /// </summary>
        public bool? IsThirdVCode { get; set; }
       

        /// <summary>
        /// 渠道编号
        /// </summary>
        public string ChannelNo { get; set; }

        /// <summary>
        /// 商场ID
        /// </summary>
        public long MallID { get; set; }

        /// <summary>
        /// POS机ID【第三方核销的时候传入的】
        /// </summary>
        public string POSID { get; set; }

        /// <summary>
        /// 流水号【第三方核销的时候传入的】
        /// </summary>
        public string TradeSerialNo { get; set; }

        /// <summary>
        /// 订单编号【第三方核销的时候传入的】
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 第三方核销人名称【第三方核销的时候传入的】
        /// </summary>
        public string ThirdOperator { get; set; }

        /// <summary>
        /// 场次信息【场次券专用】
        /// </summary>
        public string EntranceVoucherInfo { get; set; }


        /// <summary>
        /// 默认构造函数
        /// </summary>
        public CardVoucherInfo()
        {

        }
    }
}
