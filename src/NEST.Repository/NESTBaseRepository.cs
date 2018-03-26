using Elasticsearch.Net;
using Nest;
using System;
using System.Linq;

namespace NEST.Repository
{
    /// <summary>
    /// Entity
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IEntity<TKey>
    {
        /// <summary>
        /// 主键
        /// </summary>
        TKey ID { get; set; }
    }


    public class NESTBaseRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>, new()
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

            Index = index ?? typeof(TEntity).Name;
            Type = type ?? typeof(TEntity).Name;


            settings = settings.DefaultIndex(Index).DefaultTypeName(Type);
            client = new ElasticClient(settings);
        }
    }
}
