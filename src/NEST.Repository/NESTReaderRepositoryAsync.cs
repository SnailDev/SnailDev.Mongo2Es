using Nest;
using Repository.IEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Repository
{
    public class NESTReaderRepositoryAsync<TEntity, TKey> : NESTBaseRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>, new()
    {
        public NESTReaderRepositoryAsync(string connString, string index = null, string type = null)
            : base(connString, index, type)
        {

        }

        /// <summary>
        /// Get with _id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TEntity> GetAsync(TKey id)
        {
            var result = await client.GetAsync(new Nest.DocumentPath<TEntity>(new Id(id)));
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
        public async Task<TEntity> GetAsync(Func<QueryContainerDescriptor<TEntity>, QueryContainer> filterExp = null,
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

            var result = await client.SearchAsync(selector);
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
        public async Task<Tuple<long, List<TEntity>>> GetListAsync(Func<QueryContainerDescriptor<TEntity>, QueryContainer> filterExp = null,
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

            var result = await client.SearchAsync(selector);
            return new Tuple<long, List<TEntity>>(result.Total, result.Documents.ToList());
        }

        ///// <summary>
        ///// Count with filter
        ///// </summary>
        ///// <param name="filterExp"></param>
        ///// <returns></returns>
        //public async Task<long> CountAsync(Func<QueryContainerDescriptor<TEntity>, QueryContainer> filterExp = null)
        //{
        //    var result = await client.CountAsync<TEntity>(s => s
        //           .Query(filterExp ?? (q => q.MatchAll())));

        //    return result.Count;
        //}

    }
}
