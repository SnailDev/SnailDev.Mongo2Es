using System;
using System.Collections.Generic;
using System.Text;

namespace NEST.Repository.Tests
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
