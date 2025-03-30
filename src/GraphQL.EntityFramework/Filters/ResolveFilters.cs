namespace GraphQL.EntityFramework;

public delegate Filters<TDbContext>? ResolveFilters<TDbContext>(object userContext)
    where TDbContext : DbContext;