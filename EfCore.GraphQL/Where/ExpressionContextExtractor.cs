using System;
using System.Collections.Generic;
using System.Linq;

namespace EfCoreGraphQL
{
    public static class ExpressionContextExtractor
    {
        //Portfolios(wheres: [{Member: "Title", Comparison: "Contains", Value: "Communications"}]) {
        public static IEnumerable<WhereExpression> Read<T>(Func<Type, string, object> getArgument)
        {
            foreach (var whereExpression in WhereExpressions(getArgument))
            {
                yield return whereExpression;
            }
        }

        private static IEnumerable<WhereExpression> WhereExpressions(Func<Type, string, object> getArgument)
        {
            foreach (var expression in getArgument.ReadList<WhereExpression>("wheres"))
            {
                yield return expression;
            }

            var whereExpression = getArgument.Read<WhereExpression>("where");
            if (whereExpression != null)
            {
                yield return whereExpression;
            }
        }

        static T Read<T>(this Func<Type, string, object> getArgument, string name)
        {
            return (T) getArgument(typeof(T), name);
        }

        static IEnumerable<T> ReadList<T>(this Func<Type, string, object> getArgument, string name)
        {
            var argument = getArgument(typeof(T[]), name);
            if (argument == null)
            {
                return Enumerable.Empty<T>();
            }

            return (T[]) argument;
        }
    }
}