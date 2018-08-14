using Nest;
using NEST.Repository.Container;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mongo2Es.DataChecker
{
    class Program
    {
        static Program()
        {
            RepositoryContainer.Register<UserRepo>();
            MongoDB.Repository.RepositoryContainer.Register<UserMongoRepo>();

            RepositoryContainer.Register<MallCardRepo>();
            MongoDB.Repository.RepositoryContainer.Register<MallCardMongoRepo>();

            RepositoryContainer.Register<CardVoucherInfoRepo>();
            MongoDB.Repository.RepositoryContainer.Register<CardVoucherInfoMongoRepo>();
        }

        static void Main(string[] args)
        {
            // CheckUserData();
            // CheckMallCardData();
            // CheckMallCardData_New();
            CheckCardVoucherInfoData();

            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }

        static void CheckUserData()
        {
            var userEsRepo = RepositoryContainer.Resolve<UserRepo>();
            var users = userEsRepo.GetList(x => x.MallID == 10008, sortExp: x => x.ID, sortType: SortOrder.Ascending, limit: 10000);

            var userMongoRepo = MongoDB.Repository.RepositoryContainer.Resolve<UserMongoRepo>();
            var usersInMongo = userMongoRepo.GetList(x => x.MallID == 10008, sortExp: x => x.ID, sortType: MongoDB.Repository.SortType.Ascending, limit: 10000);

            Console.WriteLine(users.Item1);
            Console.WriteLine(usersInMongo.Count);

            var userIds = users.Item2.Select(x => x.ID);
            var userInMongoIds = usersInMongo.Select(x => x.ID);

            var userEsExceptMongoIds = userIds.Except(userInMongoIds);

            Console.WriteLine("在Es中不在MongoDb中的ID个数有{0}", userEsExceptMongoIds.Count());
            foreach (var id in userEsExceptMongoIds)
            {
                Console.WriteLine(id);
            }

            var userMongoExceptEsIds = userInMongoIds.Except(userIds);

            Console.WriteLine("在MongoDb中不在Es中的ID个数有{0}", userMongoExceptEsIds.Count());
            foreach (var id in userMongoExceptEsIds)
            {
                Console.WriteLine(id);
            }
        }

        static void CheckMallCardData()
        {
            var mallEsRepo = RepositoryContainer.Resolve<MallCardRepo>();
            var mallMongoRepo = MongoDB.Repository.RepositoryContainer.Resolve<MallCardMongoRepo>();

            long startIdInEs = 0;
            long startIdInMongo = 0;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            var mallsInEs = mallEsRepo.GetList(x => x.ID > startIdInEs, sortExp: x => x.ID, sortType: SortOrder.Ascending, limit: 10000);
            sw.Stop();

            Console.WriteLine("查询es耗时：{0}", sw.ElapsedMilliseconds);

            while (mallsInEs.Item2.Count > 0)
            {
                Console.WriteLine("获取数据数目：{0}", mallsInEs.Item2.Count);

                sw.Restart();
                var mallsInMongo = mallMongoRepo.GetList(x => x.ID > startIdInMongo, sortExp: x => x.ID, sortType: MongoDB.Repository.SortType.Ascending, limit: 10000);
                sw.Stop();
                Console.WriteLine("查询MongoDB耗时：{0}", sw.ElapsedMilliseconds);

                startIdInEs = mallsInEs.Item2.Last().ID;
                startIdInMongo = mallsInMongo.Last().ID;


                var mallIds = mallsInEs.Item2.Select(x => x.ID);
                var mallInMongoIds = mallsInMongo.Select(x => x.ID);

                var mallEsExceptMongoIds = mallIds.Except(mallInMongoIds);

                Console.WriteLine("在Es中不在MongoDb中的ID个数有{0}", mallEsExceptMongoIds.Count());
                foreach (var id in mallEsExceptMongoIds)
                {
                    Console.WriteLine(id);
                }

                var mallMongoExceptEsIds = mallInMongoIds.Except(mallIds);

                Console.WriteLine("在MongoDb中不在Es中的ID个数有{0}", mallMongoExceptEsIds.Count());
                foreach (var id in mallMongoExceptEsIds)
                {
                    Console.WriteLine(id);
                }

                sw.Restart();
                mallsInEs = mallEsRepo.GetList(x => x.ID > startIdInEs, sortExp: x => x.ID, sortType: SortOrder.Ascending, limit: 10000);
                sw.Stop();
                Console.WriteLine("查询es耗时：{0}", sw.ElapsedMilliseconds);
            }
        }


        static void CheckMallCardData_New()
        {
            var mallEsRepo = RepositoryContainer.Resolve<MallCardRepo>();
            var mallMongoRepo = MongoDB.Repository.RepositoryContainer.Resolve<MallCardMongoRepo>();

            //17700000~18000000数
            long startIdInEs = 17800000;
            long startIdInMongo = 17800000;

            long endIdInEs = 17850965;
            long endIdInMongo = 17850965;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            var mallsInEs = mallEsRepo.GetList(x => x.ID >= startIdInEs && x.ID < endIdInEs, sortExp: x => x.ID, sortType: SortOrder.Ascending);
            sw.Stop();

            Console.WriteLine("查询es耗时：{0}", sw.ElapsedMilliseconds);

            while (mallsInEs.Item1 > 0)
            {
                Console.WriteLine("获取数据数目：{0}", mallsInEs.Item1);               

                sw.Restart();
                var mallsInMongo = mallMongoRepo.Count(x => x.ID >= startIdInMongo && x.ID < endIdInMongo);
                sw.Stop();
                Console.WriteLine("查询MongoDB耗时：{0}", sw.ElapsedMilliseconds);


                Console.WriteLine("正在对比区间：{0}~{1}", startIdInEs, endIdInEs);
                if (mallsInEs.Item1 != mallsInMongo)
                {
                    Console.WriteLine("区间：{0}~{1}数据不一致", startIdInEs, endIdInEs);
                    Console.WriteLine("差异：{0}", mallsInEs.Item1- mallsInMongo);
                }

                startIdInEs = endIdInEs;
                startIdInMongo = endIdInMongo;

                endIdInEs += 60000;
                endIdInMongo += 60000;

                Console.WriteLine();

                sw.Restart();
                mallsInEs = mallEsRepo.GetList(x => x.ID >= startIdInEs && x.ID < endIdInEs, sortExp: x => x.ID, sortType: SortOrder.Ascending);
                sw.Stop();
                Console.WriteLine("查询es耗时：{0}", sw.ElapsedMilliseconds);
            }
        }

        static void CheckCardVoucherInfoData()
        {
            var esRepo = RepositoryContainer.Resolve<CardVoucherInfoRepo>();
            var mongoRepo = MongoDB.Repository.RepositoryContainer.Resolve<CardVoucherInfoMongoRepo>();

            long startIdInEs = 0;
            long startIdInMongo = 0;

            long endIdInEs = 300000;
            long endIdInMongo = 300000;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            var inEs = esRepo.GetList(x => x.ID >= startIdInEs && x.ID < endIdInEs, sortExp: x => x.ID, sortType: SortOrder.Ascending);
            sw.Stop();

            Console.WriteLine("查询es耗时：{0}", sw.ElapsedMilliseconds);

            while (inEs.Item1 > 0)
            {
                Console.WriteLine("获取数据数目：{0}", inEs.Item1);

                sw.Restart();
                var inMongo = mongoRepo.Count(x => x.ID >= startIdInMongo && x.ID < endIdInMongo);
                sw.Stop();
                Console.WriteLine("查询MongoDB耗时：{0}", sw.ElapsedMilliseconds);


                Console.WriteLine("正在对比区间：{0}~{1}", startIdInEs, endIdInEs);
                if (inEs.Item1 != inMongo)
                {
                    Console.WriteLine("区间：{0}~{1}数据不一致", startIdInEs, endIdInEs);
                    Console.WriteLine("差异：{0}", inEs.Item1 - inMongo);

                    Thread.Sleep(5000);
                }

                startIdInEs = endIdInEs;
                startIdInMongo = endIdInMongo;

                endIdInEs += 300000;
                endIdInMongo += 300000;

                Console.WriteLine();

                sw.Restart();
                inEs = esRepo.GetList(x => x.ID >= startIdInEs && x.ID < endIdInEs, sortExp: x => x.ID, sortType: SortOrder.Ascending);
                sw.Stop();
                Console.WriteLine("查询es耗时：{0}", sw.ElapsedMilliseconds);
            }
        }
    }
}
