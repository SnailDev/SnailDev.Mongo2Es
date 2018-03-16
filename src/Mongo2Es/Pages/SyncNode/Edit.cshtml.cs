using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mongo2Es.Pages.SyncNode
{
    public class EditModel : PageModel
    {
        private readonly Mongo2Es.Mongo.MongoClient _db;
        private readonly string database = "Mongo2Es";
        private readonly string collection = "SyncNode";

        public EditModel(Mongo2Es.Mongo.MongoClient db)
        {
            _db = db;
        }

        [BindProperty]
        public Mongo2Es.Middleware.SyncNode Node { get; set; }

        [BindProperty]
        public IList<Mongo2Es.ElasticSearch.EsNode> EsNodes { get; set; }

        [BindProperty]
        public IList<Mongo2Es.Mongo.MongoNode> MongoNodes { get; set; }

        public IActionResult OnGet(string id)
        {
            MongoNodes = _db.GetCollectionData<Mongo2Es.Mongo.MongoNode>(database, "MongoNode").ToList();
            EsNodes = _db.GetCollectionData<ElasticSearch.EsNode>(database, "EsNode").ToList();

            if (string.IsNullOrWhiteSpace(id))
            {
                Node = new Middleware.SyncNode();
            }
            else
            {
                Node = _db.GetCollectionData<Middleware.SyncNode>(database, collection, $"{{'_id':new ObjectId('{id}')}}").FirstOrDefault();
                if (Node == null)
                {
                    return RedirectToPage("/SyncNode/Index");
                }
            }

            return Page();
        }


        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Node.ID))
                _db.InsertCollectionData<Middleware.SyncNode>(database, collection, Node);
            else
            {
                Node.UpdateTime = DateTime.Now;
                _db.UpdateCollectionData<Middleware.SyncNode>(database, collection, Node);
            }

            return RedirectToPage("/SyncNode/Index");
        }
    }
}