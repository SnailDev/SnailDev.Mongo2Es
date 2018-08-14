using NEST.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mongo2Es.DataChecker
{
    public class TestRepo : NESTReaderRepository<Test, string>
    {
        public static string connString = "http://localhost:9200/";

        public TestRepo()
            : base(connString, "test", "test")
        {

        }
    }
}
