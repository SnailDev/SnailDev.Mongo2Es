using System;
using System.Collections.Generic;
using System.Text;

namespace NEST.Repository.Tests
{
    public class CardVoucherInfoRepo : NESTReaderRepository<CardVoucherInfo, long>
    {
        public static string connString = "http://localhost:9200/";

        public CardVoucherInfoRepo()
            : base(connString)
        {

        }
    }
}
