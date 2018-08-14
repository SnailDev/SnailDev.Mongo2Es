using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Repository.IEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;

namespace Mongo2Es.DataChecker
{
    //[Serializable]
    [BsonIgnoreExtraElements]
    //[DataContract]
    public class CouponLifeCycle : IEntity<string>
    {
        /// <summary>
        /// 自动增长列
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ID { get; set; }

        /// <summary>
        /// 商场ID
        /// </summary>
        public long MallID { get; set; }

        /// <summary>
        /// 商户ID（没值是0）
        /// 根据ChangeType来，比如核销时，此处是核销商户ID，撤销核销时，此处是核销方商户ID（即使撤销核销方是“商场”也一样）
        /// 注：商户核销的券只能被此商户或此商场撤销核销，不能被其它商户撤销核销
        /// </summary>
        public long ShopID { get; set; }

        /// <summary>
        /// 操作方
        /// 根据ChangeType来，比如核销时，此处是核销方，撤销核销时，此处是撤销核销方
        /// </summary>
        public OperationMethod OperationType { get; set; }

        /// <summary>
        /// 卡券规则ID（新增）
        /// </summary>
        //[DataMember(Name = "CVRID")]
        [JsonProperty("CVRID")]
        [BsonElement("CVRID")]
        public long CardVoucherRuleID { get; set; }

        /// <summary>
        /// 卡券ID
        /// </summary>
        //[DataMember(Name = "CVIID")]
        [JsonProperty("CVIID")]
        [BsonElement("CVIID")]
        public long CardVoucherInfoID { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// 之前的用户ID
        /// </summary>
        public long UserIDOld { get; set; }

        /// <summary>
        /// 撤销原因状态（1：其他;2：由于服务人员失误，多核销了一张；3：核销错了，应该核销其他的券；）
        /// </summary>
        public List<CancelType> CancelTypeList { get; set; }

        /// <summary>
        /// 撤销核销原因描述
        /// </summary>
        public string CancelDesc { get; set; }

        /// <summary>
        /// 变更类型 领取/核销/过期/撤销核销/退回/冻结/删除
        /// </summary>
        public CouponChangeType ChangeType { get; set; }

        /// <summary>
        /// 变更时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime ChangeTime { get; set; }

        /// <summary>
        /// 变更渠道
        /// </summary>
        public CouponChangeChannelV2 ChangeChannel { get; set; }

        /// <summary>
        /// 变更人ID
        /// </summary>
        public long ChangePeopleID { get; set; }

        /// <summary>
        /// 变更人
        /// </summary>
        public string ChangePeople { get; set; }

        /// <summary>
        /// 变更平台 Web/IOS/Adnroid
        /// </summary>
        public CouponChangePlatformV2 ChangePlatform { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public CouponLifeCycle()
        {
            ShopID = 0;
            OperationType = OperationMethod.None;
        }
    }

    /// <summary>
    /// 撤销核销原因的类型
    /// </summary>
    [Description("撤销核销原因的类型")]
    public enum CancelType : int
    {
        /// <summary>
        /// 无意义的，防止某些序列化工具在序列化时报错
        /// </summary>
        [Description("无意义的，防止某些序列化工具在序列化时报错")]
        None = 0,

        /// <summary>
        /// 其他
        /// </summary>
        [Description("其他")]
        Orther = 1,

        /// <summary>
        /// 由于服务人员失误，多核销了一张
        /// </summary>
        [Description("由于服务人员失误，多核销了一张")]
        MoreThanOne = 2,

        /// <summary>
        /// 核销错了，应该核销其他的券
        /// </summary>
        [Description("核销错了，应该核销其他的券")]
        Fault = 3

    }

    /// <summary>
    /// 券变更类型
    /// </summary>
    [Description("券变更类型")]
    public enum CouponChangeType : int
    {
        /// <summary>
        /// 无意义的，防止某些序列化工具在序列化时报错
        /// </summary>
        [Description("无意义的，防止某些序列化工具在序列化时报错")]
        None = 0,

        /// <summary>
        /// 领取|1
        /// </summary>
        [Description("领取")]
        Receive = 1,

        /// <summary>
        /// 核销|2
        /// </summary>
        [Description("核销")]
        Use = 2,

        /// <summary>
        /// 过期|3
        /// </summary>
        [Description("过期")]
        Expire = 3,

        /// <summary>
        /// 撤销核销|4
        /// </summary>
        [Description("撤销核销")]
        CancelVerification = 4,

        /// <summary>
        /// 退券（领取撤销）|5
        /// </summary>
        [Description("退券（领取撤销）")]
        ReturnCoupon = 5,

        /// <summary>
        /// 冻结|6
        /// </summary>
        [Description("冻结")]
        Freezed = 6,

        /// <summary>
        /// 删除|7
        /// </summary>
        [Description("删除")]
        Deleted = 7,

        /// <summary>
        /// 转赠|8
        /// </summary>
        [Description("转赠")]
        Donation = 8,

        /// <summary>
        /// 恢复|9
        /// </summary>
        [Description("恢复")]
        Restore = 9,

        /// <summary>
        /// 合并|10
        /// </summary>
        [Description("合并")]
        Merge = 10,
    }

    /// <summary>
    /// 券变更渠道(渠道代表业务类型)
    /// </summary>
    [Description("券变更渠道")]
    public enum CouponChangeChannelV2 : int
    {
        /// <summary>
        /// 无意义的，防止某些序列化工具在序列化时报错
        /// </summary>
        [Description("无意义的，防止某些序列化工具在序列化时报错")]
        None = 0,

        /// <summary>
        /// 猫管家|1
        /// </summary>
        [Description("猫管家")]
        MallManager = 1,

        /// <summary>
        /// 后台|2
        /// </summary>
        [Description("后台")]
        MP = 2,

        /// <summary>
        /// 口令核销|3
        /// </summary>
        [Description("口令核销")]
        PassWord = 3,

        /// <summary>
        /// 会员系统|4
        /// </summary>
        [Description("会员系统")]
        UserSystem = 4,

        /// <summary>
        /// 停车|5
        /// </summary>
        [Description("停车")]
        Parking = 5,

        /// <summary>
        /// 开放平台|6
        /// </summary>
        [Description("开放平台")]
        OpenPlatform = 6,

        /// <summary>
        /// 团购|7
        /// </summary>
        [Description("团购")]
        Groupbuy = 7,

        /// <summary>
        /// C端用户|8
        /// </summary>
        [Description("C端用户")]
        ClientUser = 8,

        /// <summary>
        /// 活动|9
        /// </summary>
        [Description("活动")]
        Activity = 9,

        /// <summary>
        /// 优惠|10
        /// </summary>
        [Description("优惠")]
        Promotion = 10,

        /// <summary>
        /// 抽奖|11
        /// </summary>
        [Description("抽奖")]
        Lottery = 11,

        /// <summary>
        /// 签到|12
        /// </summary>
        [Description("签到")]
        CheckIn = 12,

        /// <summary>
        /// 打印发券|13
        /// </summary>
        [Description("打印发券")]
        PrintSendCoupon = 13,

        /// <summary>
        /// 后台发券|14
        /// </summary>
        [Description("后台发券")]
        MPSendCoupon = 14,

        /// <summary>
        /// 积分换礼|15
        /// </summary>
        [Description("积分换礼")]
        PointsForGift = 15,

        /// <summary>
        /// 问卷调查|16
        /// </summary>
        [Description("问卷调查")]
        Questionnaire = 16,

        /// <summary>
        /// 兑换码|17
        /// </summary>
        [Description("兑换码")]
        RedemptionCode = 17,

        /// <summary>
        /// 满赠|18
        /// </summary>
        [Description("满赠")]
        Incentive = 18,

        /// <summary>
        /// 盒子|19
        /// </summary>
        [Description("盒子")]
        MallcooShopBox = 19,

        /// <summary>
        /// 收单|20
        /// </summary>
        [Description("收单")]
        ReciveOrder = 20,

        /// <summary>
        /// 礼品卡促销
        /// </summary>
        [Description("礼品卡促销")]
        GiftCard = 21,

        /// <summary>
        /// 自动化营销 | 22
        /// </summary>
        [Description("自动化营销")]
        MarketingAutomation = 22,

        /// <summary>
        /// 运动云勋章 | 23
        /// </summary>
        [Description("运动云勋章")]
        MovementOfCloudMedal = 23,
    }

    /// <summary>
    /// 券变更平台
    /// </summary>
    [Description("券变更平台")]
    public enum CouponChangePlatformV2 : int
    {
        /// <summary>
        /// 无意义的，防止某些序列化工具在序列化时报错
        /// </summary>
        [Description("无意义的，防止某些序列化工具在序列化时报错")]
        None = 0,

        ///// <summary>
        ///// APP|1
        ///// </summary>
        //[Description("APP")]
        //APP = 1,

        /// <summary>
        /// Web端|2
        /// </summary>
        [Description("Web端")]
        Web = 2,

        /// <summary>
        /// 微信|3
        /// </summary>
        [Description("微信")]
        WiXin = 3,

        /// <summary>
        /// IOS|4
        /// </summary>
        [Description("IOS")]
        IOS = 4,

        /// <summary>
        /// Android|5
        /// </summary>
        [Description("Android")]
        Android = 5,

        /// <summary>
        /// 桌面程序|6
        /// </summary>
        [Description("桌面程序")]
        WinFrom = 6,

        /// <summary>
        /// 其它|7
        /// </summary>
        [Description("其它")]
        Other = 7,

        /// <summary>
        /// 支付宝App
        /// </summary>
        [Description("支付宝App")]
        AliPay = 8,
    }

    /// <summary>
    /// 券操作方
    /// </summary>
    public enum OperationMethod : int
    {
        /// <summary>
        /// 无意义的，防止某些序列化工具在序列化时报错
        /// </summary>
        [Description("无意义的，防止某些序列化工具在序列化时报错")]
        None = 0,

        ///// <summary>
        ///// 不用核销|1
        ///// </summary>
        //[Description("不用核销")]
        //NotVerification = 1,

        /// <summary>
        /// 商场|2
        /// </summary>
        [Description("商场")]
        Mall = 2,

        /// <summary>
        /// 商户|3
        /// </summary>
        [Description("商户")]
        Shop = 3,
    }

}
