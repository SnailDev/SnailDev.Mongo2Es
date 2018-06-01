# NEST.Repository

A simple encapsulation with NEST client for search data form elasticsearch.

## API

### NESTReaderRepository
```csharp
TEntity Get(TKey id);
TEntity Get(Func<QueryContainerDescriptor<TEntity>, QueryContainer> filterFunc = null,
            Func<SourceFilterDescriptor<TEntity>, ISourceFilter> includeFieldFunc = null,
            Expression<Func<TEntity, object>> sortExp = null, SortOrder sortType = SortOrder.Ascending);
TEntity Get(Expression<Func<TEntity, bool>> filterExp = null,
            Expression<Func<TEntity, object>> includeFieldExp = null,
            Expression<Func<TEntity, object>> sortExp = null, SortOrder sortType = SortOrder.Ascending);
Tuple<long, List<TEntity>> GetList(Func<QueryContainerDescriptor<TEntity>, QueryContainer> filterFunc = null,
            Func<SourceFilterDescriptor<TEntity>, ISourceFilter> includeFieldFunc = null,
            Expression<Func<TEntity, object>> sortExp = null, SortOrder sortType = SortOrder.Ascending
           , int limit = 10, int skip = 0);
Tuple<long, List<TEntity>> GetList(Expression<Func<TEntity, bool>> filterExp = null,
            Expression<Func<TEntity, object>> includeFieldExp = null,
            Expression<Func<TEntity, object>> sortExp = null, SortOrder sortType = SortOrder.Ascending
           , int limit = 10, int skip = 0);
```

### NESTReaderRepositoryAsync
```csharp
Task<TEntity> GetAsync(TKey id);
Task<TEntity> GetAsync(Func<QueryContainerDescriptor<TEntity>, QueryContainer> filterFunc = null,
            Func<SourceFilterDescriptor<TEntity>, ISourceFilter> includeFieldFunc = null,
            Expression<Func<TEntity, object>> sortExp = null, SortOrder sortType = SortOrder.Ascending);
Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> filterExp = null,
            Expression<Func<TEntity, object>> includeFieldExp = null,
            Expression<Func<TEntity, object>> sortExp = null, SortOrder sortType = SortOrder.Ascending);
Task<Tuple<long, List<TEntity>>> GetListAsync(Func<QueryContainerDescriptor<TEntity>, QueryContainer> filterFunc = null,
            Func<SourceFilterDescriptor<TEntity>, ISourceFilter> includeFieldFunc = null,
            Expression<Func<TEntity, object>> sortExp = null, SortOrder sortType = SortOrder.Ascending
           , int limit = 0, int skip = 0);
Task<Tuple<long, List<TEntity>>> GetListAsync(Expression<Func<TEntity, bool>> filterExp = null,
            Expression<Func<TEntity, object>> includeFieldExp = null,
            Expression<Func<TEntity, object>> sortExp = null, SortOrder sortType = SortOrder.Ascending
           , int limit = 10, int skip = 0)
```

## Depend on
```csharp
NEST 6.0.2
Repository.IEntity 2.0.1 (or you can write IEntity<T> interface and you entity inherit it.)
```

## How to Use

First, you need have an entity inherit IEntity\<T\>, T is type of PrimaryKey. eg
```csharp
[Serializable]
[BsonIgnoreExtraElements]
public class User : IEntity<long>
{
    [BsonId]
    public long ID { get; set; }

    public double Age { get; set; }

    public double Sex { get; set; }

    public string Like { get; set; }
}
```

Second, you need have a repository inherit NESTReaderRepository or NESTReaderRepositoryAsync. eg
```csharp
public class UserRepo : NESTReaderRepository<User, long>
{
    public static string connString = "http://localhost:9200/";

    public UserRepo()
        : base(connString)
    {

    }
}
```

Now, you can search data with simple api. eg
```csharp
 static void Main(string[] args)
 {
    Repository.Container.RepositoryContainer.Register<UserRepo>();
    var userRepo = Repository.Container.RepositoryContainer.Resolve<UserRepo>();

    var user = userRepo.Get(9);
    var users = userRepo.GetList(
        filterFunc: q => +q.Range(r => r.Field(f => f.Age).GreaterThan(13).LessThan(28)), 
		// filterFunc: q => +q.Range(r => r.Field("Age").GreaterThan(13).LessThan(28)),
        includeFieldFunc: p => p.Includes(i => i.Fields(f => f.Age, f => f.Sex, f => f.Like)),
        sortExp: s => s.Age,
        sortType: Nest.SortOrder.Ascending,
        limit: 100,
        skip: 0
    );

	// lambda expression. haven't support BsonElement(Name)
	var users = userRepo.GetList(
        filterExp: x=> x.Age > 13 && x.Age < 28, 
        includeFieldExp: x=> new { x.Age, x.Sex, x.Like },
        sortExp: s => s.Age,
        sortType: Nest.SortOrder.Ascending,
        limit: 100,
        skip: 0
    );
 }
```

## How to write a Query
### 0x00. Structured Search
>By default, documents will be returned in _score descending order, where the _score for each hit is the relevancy score calculated for how well the document matched the query criteria.
```csharp
q => q.DateRange(r => r
    .Field(f => f.{Field with DateTime Type})
    .GreaterThanOrEquals(new DateTime(2017, 01, 01))
    .LessThan(new DateTime(2018, 01, 01))
)
```

>The benefit of executing a query in a filter context is that Elasticsearch is able to forgo calculating a relevancy score, as well as cache filters for faster subsequent performance.
```csharp
 q => q.Bool(b => b.Filter(bf => bf
    .DateRange(r => r
        .Field(f => f.{Field with DateTime Type})
        .GreaterThanOrEquals(new DateTime(2017, 01, 01))
        .LessThan(new DateTime(2018, 01, 01))
        )
    )
)
```

### 0x01. Unstructured Search
Full text queries (find all documents that contain "Russ" in the lead developer first name field)
```csharp
q => q.Match(m => m
    .Field(f => f.LeadDeveloper.FirstName)
    .Query("Russ")
)
```

### 0x02. Combining Search
```csharp
q => q.Bool(b => b
    .Must(mu => mu
        .Match(m => m
            .Field(f => f.LeadDeveloper.FirstName)
            .Query("Russ")
        ), mu => mu
        .Match(m => m
            .Field(f => f.LeadDeveloper.LastName)
            .Query("Cam")
        )
    )
    .Filter(fi => fi
        .DateRange(r => r
            .Field(f => f.StartedOn)
            .GreaterThanOrEquals(new DateTime(2017, 01, 01))
            .LessThan(new DateTime(2018, 01, 01))
        )
    )
)
```

use operator
```csharp
q => q.Match(m => m
        .Field(f => f.LeadDeveloper.FirstName)
        .Query("Russ")
    ) && q
    .Match(m => m
        .Field(f => f.LeadDeveloper.LastName)
        .Query("Cam")
    ) && +q
    .DateRange(r => r
        .Field(f => f.StartedOn)
        .GreaterThanOrEquals(new DateTime(2017, 01, 01))
        .LessThan(new DateTime(2018, 01, 01))
    )
)
```

Should ==> OR ==> ||  
Must ==> And ==> &&  
Must_Not ==> NOT==> !  
Filter ==> + 

the query will be converted to a bool query if use any operator, and the answer to the bool query is always yes or no , so that will not score.


## Reference
[https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/writing-queries.html](https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/writing-queries.html)
[https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/bool-queries.html](https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/bool-queries.html)