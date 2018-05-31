using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Repository.Translater
{
    public class QueryTranslator : ExpressionVisitor
    {
        internal QueryContainer Query { get; set; }

        public QueryContainer VisitQuery(Expression predicate)
        {
            this.Visit(predicate);

            return Query;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(string) && node.Method.Name == "Contains")
            {
                if (node.Object is MemberExpression propertyExpression)
                {
                    var value = EvalValue(node.Arguments[0]);
                    var name = propertyExpression.Member.Name;
                    Query = new QueryContainer(new QueryStringQuery()
                    {
                        Fields = new Field(name),
                        Query = "*" + value + "*",
                        AnalyzeWildcard = true
                    });
                }
            }
            if (node.Method.DeclaringType == typeof(Enumerable) && node.Method.Name == "Contains")
            {
                if (node.Arguments[1] is MemberExpression propertyExpression)
                {
                    var name = propertyExpression.Member.Name;
                    var value = EvalValue(node.Arguments[0]) as IEnumerable<object>;
                    var terms = value?.Cast<object>();
                    Query = new QueryContainer(new TermsQuery { Field = name, Terms = terms });
                }
            }
            if (node.Method.DeclaringType != null && node.Method.DeclaringType.IsGenericType && node.Method.DeclaringType.GetGenericTypeDefinition() == typeof(List<>) && node.Method.Name == "Contains")
            {
                if (node.Arguments[0] is MemberExpression propertyExpression)
                {
                    var name = propertyExpression.Member.Name;
                    var value = EvalValue(node.Object) as IEnumerable<object>;
                    var terms = value?.Cast<object>();
                    Query = new QueryContainer(new TermsQuery { Field = name, Terms = terms });
                }
            }
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var handler = new BinaryExpressionHandler(node);
            switch (node.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    VisitAnd(node);
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    VisitOr(node);
                    break;
                case ExpressionType.Equal:
                    VisitEqual(handler);
                    break;
                case ExpressionType.NotEqual:
                    VisitNotEqual(handler);
                    break;
                case ExpressionType.LessThan:
                    VisitLessThanOrEqual(handler);
                    break;
                case ExpressionType.LessThanOrEqual:
                    VisitLessThanOrEqual(handler, true);
                    break;
                case ExpressionType.GreaterThan:
                    VisitGreaterThanOrEqual(handler);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    VisitGreaterThanOrEqual(handler, true);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The binary operator ‘{0}’ is not supported", node.NodeType));
            }
            return node;
        }

        private void VisitAnd(BinaryExpression node)
        {
            var leftQuery = new QueryTranslator().VisitQuery(node.Left);
            var rightQuery = new QueryTranslator().VisitQuery(node.Right);

            Query = leftQuery && rightQuery;
        }

        private void VisitOr(BinaryExpression node)
        {
            var leftQuery = new QueryTranslator().VisitQuery(node.Left);
            var rightQuery = new QueryTranslator().VisitQuery(node.Right);
            Query = leftQuery || rightQuery;
        }

        private void VisitEqual(BinaryExpressionHandler node)
        {
            if (node.HasPropertyExpression && node.PropertyExpression.Expression.NodeType == ExpressionType.Parameter)
            {
                var name = node.PropertyExpression.Member.Name;
                var value = EvalValue(node.ValueExpression);
                Query = new QueryContainer(new TermQuery() { Field = name, Value = value });
                return;
            }

            throw new NotSupportedException("Not support this expression：" + node);
        }

        private void VisitNotEqual(BinaryExpressionHandler node)
        {
            if (node.HasPropertyExpression && node.PropertyExpression.Expression.NodeType == ExpressionType.Parameter)
            {
                var name = node.PropertyExpression.Member.Name;
                var value = EvalValue(node.ValueExpression);
                var q = new QueryContainer(new TermQuery() { Field = name, Value = value });
                Query = new QueryContainer(new BoolQuery { MustNot = new List<QueryContainer>() { q } });
                return;
            }

            throw new NotSupportedException("Not support this expression：" + node);
        }

        private void VisitGreaterThanOrEqual(BinaryExpressionHandler node, bool containEqual = false)
        {
            if (node.HasPropertyExpression && node.PropertyExpression.Expression.NodeType == ExpressionType.Parameter)
            {
                var name = node.PropertyExpression.Member.Name;
                dynamic value = EvalValue(node.ValueExpression);
                if (node.IsRight)
                {
                    Query = LowerThanOrEqual(name, value, containEqual);
                }
                else
                {
                    Query = GreaterThanOrEqual(name, value, containEqual);
                }
            }
        }

        private void VisitLessThanOrEqual(BinaryExpressionHandler node, bool containEqual = false)
        {
            if (node.HasPropertyExpression && node.PropertyExpression.Expression.NodeType == ExpressionType.Parameter)
            {
                var name = node.PropertyExpression.Member.Name;
                dynamic value = EvalValue(node.ValueExpression);
                if (node.IsRight)
                {
                    Query = GreaterThanOrEqual(name, value, containEqual);
                }
                else
                {
                    Query = LowerThanOrEqual(name, value, containEqual);
                }
            }
        }

        private object EvalValue(Expression expression)
        {
            if (expression is ConstantExpression c)
            {
                return c.Value;
            }

            LambdaExpression lambda = Expression.Lambda(expression);
            return lambda.Compile().DynamicInvoke(null);
        }

        private QueryContainer GreaterThanOrEqual(string fieldName, object value, bool containEqual = false)
        {
            if (containEqual)
            {
                return new QueryContainer(new TermRangeQuery() { Field = fieldName, GreaterThanOrEqualTo = value.ToString() });
            }

            return new QueryContainer(new TermRangeQuery() { Field = fieldName, GreaterThan = value.ToString() });
        }

        private QueryContainer LowerThanOrEqual(string fieldName, object value, bool containEqual = false)
        {
            if (containEqual)
            {
                return new QueryContainer(new TermRangeQuery() { Field = fieldName, LessThanOrEqualTo = value.ToString() });
            }

            return new QueryContainer(new TermRangeQuery() { Field = fieldName, LessThan = value.ToString() });
        }

        private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.000Z";
        private QueryContainer GreaterThanOrEqual(string fieldName, DateTime value, bool containEqual = false)
        {
            if (containEqual)
            {
                return new QueryContainer(new DateRangeQuery() { Field = fieldName, GreaterThanOrEqualTo = value.ToString() });
            }

            return new QueryContainer(new DateRangeQuery() { Field = fieldName, GreaterThan = value.ToUniversalTime().ToString(DateTimeFormat) });
        }

        private QueryContainer LowerThanOrEqual(string fieldName, DateTime value, bool containEqual = false)
        {
            if (containEqual)
            {
                return new QueryContainer(new DateRangeQuery() { Field = fieldName, LessThanOrEqualTo = value.ToString() });
            }

            return new QueryContainer(new DateRangeQuery() { Field = fieldName, LessThan = value.ToUniversalTime().ToString(DateTimeFormat) });
        }
    }
}
