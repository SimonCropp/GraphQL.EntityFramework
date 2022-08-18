using GraphQL.Builders;
using GraphQL.Resolvers;
using GraphQL.Types;

class FieldBuilderEx<TSource, TReturn> : FieldBuilder<TSource, TReturn>
{
    public FieldBuilderEx(FieldType fieldType) : base(fieldType)
    {
    }

    public override FieldBuilder<TSource, TReturn> Resolve(IFieldResolver? resolver) =>
        throw new("The resolve has already been configured");
}