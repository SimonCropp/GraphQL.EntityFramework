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
            throw new Exception($"Cannot perform {comparison} on {propertyType.FullName}.");
        }

        if (@case != null)
        {
            throw new Exception($"Cannot use {nameof(StringComparison)} when comparing {propertyType.FullName}.");
        }
    }

    public static void ValidateSingleObject(Type propertyType, Comparison comparison, StringComparison? @case)
    {
        ValidateObject(propertyType, comparison, @case);
        if (comparison == Comparison.In ||
            comparison == Comparison.NotIn)
        {
            throw new Exception($"Cannot perform {comparison} on {propertyType.FullName}.");
        }
    }

    public static void ValidateString(Comparison comparison, StringComparison? @case)
    {
        if (comparison == Comparison.GreaterThan ||
            comparison == Comparison.GreaterThanOrEqual ||
            comparison == Comparison.LessThanOrEqual ||
            comparison == Comparison.LessThan)
        {
            throw new Exception($"Cannot perform {comparison} on a String.");
        }

        if (comparison == Comparison.Like && @case != null)
        {
            throw new Exception($"{nameof(Comparison.Like)} is not compatible with {nameof(StringComparison)}.");
        }
    }

    public static void ValidateSingleString(Comparison comparison, StringComparison? @case)
    {
        ValidateString(comparison, @case);
        if (comparison == Comparison.In ||
            comparison == Comparison.NotIn)
        {
            throw new Exception($"Cannot perform {comparison} on a single String.");
        }
    }
}