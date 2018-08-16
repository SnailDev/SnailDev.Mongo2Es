using System;
using System.Collections.Generic;
using System.Text;

namespace NEST.Repository.Tests
{
    public class UserRepo : NESTReaderRepository<User, long>
    {
        public static string connString = "http://elasticsearch-t.mallcoo.cn/";

        public UserRepo()
            : base(connString)
        {

        }
    }
}
