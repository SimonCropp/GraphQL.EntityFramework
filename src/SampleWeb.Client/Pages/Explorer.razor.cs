namespace SampleWeb.Client;

/// <summary>
/// The query explorer: the IDE over the host app's own <c>/graphql</c> endpoint, through the same
/// fetcher the home page uses, so both pages land in the one sidecar log. There is no endpoint bar
/// and no sidecar panel — the endpoint is this app's, and this page is the IDE itself.
/// </summary>
public partial class Explorer
{
    /// <summary>
    /// What the first tab opens with. Every argument here is one the library adds rather than
    /// something the graph types declare, so running it is also a tour of what it generates.
    /// </summary>
    const string DefaultQuery =
        """
        # The schema behind this endpoint is EF Core, mapped by GraphQL.EntityFramework.
        #
        # Nothing below is hand-written resolver code: the nested selection decides what is
        # loaded, and where/orderBy/skip/take become LINQ against the database.
        #
        # Press the play button (or Ctrl-Enter) to run it. Space completes, and hovering a
        # field shows its docs.

        query Companies {
          companies(
            where: {employees: {any: {age: {greaterThan: 20}}}}
            orderBy: {content: ascending}
            take: 3) {
            id
            content
            employees(orderBy: {age: descending}) {
              content
              age
            }
          }
        }
        """;
}
