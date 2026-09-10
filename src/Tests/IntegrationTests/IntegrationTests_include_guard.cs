public partial class IntegrationTests
{
    // The include path skipped every navigation to a type already on the path, or to a base of
    // one. So a second, unrelated navigation from an attachment back to the root's base type was
    // never included, and came back null. Only the inverse of the navigation just traversed is
    // skipped now, which is the one nested include EF rejects.
    [Fact]
    public async Task Unrelated_navigation_to_base_type_is_included()
    {
        var related = new TphLeafEntity
        {
            Property = "Related",
            LeafProperty = "RelatedLeaf"
        };
        var leaf = new TphLeafEntity
        {
            Property = "TheRequest",
            LeafProperty = "LeafValue"
        };
        var attachment = new TphAttachmentEntity
        {
            Property = "TheAttachment",
            Request = leaf,
            RelatedRequest = related
        };
        leaf.Attachments.Add(attachment);

        var query =
            """
            {
              tphMiddleEntities(where: {property: {equal: "TheRequest"}})
              {
                property
                attachments
                {
                  property
                  relatedRequest
                  {
                    property
                  }
                }
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, true, [related, leaf, attachment]);
    }

    // The inverse skip has to hold in a no tracking query, where EF rejects the cycle outright
    [Fact]
    public async Task Tph_base_type_include_skipped_with_filter_no_tracking()
    {
        var leaf = new TphLeafEntity
        {
            Property = "TheRequest",
            LeafProperty = "LeafValue"
        };
        var attachment = new TphAttachmentEntity
        {
            Property = "TheAttachment",
            Request = leaf
        };
        leaf.Attachments.Add(attachment);

        var query =
            """
            {
              tphMiddleEntities
              {
                property
                attachments
                {
                  property
                }
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, BuildTphFilters(), true, [leaf, attachment]);
    }
}
