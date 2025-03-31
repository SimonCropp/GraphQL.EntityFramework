namespace GraphQL.EntityFramework;

public delegate TDbContext ResolveDbContext<out TDbContext>(object userContext, IServiceProvider? requestServices)
    where TDbContext : DbContext;