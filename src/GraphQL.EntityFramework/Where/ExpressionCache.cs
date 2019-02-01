using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

static class ExpressionCache
{
    public static ConstantExpression NegativeOne = Expression.Constant(-1);
    public static ConstantExpression Null = Expression.Constant(null, typeof(object));
    public static ConstantExpression EfFunction = Expression.Constant(EF.Functions);
    public static ParameterExpression StringParam = Expression.Parameter(typeof(string));
}