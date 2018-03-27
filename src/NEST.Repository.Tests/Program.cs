using System;

namespace NEST.Repository.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            //Repository.Container.RepositoryContainer.Register<TestRepo>();
            //var testRepo = Repository.Container.RepositoryContainer.Resolve<TestRepo>();
            Repository.Container.RepositoryContainer.Register<UserRepo>();
            var userRepo = Repository.Container.RepositoryContainer.Resolve<UserRepo>();

            //var test = testRepo.Get("5ab0df04b389d73dfe4c8f42");
            //var tests = testRepo.GetList(
            //   filterExp: q => q.Term(i => i.Field(f => f.Age).Value(13)),
            //   includeFieldExp: p => p.Includes(i => i.Fields(f => f.Age, f => f.Role)),
            //   sortExp: s => s.Age,
            //   sortType: Nest.SortOrder.Ascending,
            //   limit: 100,
            //   skip: 0
            //   );


            // es里面的字段必须小写，否则查询会出现问题
            var user = userRepo.Get(9);
            var users = userRepo.GetList(
                filterExp: q => q.Term(i => i.Field(f => f.Age).Value(13)),
                includeFieldExp: p => p.Includes(i => i.Fields(f => f.Age)),
                sortExp: s => s.Age,
                sortType: Nest.SortOrder.Ascending,
                limit: 100,
                skip: 0
               );


            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
