using System.Collections.Generic;

namespace GraphQL.Types.Relay.DataObjects
{
    public class Pagination<TNode>
    {
        public PaginationMetaData MetaData { get; set; }
        public List<TNode> Items { get; set; }
        public Pagination(List<TNode> items, PaginationMetaData metaData)
        {
            MetaData = metaData;
            Items = items;
        }
    }
}