using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mongo2Es.SyncWeb.Pages.SyncNode
{
    public class IndexModel : PageModel
    {
        private readonly Mongo2Es.Mongo.MongoClient _db;
        private readonly string database = "Mongo2Es";
        private readonly string collection = "SyncNode";

        public  IndexModel(Mongo2Es.Mongo.MongoClient db)
        {
            _db = db;
        }

        public IList<Middleware.SyncNode> Nodes { get; private set; }

        public void OnGet()
        {
            Nodes = _db.GetCollectionData<Middleware.SyncNode>(database, collection).ToList();
        }

        public IActionResult OnPostDelete(string id)
        {
            _db.DeleteCollectionData<Middleware.SyncNode>(database, collection, id);
            
            return RedirectToPage();
        }
    }
}
