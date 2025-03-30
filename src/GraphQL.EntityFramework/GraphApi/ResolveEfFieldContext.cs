namespace GraphQL.EntityFramework;

public class ResolveEfFieldContext<TDbContext, TSource> :
    ResolveFieldContext<TSource>
    where TDbContext : DbContext
{
    public TDbContext DbContext { get; set; } = null!;
    public Filters<TDbContext>? Filters { get; set; }
}