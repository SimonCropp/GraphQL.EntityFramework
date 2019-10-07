using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace GraphQL.EntityFramework
{
    public static class FilterBuilder<T>
    {
        const string LIST_PROPERTY_PATTERN = @"\[(.*)\]";

        private static readonly MethodInfo StringContains = typeof(string).GetMethod("Contains");
        private static readonly MethodInfo StringStartsWith = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        private static readonly MethodInfo StringEndsWith = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
        private static readonly MethodInfo StringTrim = typeof(string).GetMethod("Trim", new Type[0]);
        private static readonly MethodInfo StringToLower = typeof(string).GetMethod("ToLower", new Type[0]);

        //private static readonly Dictionary<Comparison, Func<Expression, Expression, Expression, Expression>> Expressions = new Dictionary<Comparison, Func<Expression, Expression, Expression, Expression>>
        //    {
        //        { Comparison.Equal, (member, constant, constant2) => Expression.Equal(member, constant) },
        //        { Comparison.NotEqual, (member, constant, constant2) => Expression.NotEqual(member, constant) },
        //        { Comparison.GreaterThan, (member, constant, constant2) => Expression.GreaterThan(member, constant) },
        //        { Comparison.GreaterThanOrEqual, (member, constant, constant2) => Expression.GreaterThanOrEqual(member, constant) },
        //        { Comparison.LessThan, (member, constant, constant2) => Expression.LessThan(member, constant) },
        //        { Comparison.LessThanOrEqual, (member, constant, constant2) => Expression.LessThanOrEqual(member, constant) },
        //        { Comparison.Contains, (member, constant, constant2) => Contains(member, constant) },
        //        { Comparison.NotContains, (member, constant, constant2) => Expression.Not(Contains(member, constant)) },
        //        { Comparison.StartsWith, (member, constant, constant2) => Expression.Call(member, StringStartsWith, constant) },
        //        { Comparison.EndsWith, (member, constant, constant2) => Expression.Call(member, StringEndsWith, constant) },
        //        { Comparison.Between, (member, constant, constant2) => Between(member, constant, constant2) },
        //        { Comparison.In, (member, constant, constant2) => Contains(member, constant) },
        //        { Comparison.NotIn, (member, constant, constant2) => Expression.Not(Contains(member, constant)) },
        //        { Comparison.IsNull, (member, constant, constant2) => Expression.Equal(member, Expression.Constant(null)) },
        //        { Comparison.IsNotNull, (member, constant, constant2) => Expression.NotEqual(member, Expression.Constant(null)) },
        //        { Comparison.IsEmpty, (member, constant, constant2) => Expression.Equal(member, Expression.Constant(string.Empty)) },
        //        { Comparison.IsNotEmpty, (member, constant, constant2) => Expression.NotEqual(member, Expression.Constant(string.Empty)) },
        //        { Comparison.IsNullOrWhiteSpace, (member, constant, constant2) => IsNullOrWhiteSpace(member) },
        //        { Comparison.IsNotNullNorWhiteSpace, (member, constant, constant2) => IsNotNullNorWhiteSpace(member) }
        //    };

        #region Conditional List

        /// <summary>
        /// Build a predicate for a supplied list of where's (Grouped or not)
        /// </summary>
        /// <param name="wheres"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> BuildPredicate(IEnumerable<WhereExpression> wheres)
        {
            var expressionBody = MakePredicateBody(wheres);
            var param = PropertyCache<T>.GetSourceParameter();

            return Expression.Lambda<Func<T, bool>>(expressionBody, param);
        }


        /// <summary>
        /// Makes the predicate body from the supplied parameter and list of where expressions
        /// </summary>
        /// <param name="wheres"></param>
        /// <returns></returns>
        private static Expression MakePredicateBody(IEnumerable<WhereExpression> wheres)
        {
            Expression mainExpression = null;
            WhereExpression previousWhere = null;

            // Iterate over wheres
            foreach (var where in wheres)
            {
                Expression nextExpression;

                // If there are grouped expressions
                if (where.GroupedExpressions?.Count() > 0)
                {
                    // Recurse with new set of expression
                    nextExpression = MakePredicateBody(where.GroupedExpressions);
                }
                // Otherwise handle single expressions
                else
                {
                    // Get the predicate body for the single expression
                    nextExpression = MakePredicateBody(where.Path, where.Comparison ?? Comparison.Equal, where.Value, where.Case);
                }

                // If this is the first where processed
                if (mainExpression == null)
                {
                    // Assign to main expression
                    mainExpression = nextExpression;
                }
                else
                {
                    // Otherwise combine expression by specified connector or default (AND) if not provided
                    mainExpression = CombineExpressions(previousWhere.Connector ?? Connector.And, mainExpression, nextExpression);
                }

                // Save the previous where so the connector can be retrieved
                previousWhere = where;
            }

            return mainExpression;
        }

        #endregion


        #region Conditional Single

        /// <summary>
        /// Create a single predicate for the single set of supplied conditional arguments
        /// </summary>
        /// <param name="path"></param>
        /// <param name="comparison"></param>
        /// <param name="values"></param>
        /// <param name="stringComparison"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> BuildPredicate(string path, Comparison comparison, string[] values, StringComparison? stringComparison = null)
        {
            var expressionBody = MakePredicateBody(path, comparison, values, stringComparison);
            var param = PropertyCache<T>.GetSourceParameter();

            return Expression.Lambda<Func<T, bool>>(expressionBody, param);
        }

        /// <summary>
        /// Makes the predicate body from the single set of supplied conditional arguments
        /// </summary>
        /// <param name="param"></param>
        /// <param name="path"></param>
        /// <param name="comparison"></param>
        /// <param name="values"></param>
        /// <param name="stringComparison"></param>
        /// <returns></returns>
        private static Expression MakePredicateBody(string path, Comparison comparison, string[] values, StringComparison? stringComparison = null)
        {
            Expression expression;

            // If path includes list property access
            if (HasListInPath(path))
            {
                // Handle a list path
                expression = ProcessList(path, comparison, values, stringComparison);
            }
            // Otherwise linear property access
            else
            {
                // Just get expression
                expression = GetExpression(path, comparison, values, stringComparison);
            }

            return expression;
        }

        #endregion


        #region Body Builders (Lol)

        /// <summary>
        /// Process a list based item inside the property path
        /// </summary>
        /// <param name="param"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        private static Expression ProcessList(string path, Comparison comparison, string[] values, StringComparison? stringComparison = null)
        {
            // Get the path pertaining to individual list items
            var listPath = Regex.Match(path, LIST_PROPERTY_PATTERN).Groups[1].Value;
            // Remove the part of the path that leads into list item properties
            path = Regex.Replace(path, LIST_PROPERTY_PATTERN, "");

            // Get the property on the current object up to the list member
            var property = PropertyCache<T>.GetProperty(path);

            // Get the list item type details
            var listItemType = property.PropertyType.GetGenericArguments().Single();
            var listItemParam = Expression.Parameter(listItemType, "i");

            // Generate the predicate for the list item type
            var subPredicate = typeof(FilterBuilder<>)
                .MakeGenericType(listItemType)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name.Equals("BuildPredicate") && m.GetParameters().Length == 4).Single()
                .Invoke(new object(), new object[] { path, comparison, values, stringComparison }) as Expression;

            // Generate a method info for the Any Enumerable Static Method
            var anyInfo = typeof(Enumerable)
                        .GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .First(m => m.Name == "Any" && m.GetParameters().Count() == 2)
                        .MakeGenericMethod(listItemType);

            // Create Any Expression Call
            return Expression.Call(anyInfo, property.Left, subPredicate);
        }


        /// <summary>
        /// Build an expression from provided where parameters
        /// </summary>
        /// <param name="param"></param>
        /// <param name="path"></param>
        /// <param name="comparison"></param>
        /// <param name="values"></param>
        /// <param name="stringComparison"></param>
        /// <returns></returns>
        private static Expression GetExpression(string path, Comparison comparison, string[] values, StringComparison? stringComparison = null)
        {
            var property = PropertyCache<T>.GetProperty(path);
            Expression expressionBody;

            if (property.PropertyType == typeof(string))
            {
                WhereValidator.ValidateString(comparison, stringComparison);
                switch (comparison)
                {
                    case Comparison.In:
                        expressionBody = MakeStringIn(values, property, stringComparison);
                        break;

                    case Comparison.NotIn:
                        expressionBody = MakeStringIn(values, property, stringComparison, true);
                        break;

                    default:
                        var value = values?.Single();
                        expressionBody = MakeStringCompare(comparison, value, property, stringComparison);
                        break;
                }
            }
            else
            {
                WhereValidator.ValidateObject(property.PropertyType, comparison, stringComparison);
                switch (comparison)
                {
                    case Comparison.In:
                        expressionBody = MakeObjectIn(values, property);
                        break;

                    case Comparison.NotIn:
                        expressionBody = MakeObjectIn(values, property, true);
                        break;

                    default:
                        var value = values?.Single();
                        expressionBody = MakeObjectCompare(comparison, value, property);
                        break;
                }
            }

            return expressionBody;
        }

        #endregion


        #region Operations

        static Expression MakeStringCompare(Comparison comparison, string value, Property<T> property, StringComparison? stringComparison)
        {
            WhereValidator.ValidateSingleString(comparison, stringComparison);
            var body = MakeStringComparison(property.Left, comparison, value, stringComparison);
            return body;
        }

        static Expression MakeObjectCompare(Comparison comparison, string value, Property<T> property)
        {
            WhereValidator.ValidateSingleObject(property.PropertyType, comparison, null);
            var valueObject = TypeConverter.ConvertStringToType(value, property.PropertyType);
            var body = MakeObjectComparison(property.Left, comparison, valueObject);
            return body;
        }

        static Expression MakeObjectIn(string[] values, Property<T> property, bool not = false)
        {
            var objects = TypeConverter.ConvertStringsToList(values, property.PropertyType);
            var constant = Expression.Constant(objects);
            var body = Expression.Call(constant, property.ListContainsMethod, property.Left);
            return not ? Expression.Not(body) : (Expression)body;
        }

        static Expression MakeStringIn(string[] array, Property<T> property, StringComparison? comparison, bool not = false)
        {
            MethodCallExpression equalsBody;
            if (comparison == null)
            {
                equalsBody = Expression.Call(null, ReflectionCache.StringEqual, ExpressionCache.StringParam, property.Left);
            }
            else
            {
                equalsBody = Expression.Call(null, ReflectionCache.StringEqualComparison, ExpressionCache.StringParam, property.Left, Expression.Constant(comparison));
            }

            var itemEvaluate = Expression.Lambda<Func<string, bool>>(equalsBody, ExpressionCache.StringParam);
            var anyBody = Expression.Call(null, ReflectionCache.StringAny, Expression.Constant(array), itemEvaluate);
            return not ? Expression.Not(anyBody) : (Expression)anyBody;
        }

        static Expression MakeStringComparison(Expression left, Comparison comparison, string value, StringComparison? stringComparison)
        {
            var valueConstant = Expression.Constant(value, typeof(string));
            var nullCheck = Expression.NotEqual(left, ExpressionCache.Null);

            if (stringComparison == null)
            {
                switch (comparison)
                {
                    case Comparison.Equal:
                        return Expression.Call(ReflectionCache.StringEqual, left, valueConstant);
                    case Comparison.Like:
                        return Expression.Call(null, ReflectionCache.StringLike, ExpressionCache.EfFunction, left, valueConstant);
                    case Comparison.NotEqual:
                        var notEqualsCall = Expression.Call(ReflectionCache.StringEqual, left, valueConstant);
                        return Expression.Not(notEqualsCall);
                    case Comparison.StartsWith:
                        var startsWithExpression = Expression.Call(left, ReflectionCache.StringStartsWith, valueConstant);
                        return Expression.AndAlso(nullCheck, startsWithExpression);
                    case Comparison.EndsWith:
                        var endsWithExpression = Expression.Call(left, ReflectionCache.StringEndsWith, valueConstant);
                        return Expression.AndAlso(nullCheck, endsWithExpression);
                    case Comparison.Contains:
                        var indexOfExpression = Expression.Call(left, ReflectionCache.StringIndexOf, valueConstant);
                        var notEqualExpression = Expression.NotEqual(indexOfExpression, ExpressionCache.NegativeOne);
                        return Expression.AndAlso(nullCheck, notEqualExpression);
                }
            }
            else
            {
                var comparisonConstant = Expression.Constant(stringComparison, typeof(StringComparison));
                switch (comparison)
                {
                    case Comparison.Equal:
                        return Expression.Call(ReflectionCache.StringEqualComparison, left, valueConstant, comparisonConstant);
                    case Comparison.NotEqual:
                        var notEqualsCall = Expression.Call(ReflectionCache.StringEqualComparison, left, valueConstant, comparisonConstant);
                        return Expression.Not(notEqualsCall);
                    case Comparison.StartsWith:
                        var startsWithExpression = Expression.Call(left, ReflectionCache.StringStartsWithComparison, valueConstant, comparisonConstant);
                        return Expression.AndAlso(nullCheck, startsWithExpression);
                    case Comparison.EndsWith:
                        var endsWithExpression = Expression.Call(left, ReflectionCache.StringEndsWithComparison, valueConstant, comparisonConstant);
                        return Expression.AndAlso(nullCheck, endsWithExpression);
                    case Comparison.Contains:
                        var indexOfExpression = Expression.Call(left, ReflectionCache.StringIndexOfComparison, valueConstant, comparisonConstant);
                        var notEqualExpression = Expression.NotEqual(indexOfExpression, ExpressionCache.NegativeOne);
                        return Expression.AndAlso(nullCheck, notEqualExpression);
                }

            }

            throw new NotSupportedException($"Invalid comparison operator '{comparison}'.");
        }

        static Expression MakeObjectComparison(Expression left, Comparison comparison, object value)
        {
            var constant = Expression.Constant(value, left.Type);
            switch (comparison)
            {
                case Comparison.Equal:
                    return Expression.MakeBinary(ExpressionType.Equal, left, constant);
                case Comparison.NotEqual:
                    return Expression.MakeBinary(ExpressionType.NotEqual, left, constant);
                case Comparison.GreaterThan:
                    return Expression.MakeBinary(ExpressionType.GreaterThan, left, constant);
                case Comparison.GreaterThanOrEqual:
                    return Expression.MakeBinary(ExpressionType.GreaterThanOrEqual, left, constant);
                case Comparison.LessThan:
                    return Expression.MakeBinary(ExpressionType.LessThan, left, constant);
                case Comparison.LessThanOrEqual:
                    return Expression.MakeBinary(ExpressionType.LessThanOrEqual, left, constant);
            }

            throw new NotSupportedException($"Invalid comparison operator '{comparison}'.");
        }

        #endregion


        #region New Operations

        //private static Expression CombineExpressions(Expression expr1, Expression expr2, Connector connector)
        //{
        //    return connector == Connector.And ? Expression.AndAlso(expr1, expr2) : Expression.OrElse(expr1, expr2);
        //}

        //private static Expression Contains(Expression member, Expression expression)
        //{
        //    MethodCallExpression contains = null;
        //    if (expression is ConstantExpression constant && typeof(IEnumerable<>).IsAssignableFrom(constant.Value.GetType()))
        //    {
        //        var type = constant.Value.GetType();
        //        var containsInfo = type.GetMethod("Contains", new[] { type.GetGenericArguments()[0] });
        //        contains = Expression.Call(constant, containsInfo, member);
        //    }

        //    return contains ?? Expression.Call(member, StringContains, expression); ;
        //}

        //private static Expression Between(Expression member, Expression constant, Expression constant2)
        //{
        //    var left = Expressions[Comparison.GreaterThanOrEqual].Invoke(member, constant, null);
        //    var right = Expressions[Comparison.LessThanOrEqual].Invoke(member, constant2, null);

        //    return CombineExpressions(left, right, Connector.And);
        //}

        //private static Expression IsNullOrWhiteSpace(Expression member)
        //{
        //    Expression exprNull = Expression.Constant(null);
        //    var trimMemberCall = Expression.Call(member, StringTrim);
        //    Expression exprEmpty = Expression.Constant(string.Empty);
        //    return Expression.OrElse(
        //                            Expression.Equal(member, exprNull),
        //                            Expression.Equal(trimMemberCall, exprEmpty));
        //}

        //private static Expression IsNotNullNorWhiteSpace(Expression member)
        //{
        //    Expression exprNull = Expression.Constant(null);
        //    var trimMemberCall = Expression.Call(member, StringTrim);
        //    Expression exprEmpty = Expression.Constant(string.Empty);
        //    return Expression.AndAlso(
        //                            Expression.NotEqual(member, exprNull),
        //                            Expression.NotEqual(trimMemberCall, exprEmpty));
        //}

        #endregion


        #region Helpers

        /// <summary>
        /// Checks the path for matching list property marker
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool HasListInPath(string path)
        {
            return Regex.IsMatch(path, LIST_PROPERTY_PATTERN);
        }

        /// <summary>
        /// Combine expressions by a specified binary operator
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="expr1"></param>
        /// <param name="expr2"></param>
        /// <returns></returns>
        static Expression CombineExpressions(Connector connector, Expression expr1, Expression expr2)
        {
            switch (connector)
            {
                case Connector.And:
                    return Expression.AndAlso(expr1, expr2);
                case Connector.Or:
                    return Expression.OrElse(expr1, expr2);
            }

            throw new NotSupportedException($"Invalid connector operator '{connector}'.");
        }

        #endregion
    }
}
