public partial class IntegrationTests
{
    // Reproduces bug where a filter on a child entity adds a navigation back to a TPH base type,
    // causing a circular include (e.g. TravelRequest -> Attachment -> BaseRequest).
    // The fix in IsVisitedOrBaseType detects that the navigation target is a base type
    // of an already-visited type and skips the include.
    [Fact]
    public async Task Tph_base_type_include_skipped_with_filter()
    {
        var leaf = new TphLeafEntity
        {
            Property = "TheRequest",
            LeafProperty = "LeafValue"
        };
        var attachment = new TphAttachmentEntity
        {
            Property = "TheAttachment",
            Request = leaf,
            RequestId = leaf.Id
        };
        leaf.Attachments.Add(attachment);

        // Query the abstract middle type (TphMiddleEntity) which triggers the includes path.
        // The filter on TphAttachmentEntity accesses _.Request.Property, adding a navigation
        // back to TphRootEntity. Without the fix, this would cause:
        //   .Include("Attachments.Request") -> circular back to TphRootEntity (base of TphMiddleEntity)
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
        await RunQuery(database, query, null, BuildTphFilters(), false, [leaf, attachment]);
    }

    [Fact]
    public async Task Tph_base_type_single_include_skipped_with_filter()
    {
        var leaf = new TphLeafEntity
        {
            Property = "TheRequest",
            LeafProperty = "LeafValue"
        };
        var attachment = new TphAttachmentEntity
        {
            Property = "TheAttachment",
            Request = leaf,
            RequestId = leaf.Id
        };
        leaf.Attachments.Add(attachment);

        var query = $$"""
            {
              tphMiddleEntity(id: "{{leaf.Id}}")
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
        await RunQuery(database, query, null, BuildTphFilters(), false, [leaf, attachment]);
    }

    static Filters<IntegrationDbContext> BuildTphFilters()
    {
        var filters = new Filters<IntegrationDbContext>();
        // Filter accesses _.Request.Property which forces the Request navigation
        // to be included. This back-references TphRootEntity (base of TphMiddleEntity).
        filters.For<TphAttachmentEntity>().Add(
            _ => new TphFilterProjection
            {
                RequestProperty = _.Request != null ? _.Request.Property : null
            },
            (_, _, _, item) => item.RequestProperty != "Ignore");
        return filters;
    }

    class TphFilterProjection
    {
        public string? RequestProperty { get; set; }
    }
}
