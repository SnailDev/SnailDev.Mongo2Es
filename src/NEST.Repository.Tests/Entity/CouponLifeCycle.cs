using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Repository.IEntity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NEST.Repository.Tests
{
    [Serializable]
    [BsonIgnoreExtraElements]
    public class CouponLifeCycle: IEntity<string>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ID { get; set; }

        public long MallID { get; set; }
    }
}
