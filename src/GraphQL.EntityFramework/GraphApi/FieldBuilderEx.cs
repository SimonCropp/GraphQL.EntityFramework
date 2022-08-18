using GraphQL.Builders;
using GraphQL.Types;

class FieldBuilderEx<TSource, TReturn> : FieldBuilder<TSource, TReturn>
{
    public FieldBuilderEx(FieldType fieldType) : base(fieldType)
    {
    }
}