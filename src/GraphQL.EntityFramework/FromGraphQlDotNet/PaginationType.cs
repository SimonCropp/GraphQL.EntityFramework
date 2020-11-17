
namespace GraphQL.Types.Relay
{
    public class PaginationType<TNodeType> : ObjectGraphType<object>
        where TNodeType : IGraphType
    {
        public PaginationType()
        {
            Name = $"{typeof(TNodeType).GraphQLName()}Pagination";
            Description = $"A pagination from an object to a list of objects of type `{typeof(TNodeType).GraphQLName()}`.";

            Field<NonNullGraphType<PaginationMetaDataType>>()
                .Name("metaData")
                .Description("Information to aid in pagination.");


            Field<ListGraphType<TNodeType>>()
                .Name("items")
                .Description(
                    "A list of all of the objects returned in the pagination. This is a convenience field provided " +
                    "for quickly exploring the API; rather than querying for \"{ edges { node } }\" when no edge data " +
                    "is needed, this field can be used instead. Note that when clients like Relay need to fetch " +
                    "the \"cursor\" field on the edge to enable efficient pagination, this shortcut cannot be used, " +
                    "and the full \"{ edges { node } } \" version should be used instead.");
        }
    }
}