using System;
using System.Collections.Generic;
using System.Linq;

namespace EfCoreGraphQL
{
    public static class ExpressionContextExtractor
    {
        //Portfolios(where: [{Member: "Title", Comparison: "Contains", Value: "Communications"}]) {
        public static IEnumerable<WhereExpression> Read<T>(Func<Type, string, object> getArgument)
        {
            foreach (var expression in getArgument.ReadList<WhereExpression>("where"))
            {
                yield return expression;
            }
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