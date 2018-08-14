using NEST.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mongo2Es.DataChecker
{
    public class ActivityGiftRepo : NESTReaderRepository<ActivityGift, long>
    {
        public static string connString = "http://localhost:9200/";

        public ActivityGiftRepo()
            : base(connString)
        {

        }
    }
}
