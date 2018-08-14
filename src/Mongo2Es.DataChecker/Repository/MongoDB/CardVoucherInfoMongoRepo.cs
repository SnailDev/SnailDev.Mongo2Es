using MongoDB.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mongo2Es.DataChecker
{ 
    public class CardVoucherInfoMongoRepo : MongoReaderRepository<CardVoucherInfo, long>
    {
        private static readonly string connString = Contansts.ConnectString;
        private static readonly string dbName = "Coupon_Ext";
        private static readonly MongoSequence sequence = new MongoSequence();

        public CardVoucherInfoMongoRepo()
        : base(connString, dbName, null, null, null, sequence)
        {

        }
    }
}
