using System;
using System.Collections.Generic;
using System.Text;

namespace NEST.Repository.Tests
{
    public class CardVoucherInfoRepo : NESTReaderRepository<CardVoucherInfo, long>
    {
        public static string connString = "http://elasticsearch-t.mallcoo.cn/";

        public CardVoucherInfoRepo()
            : base(connString)
        {

        }
    }
}
