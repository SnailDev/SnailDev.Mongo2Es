using NEST.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mongo2Es.DataChecker
{
    public class UserRepo : NESTReaderRepository<User, long>
    {
        public static string connString = "http://localhost:9200/";

        public UserRepo()
            : base(connString)
        {

        }
    }
}
