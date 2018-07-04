using System;
using System.Collections.Generic;
using System.Text;

namespace NEST.Repository.Tests
{
    public class CouponLifeCycleRepo : NESTReaderRepository<CouponLifeCycle, string>
    {
        public static string connString = "http://elasticsearch-t.mallcoo.cn/";

        public CouponLifeCycleRepo()
            : base(connString)
        {

        }
    }
}
