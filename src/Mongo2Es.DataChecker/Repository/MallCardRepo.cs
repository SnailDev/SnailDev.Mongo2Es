using NEST.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mongo2Es.DataChecker
{
    public class MallCardRepo : NESTReaderRepository<MallCard, long>
    {
        public static string connString = "http://10.27.70.7:9200/";

        public MallCardRepo()
            : base(connString)
        {

        }
    }
}
