using System;
using System.Collections.Generic;
using System.Text;

namespace NEST.Repository.Tests
{
    public class CouponLifeCycleRepo : NESTReaderRepository<CouponLifeCycle, string>
    {
        public static string connString = "http://116.62.55.151:9200/";

        public CouponLifeCycleRepo()
            : base(connString)
        {

        }
    }
}
