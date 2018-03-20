using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mongo2Es.Pages.SyncNode
{
    public class IndexModel : PageModel
    {
        private readonly Mongo2Es.Mongo.MongoClient _db;
        private readonly string database = "Mongo2Es";
        private readonly string collection = "SyncNode";

        public IndexModel(Mongo2Es.Mongo.MongoClient db)
        {
            _db = db;
        }

        public IList<Middleware.SyncNode> Nodes { get; private set; }

        public Dictionary<Middleware.SyncSwitch, string> SyncSwitchDic = new Dictionary<Middleware.SyncSwitch, string>()
        {
            { Middleware.SyncSwitch.Stop,"停止" },
            { Middleware.SyncSwitch.Stoping,"停止中" },
            { Middleware.SyncSwitch.Run,"运行" },
        };

        public Dictionary<Middleware.SyncStatus, string> SyncStatusDic = new Dictionary<Middleware.SyncStatus, string>()
        {
            { Middleware.SyncStatus.WaitForScan,   "等待全表扫描"},
            { Middleware.SyncStatus.ProcessScan,   "执行全表扫描"},
            { Middleware.SyncStatus.ScanException, "全部扫描异常"},
            { Middleware.SyncStatus.CompletedScan, "完成全表扫描"},
            { Middleware.SyncStatus.WaitForTail,   "等待增量同步"},
            { Middleware.SyncStatus.ProcessTail,   "增量同步中"},
            { Middleware.SyncStatus.TailException, "增量同步失败"},
        };


        public void OnGet()
        {
            Nodes = _db.GetCollectionData<Middleware.SyncNode>(database, collection).ToList();
        }

        public IActionResult OnPostDelete(string id)
        {
            _db.DeleteCollectionData<Middleware.SyncNode>(database, collection, id);

            return RedirectToPage();
        }

        public IActionResult OnPostSwitch(string id, Middleware.SyncSwitch flag)
        {
            var node = _db.GetCollectionData<Middleware.SyncNode>(database, collection, $"{{'_id':new ObjectId('{id}')}}").FirstOrDefault();
            if (node != null)
            {
                if (flag == Middleware.SyncSwitch.Run)
                {
                    if (node.Switch == Middleware.SyncSwitch.Run)
                        node.Switch = Middleware.SyncSwitch.Stoping;
                }
                else
                {
                    if (node.Switch == Middleware.SyncSwitch.Stop)
                        node.Switch = Middleware.SyncSwitch.Run;
                    if (node.Status == Middleware.SyncStatus.ScanException)
                        node.Status = Middleware.SyncStatus.WaitForScan;
                    if (node.Status == Middleware.SyncStatus.TailException)
                        node.Status = Middleware.SyncStatus.WaitForTail;
                }

                _db.UpdateCollectionData<Middleware.SyncNode>(database, collection, node);
            }

            return RedirectToPage();
        }
    }
}
