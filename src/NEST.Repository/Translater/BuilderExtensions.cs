using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Repository.Translater
{
    public static class BuilderExtensions
    {
        public static Builders<TEntity> Where<TEntity>(this Builders<TEntity> filter, QueryContainer appendQuery)
        {
            return new Builders<TEntity>(filter.Query.And(appendQuery));
        }

        public static Builders<TEntity> Where<TEntity>(this Builders<TEntity> filter, Expression<Func<TEntity, bool>> predicate)
        {
            var container = new QueryTranslator().VisitQuery(predicate);

            return filter.Where<TEntity>(container);
        }

        public static Builders<TEntity> Include<TEntity>(this Builders<TEntity> project, string field)
        {
            if (project.Project.Includes == null)
            {
                project.Project.Includes = new Field(field);
            }
            else
            {
                project.Project.Includes.And(field);
            }
            return new Builders<TEntity>(project.Project);
        }

        public static QueryContainer And(this QueryContainer left, QueryContainer right)
        {
            if (left == null)
            {
                return right;
            }
            return left && right;
        }

        public static QueryContainer Or(this QueryContainer left, QueryContainer right)
        {
            if (left == null)
            {
                return right;
            }
            return left || right;
        }
    }
}
