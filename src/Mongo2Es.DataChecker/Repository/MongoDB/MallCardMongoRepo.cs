using MongoDB.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mongo2Es.DataChecker
{
    public class MallCardMongoRepo : MongoReaderRepository<MallCard, long>
    {
        private static readonly string connString = Contansts.ConnectString;
        private static readonly string dbName = "SP_UserCenter";
        private static readonly MongoSequence sequence = new MongoSequence();

        public MallCardMongoRepo()
        : base(connString, dbName, null, null, null, sequence)
        {

        }
    }
}
