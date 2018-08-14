using MongoDB.Bson.Serialization.Attributes;
using Repository.IEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mongo2Es.DataChecker
{
    [Serializable]
    [BsonIgnoreExtraElements]
    public class MallCard : IEntity<long>
    {
        [BsonId]
        public long ID { get; set; }

        public long MallID { get; set; }
    }
}
