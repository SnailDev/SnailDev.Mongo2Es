using NEST.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mongo2Es.DataChecker
{
    public class CouponLifeCycleRepo : NESTReaderRepository<CouponLifeCycle, string>
    {
        public static string connString = "http://localhost:9200/";

        public CouponLifeCycleRepo()
            : base(connString)
        {

        }
    }
}
