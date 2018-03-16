using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mongo2Es.Pages.SyncNode.Mongo
{
    public class IndexModel : PageModel
    {
        private readonly Mongo2Es.Mongo.MongoClient _db;
        private readonly string database = "Mongo2Es";
        private readonly string collection = "MongoNode";

        public IndexModel(Mongo2Es.Mongo.MongoClient db)
        {
            _db = db;
        }

        public IList<Mongo2Es.Mongo.MongoNode> Nodes { get; private set; }

        public void OnGet()
        {
            Nodes = _db.GetCollectionData<Mongo2Es.Mongo.MongoNode>(database, collection).ToList();
        }

        public IActionResult OnPostDelete(string id)
        {
            _db.DeleteCollectionData<Mongo2Es.Mongo.MongoNode>(database, collection, id);

            return RedirectToPage();
        }
    }
}
