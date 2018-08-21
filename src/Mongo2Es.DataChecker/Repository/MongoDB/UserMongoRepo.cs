using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Repository;

namespace Mongo2Es.DataChecker
{
    public class UserMongoRepo : MongoRepository<User, long>
    {
        private static readonly string connString = Contansts.ConnectString;
        private static readonly string dbName = "WanMing";
        private static readonly MongoSequence sequence = new MongoSequence() { SequenceName = "Sequence", CollectionName = "CollectionName", IncrementID = "IncrementID" };



        public UserMongoRepo()
        : base(connString, dbName, "User", null, null, sequence)
        {

        }
    }
}
