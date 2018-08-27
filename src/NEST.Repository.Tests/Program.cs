using Nest;
using NEST.Repository.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NEST.Repository.Tests
{
    class Program
    {
        static void Main(string[] args)
        {

            string datestr = "{'date':'0001-01-01T00:00:00Z','date1':'2018-01-01T00:00:00Z'}";

            var obj = JsonConvert.DeserializeObject<HaHa>(datestr, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Include,
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                Converters = { new CustomDateTimeJsonConverter() }
            });

            Console.WriteLine(DateTime.Parse("0001-01-01T00:00:00Z").ToLocalTime());


            //Repository.Container.RepositoryContainer.Register<TestRepo>();
            //var testRepo = Repository.Container.RepositoryContainer.Resolve<TestRepo>();
            Repository.Container.RepositoryContainer.Register<UserRepo>();
            var userRepo = Repository.Container.RepositoryContainer.Resolve<UserRepo>();

            //Repository.Container.RepositoryContainer.Register<CouponLifeCycleRepo>();
            //var couponRepo = Repository.Container.RepositoryContainer.Resolve<CouponLifeCycleRepo>();

            //var test = testRepo.Get("5ab0df04b389d73dfe4c8f42");
            //var tests = testRepo.GetList(
            //   filterExp: q => +q.Term(i => i.Field(f => f.Age).Value(13)),
            //   includeFieldExp: p => p.Includes(i => i.Fields(f => f.Age, f => f.Role)),
            //   sortExp: s => s.Age,
            //   sortType: Nest.SortOrder.Ascending,
            //   limit: 100,
            //   skip: 0
            //   );

            // f => f.MallID
            var userList = userRepo.GetList(x => x.MallID == 10008);
            userList = userRepo.GetList(x => x.MallID == 10008, sortExp: x => x.ID, sortType: SortOrder.Descending, skip: 31740, limit: 20);
            userList = userRepo.GetList(x => x.MallID == 10008, skip: 31740);

            //var tests1 = couponRepo.GetList(filterFunc: q => q.Term(i => i.Field(f => f.MallID).Value(10008)));

            //var tests2 = couponRepo.GetList(filterFunc: x => x.Term(m => m.Field(f => f.ID).Value("5a0973bb04c1a69fd8166666")));

            //QueryContainerDescriptor<CouponLifeCycle> desc = new QueryContainerDescriptor<CouponLifeCycle>();
            //QueryContainer container = desc.Term(m => m.Field(f => f.ID).Value("5a0973bb04c1a69fd8166666"));

            //container = container || desc.Term(i => i.Field("MallID").Value(10057));
            //Func<QueryContainerDescriptor<CouponLifeCycle>, QueryContainer> filterFunc = x => container;

            //var tests3 = couponRepo.GetList(filterFunc: x => container);


            //var result = couponRepo.GetList(filterExp: x => x.ID == "5a0973bb04c1a69fd8166666", sortExp: y => y.ChangePeople.Suffix("keyword"), sortType: Nest.SortOrder.Descending);

            //var tests1 = couponRepo.Get("59639443c0801209046d9a8e");
            //var tests = couponRepo.GetList(filterExp: x => x.Mall == 10008, includeFieldExp: x => new { x.Mall });

            // Console.WriteLine("Hello World!");
            //// es里面的字段必须小写，否则查询会出现问题
            //var user = userRepo.Get(9); 
            //var users = userRepo.GetList(
            //    filterExp: q => +q.Range(r => r.Field(f => f.Age).GreaterThan(13).LessThan(28)), 
            //                    // +q.Match(m => m.Field(f => f.Age).Query("13")), 
            //                    // q => +q.Term(i => i.Field(f => f.Age).Value(13)),
            //    includeFieldExp: p => p.Includes(i => i.Fields(f => f.Age, f => f.Sex, f => f.Like)),
            //    sortExp: s => s.Age,
            //    sortType: Nest.SortOrder.Ascending,
            //    limit: 100,
            //    skip: 0
            //   );

            // fileterExp 写法：
            // q => q.Term(i => i.Field(f => f.Age).Value(13))
            // q => q.Match(m => m.Field(f => f.Age).Query("13"))

            // 0x00. Structured search
            // "By default, documents will be returned in _score descending order, 
            //  where the _score for each hit is the relevancy score calculated for 
            //  how well the document matched the query criteria."

            // DateRange   1. ==>q => q.DateRange(r => r
            //                          .Field(f => f.{DateTimeTypeField})
            //                          .GreaterThanOrEquals(new DateTime(2017, 01, 01))
            //                          .LessThan(new DateTime(2018, 01, 01))
            //                        )

            // The benefit of executing a query in a filter context is that Elasticsearch is able to 
            // forgo calculating a relevancy score, as well as cache filters for faster subsequent performance.

            //             2. ==>q => q.Bool(b => b
            //                          .Filter(bf => bf
            //                              .DateRange(r => r
            //                                  .Field(f => f.StartedOn)
            //                                  .GreaterThanOrEquals(new DateTime(2017, 01, 01))
            //                                  .LessThan(new DateTime(2018, 01, 01))
            //                                  )
            //                              )
            //                          )

            // 0x01. Unstructured search
            // Full text queries (find all documents that contain "Russ" in the lead developer first name field)
            //                  ==>q => q.Match(m => m
            //                              .Field(f => f.LeadDeveloper.FirstName)
            //                              .Query("Russ")
            //                             )

            // 0x02. Combining search
            //              1.  ==>q => q.Bool(b => b
            //                              .Must(mu => mu
            //                                  .Match(m => m
            //                                      .Field(f => f.LeadDeveloper.FirstName)
            //                                      .Query("Russ")
            //                                  ), mu => mu
            //                                  .Match(m => m
            //                                      .Field(f => f.LeadDeveloper.LastName)
            //                                      .Query("Cam")
            //                                  )
            //                              )
            //                              .Filter(fi => fi
            //                                   .DateRange(r => r
            //                                      .Field(f => f.StartedOn)
            //                                      .GreaterThanOrEquals(new DateTime(2017, 01, 01))
            //                                      .LessThan(new DateTime(2018, 01, 01))
            //                                  )
            //                              )
            //                          )

            //               2.       q => q.Match(m => m
            //                                    .Field(f => f.LeadDeveloper.FirstName)
            //                                    .Query("Russ")
            //                                ) && q
            //                                .Match(m => m
            //                                    .Field(f => f.LeadDeveloper.LastName)
            //                                    .Query("Cam")
            //                                ) && +q
            //                                .DateRange(r => r
            //                                    .Field(f => f.StartedOn)
            //                                    .GreaterThanOrEquals(new DateTime(2017, 01, 01))
            //                                    .LessThan(new DateTime(2018, 01, 01))
            //                                )
            //                            )



            // Should==>OR==>||  Must==>And==>&&  Must_Not==>NOT==>!  Filter==>+  operator都会转bool

            // https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/writing-queries.html
            // https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/bool-queries.html

            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}

public class HaHa
{
    //[JsonConverter(typeof(CustomDateTimeJsonConverter))]
    public DateTime date { get; set; }

    public DateTime date1 { get; set; }
}
