using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;

namespace Mongo2Es.Pages.SyncNode
{
    public class LogsModel : PageModel
    {
        private readonly Mongo2Es.Mongo.MongoClient _db;
        private readonly string database = "Mongo2Es";
        private readonly string collection = "SystemLog";

        public LogsModel(Mongo2Es.Mongo.MongoClient db)
        {
            _db = db;
        }

        public IList<BsonDocument> Logs { get; private set; }
        public void OnGet(string id)
        {
            Logs = _db.GetCollectionData<BsonDocument>(database, collection, $"{{'Properties.nodeid':'{id}'}}", "{'_id':-1}", 250).ToList();
        }
    }
}