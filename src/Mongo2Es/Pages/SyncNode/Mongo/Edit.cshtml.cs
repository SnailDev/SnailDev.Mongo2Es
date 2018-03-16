using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mongo2Es.Pages.SyncNode.Mongo
{
    public class EditModel : PageModel
    {
        private readonly Mongo2Es.Mongo.MongoClient _db;
        private readonly string database = "Mongo2Es";
        private readonly string collection = "MongoNode";

        public EditModel(Mongo2Es.Mongo.MongoClient db)
        {
            _db = db;
        }

        [BindProperty]
        public Mongo2Es.Mongo.MongoNode Node { get; set; }

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                Node = new Mongo2Es.Mongo.MongoNode();
            }
            else
            {
                Node = _db.GetCollectionData<Mongo2Es.Mongo.MongoNode>(database, collection, $"{{'_id':new ObjectId('{id}')}}").FirstOrDefault();
                if (Node == null)
                {
                    return RedirectToPage("/SyncNode/Mongo/Index");
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
                _db.InsertCollectionData<Mongo2Es.Mongo.MongoNode>(database, collection, Node);
            else
            {
                Node.UpdateTime = DateTime.Now;
                _db.UpdateCollectionData<Mongo2Es.Mongo.MongoNode>(database, collection, Node);
            }

            return RedirectToPage("/SyncNode/Mongo/Index");
        }
    }
}