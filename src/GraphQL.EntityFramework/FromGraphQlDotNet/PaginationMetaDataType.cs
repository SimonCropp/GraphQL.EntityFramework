namespace GraphQL.Types.Relay
{
    public class PaginationMetaDataType : ObjectGraphType<object>
    {
        public PaginationMetaDataType()
        {
            Name = "PaginationMetaData";
            Description = "Information about pagination in a pagination.";

            Field<NonNullGraphType<IntGraphType>>("totalPages", "Total number of pages");
            Field<NonNullGraphType<IntGraphType>>("total", "Total number of items");
            Field<NonNullGraphType<IntGraphType>>("row", "Row per page");
            Field<NonNullGraphType<IntGraphType>>("page", "Current page");
        }
    }
}