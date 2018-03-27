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
    public class User : IEntity<long>
    {
        [BsonId]
        public long ID { get; set; }

        public double Age { get; set; }

        public double Sex { get; set; }

        public string Like { get; set; }
    }
}
