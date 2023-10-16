class FieldBuilderEx<TSource, TReturn>(FieldType fieldType) :
    FieldBuilder<TSource, TReturn>(fieldType)
{
    public override FieldBuilder<TSource, TReturn> Resolve(IFieldResolver? resolver) =>
        throw new("The resolve has already been configured");
}