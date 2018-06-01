using Elasticsearch.Net;
using Nest;
using NEST.Repository.Translater;
using Repository.IEntity;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace NEST.Repository
{
    ///// <summary>
    ///// Entity
    ///// </summary>
    ///// <typeparam name="TKey"></typeparam>
    //public interface IEntity<TKey>
    //{
    //    /// <summary>
    //    /// 主键
    //    /// </summary>
    //    TKey ID { get; set; }
    //}
    public class NESTBaseRepository
    {

    }

    public class NESTBaseRepository<TEntity, TKey> : NESTBaseRepository where TEntity : class, IEntity<TKey>, new()
    {
        protected ElasticClient client;

        protected virtual string Index { get; private set; }

        protected virtual string Type { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connString"></param>
        /// <param name="index"></param>
        /// <param name="type"></param>
        public NESTBaseRepository(string connString, string index = null, string type = null)
        {
            if (string.IsNullOrWhiteSpace(connString))
            {
                throw new Exception("ElasticSearch ConnnectString could not be empty.");
            }

            var uris = connString.Split(',').ToList().ConvertAll(x => new Uri(x));
            var connectionPool = new StaticConnectionPool(uris);
            var settings = new ConnectionSettings(connectionPool);

            Index = index ?? typeof(TEntity).Name.ToLower();
            Type = type ?? typeof(TEntity).Name.ToLower();


            settings = settings.DefaultIndex(Index)
                        .DefaultTypeName(Type)
                        .DefaultFieldNameInferrer(s => s); // By default, NEST camelcases all field names, this make no changes to the name
            client = new ElasticClient(settings);
        }

        public Func<SourceFilterDescriptor<TEntity>, ISourceFilter> IncludeFields(Expression<Func<TEntity, object>> fieldsExp)
        {
            var builder = Builders<TEntity>.Projection;

            var body = (fieldsExp.Body as NewExpression);
            if (body == null || body.Members == null)
            {
                throw new Exception("fieldsExp is invalid expression format， eg: x => new { x.Field1, x.Field2 }");
            }
            foreach (var m in body.Members)
            {
                builder = builder.Include(m.Name);
            }

            return x => builder.Project;
        }
    }
}
