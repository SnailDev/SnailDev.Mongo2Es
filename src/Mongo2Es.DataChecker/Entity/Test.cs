using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Repository.IEntity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mongo2Es.DataChecker
{
    [Serializable]
    [BsonIgnoreExtraElements]
    public class Test : IEntity<string>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ID { get; set; }

        public double Age { get; set; }

        public double Haha { get; set; }

        public string Role { get; set; }
    }
}
