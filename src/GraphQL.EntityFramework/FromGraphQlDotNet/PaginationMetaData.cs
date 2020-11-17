namespace GraphQL.Types.Relay.DataObjects
{
    public class PaginationMetaData
    {
        public int TotalPages { get; set; }
        public int Total { get; set; }
        public int Row { get; set; }
        public int Page { get; set; }
    }
}