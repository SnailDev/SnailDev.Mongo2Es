# NEST.Repository

A simple encapsulation with NEST client for search data form elasticsearch.

## API

### NESTReaderRepository
```csharp
TEntity Get(TKey id);
TEntity Get(Func<QueryContainerDescriptor<TEntity>, QueryContainer> filterExp = null,
            Func<SourceFilterDescriptor<TEntity>, ISourceFilter> includeFieldExp = null,
            Expression<Func<TEntity, object>> sortExp = null, SortOrder sortType = SortOrder.Ascending);
Tuple<long, List<TEntity>> GetList(Func<QueryContainerDescriptor<TEntity>, QueryContainer> filterExp = null,
            Func<SourceFilterDescriptor<TEntity>, ISourceFilter> includeFieldExp = null,
            Expression<Func<TEntity, object>> sortExp = null, SortOrder sortType = SortOrder.Ascending
           , int limit = 10, int skip = 0)
```

### NESTReaderRepositoryAsync
```csharp
Task<TEntity> GetAsync(TKey id);
Task<TEntity> GetAsync(Func<QueryContainerDescriptor<TEntity>, QueryContainer> filterExp = null,
            Func<SourceFilterDescriptor<TEntity>, ISourceFilter> includeFieldExp = null,
            Expression<Func<TEntity, object>> sortExp = null, SortOrder sortType = SortOrder.Ascending);
Task<Tuple<long, List<TEntity>>> GetListAsync(Func<QueryContainerDescriptor<TEntity>, QueryContainer> filterExp = null,
            Func<SourceFilterDescriptor<TEntity>, ISourceFilter> includeFieldExp = null,
            Expression<Func<TEntity, object>> sortExp = null, SortOrder sortType = SortOrder.Ascending
           , int limit = 0, int skip = 0)
```

## Depend on
```csharp
NEST 6.0.2
Repository.IEntity 2.0.1 (or you can write IEntity<T> interface and you entity inherit it.)
```

## How to Use

First, you need have an entity inherit IEntity<T>, T is type of PrimaryKey. eg
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
        filterExp: q => +q.Range(r => r.Field(f => f.Age).GreaterThan(13).LessThan(28)), 
        includeFieldExp: p => p.Includes(i => i.Fields(f => f.Age, f => f.Sex, f => f.Like)),
        sortExp: s => s.Age,
        sortType: Nest.SortOrder.Ascending,
        limit: 100,
        skip: 0
    );
 }
```

## How to write a Query


## Reference
[https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/writing-queries.html](https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/writing-queries.html)
[https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/bool-queries.html](https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/bool-queries.html)