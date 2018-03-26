using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace NEST.Repository
{
    public class NESTReaderRepository<TEntity, TKey> : NESTBaseRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>, new()
    {
        public NESTReaderRepository(string connString, string index = null, string type = null)
            : base(connString, index, type)
        {

        }

        /// <summary>
        /// Get with _id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TEntity Get(TKey id)
        {
            var result = client.Get(new Nest.DocumentPath<TEntity>(new Id(id)));
            if (result.Found)
            {
                return result.Source;
            }

            return null;
        }

        /// <summary>
        /// Get with filters
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="includeFieldExp"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <returns></returns>
        public TEntity Get(Func<QueryContainerDescriptor<TEntity>, QueryContainer> filterExp,
            Func<SourceFilterDescriptor<TEntity>, ISourceFilter> includeFieldExp = null,
            Expression<Func<TEntity, object>> sortExp = null, SortOrder sortType = SortOrder.Ascending)
        {
            Func<SearchDescriptor<TEntity>, ISearchRequest> selector = null;

            if (sortExp != null)
            {
                selector = new Func<SearchDescriptor<TEntity>, ISearchRequest>(s => s
                    .Query(filterExp ?? (q => q.MatchAll()))
                    .Sort(st => st.Field(sortExp, sortType))
                    .Source(includeFieldExp ?? (i => i.IncludeAll())));
            }
            else
            {
                selector = new Func<SearchDescriptor<TEntity>, ISearchRequest>(s => s
                    .Query(filterExp ?? (q => q.MatchAll()))
                    .Source(includeFieldExp ?? (i => i.IncludeAll())));
            }

            var result = client.Search(selector);
            if (result.Total > 0)
            {
                return result.Documents.FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// GetList
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="includeFieldExp"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        public List<TEntity> GetList(Func<QueryContainerDescriptor<TEntity>, QueryContainer> filterExp,
            Func<SourceFilterDescriptor<TEntity>, ISourceFilter> includeFieldExp = null,
            Expression<Func<TEntity, object>> sortExp = null, SortOrder sortType = SortOrder.Ascending
           , int limit = 0, int skip = 0)
        {
            Func<SearchDescriptor<TEntity>, ISearchRequest> selector = null;

            if (sortExp != null)
            {
                selector = new Func<SearchDescriptor<TEntity>, ISearchRequest>(s => s
                    .Query(filterExp ?? (q => q.MatchAll()))
                    .Sort(st => st.Field(sortExp, sortType))
                    .Source(includeFieldExp ?? (i => i.IncludeAll()))
                    .From(skip)
                    .Size(limit));
            }
            else
            {
                selector = new Func<SearchDescriptor<TEntity>, ISearchRequest>(s => s
                    .Query(filterExp ?? (q => q.MatchAll()))
                    .Source(includeFieldExp ?? (i => i.IncludeAll()))
                    .From(skip)
                    .Size(limit));
            }

            var result = client.Search(selector);
            if (result.Total > 0)
            {
                return result.Documents.ToList();
            }

            return null;
        }

        /// <summary>
        /// Count with filter
        /// </summary>
        /// <param name="filterExp"></param>
        /// <returns></returns>
        public long Count(Func<QueryContainerDescriptor<TEntity>, QueryContainer> filterExp)
        {
            var result = client.Count<TEntity>(s => s
                   .Query(filterExp ?? (q => q.MatchAll())));

            return result.Count;
        }

    }
}
