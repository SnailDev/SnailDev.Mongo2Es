using NEST.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mongo2Es.DataChecker
{
    public class CardVoucherInfoRepo : NESTReaderRepository<CardVoucherInfo, long>
    {
        public static string connString = "http://10.27.70.7:9200/";

        public CardVoucherInfoRepo()
            : base(connString)
        {

        }
    }
}
