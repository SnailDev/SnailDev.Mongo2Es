using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mongo2Es.ElasticSearch
{
    public class EsData
    {
        public string Oper { get; set; }

        public string ID { get; set; }

        public object Data { get; set; }

        public DateTime Time { get; set; }
    }
}
