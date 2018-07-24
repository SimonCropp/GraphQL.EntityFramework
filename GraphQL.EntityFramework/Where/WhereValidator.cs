using System;
using GraphQL.EntityFramework;

static class WhereValidator
{
    public static void ValidateObject(Type propertyType, Comparison comparison, StringComparison? @case)
    {
        if (comparison == Comparison.Contains ||
            comparison == Comparison.StartsWith ||
            comparison == Comparison.EndsWith ||
            comparison == Comparison.Like)
        {
            throw new ErrorException($"Cannot perform {comparison} on {propertyType.FullName}.");
        }

        if (@case != null)
        {
            throw new ErrorException($"Cannot use {nameof(StringComparison)} when comparing {propertyType.FullName}.");
        }
    }

    public static void ValidateString(Comparison comparison, StringComparison? @case)
    {
        if (comparison == Comparison.GreaterThan ||
            comparison == Comparison.GreaterThanOrEqual ||
            comparison == Comparison.LessThanOrEqual ||
            comparison == Comparison.LessThan)
        {
            throw new ErrorException($"Cannot perform {comparison} on a String.");
        }

        if (comparison == Comparison.Like && @case != null)
        {
            throw new ErrorException($"{nameof(Comparison.Like)} is not compatible with {nameof(StringComparison)}.");
        }
    }
}