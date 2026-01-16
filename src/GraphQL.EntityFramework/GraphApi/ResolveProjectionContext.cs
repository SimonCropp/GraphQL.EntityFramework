namespace GraphQL.EntityFramework;

public class ResolveProjectionContext<TDbContext, TProjection>
    where TDbContext : DbContext
{
    public TProjection Projection { get; init; } = default!;
    public TDbContext DbContext { get; init; } = null!;
    public ClaimsPrincipal? User { get; init; }
    public Filters<TDbContext>? Filters { get; init; }
    public IResolveFieldContext FieldContext { get; init; } = null!;
}
