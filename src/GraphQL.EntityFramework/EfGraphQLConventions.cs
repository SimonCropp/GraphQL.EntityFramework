namespace GraphQL.EntityFramework;

public static class EfGraphQLConventions
{
    /// <summary>
    /// Register the necessary services with the service provider for a data context of <typeparamref name="TDbContext"/>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="resolveDbContext">A function to obtain the <typeparamref name="TDbContext"/> from the GraphQL user context. If null, then it will be extracted from the <see cref="IServiceProvider"/>.</param>
    /// <param name="model">The <see cref="IModel"/> to use. If null, then it will be extracted from the <see cref="IServiceProvider"/>.</param>
    /// <param name="resolveFilters">A function to obtain a list of filters to apply to the returned data. If null, then it will be extracted from the <see cref="IServiceProvider"/>.</param>
    /// <param name="disableTracking">Use <see cref="EntityFrameworkQueryableExtensions.AsNoTracking{TEntity}"/> for all <see cref="IQueryable{T}"/> operations.</param>
    /// <param name="includeSqlInExceptions">Include the generated sql in exception messages. Off by default, since those messages can reach clients and the sql carries table and column names and, depending on the provider, parameter values.</param>
    /// <param name="orderByStyle">The shape of the generated orderBy argument. <see cref="OrderByStyle.Enum"/>, the default, takes an enum of flattened paths, as in <c>orderBy: property</c> or <c>orderBy: [property, parent_property_desc]</c>. <see cref="OrderByStyle.Object"/> takes an input object per item, as in <c>orderBy: {property: ascending}</c>.</param>
    /// <param name="orderByEnumOptions">Options for <see cref="OrderByStyle.Enum"/>. Registered for the whole container, so the first registration wins when several contexts are registered.</param>

    #region RegisterInContainer

    public static void RegisterInContainer<TDbContext>(
            IServiceCollection services,
            ResolveDbContext<TDbContext>? resolveDbContext = null,
            IModel? model = null,
            ResolveFilters<TDbContext>? resolveFilters = null,
            bool disableTracking = false,
            bool includeSqlInExceptions = false,
            OrderByStyle orderByStyle = OrderByStyle.Enum,
            OrderByEnumOptions? orderByEnumOptions = null)

        #endregion

        where TDbContext : DbContext
    {
        RegisterScalarsAndArgs(services, orderByEnumOptions);
        services.AddHttpContextAccessor();
        services.AddTransient<HttpContextCapture>();
        services.AddSingleton(provider => Build(resolveDbContext, model, resolveFilters, provider, disableTracking, includeSqlInExceptions, orderByStyle));
        services.AddSingleton<IEfGraphQLService<TDbContext>>(provider => provider.GetRequiredService<EfGraphQLService<TDbContext>>());
        // The generated where and orderBy input types resolve every registered service, so a
        // type mapped in any of the contexts is found
        services.AddSingleton<IEfGraphQLService>(provider => provider.GetRequiredService<EfGraphQLService<TDbContext>>());
    }

    static EfGraphQLService<TDbContext> Build<TDbContext>(
        ResolveDbContext<TDbContext>? dbContextResolver,
        IModel? model,
        ResolveFilters<TDbContext>? filters,
        IServiceProvider provider,
        bool disableTracking,
        bool includeSqlInExceptions,
        OrderByStyle orderByStyle)
        where TDbContext : DbContext
    {
        model ??= ResolveModel<TDbContext>(provider);
        filters ??= provider.GetService<ResolveFilters<TDbContext>>();
        dbContextResolver ??= (_, requestServices) => DbContextFromProvider<TDbContext>(provider, requestServices);

        return new(
            model,
            dbContextResolver,
            filters,
            disableTracking,
            includeSqlInExceptions,
            orderByStyle);
    }

    static TDbContext DbContextFromProvider<TDbContext>(IServiceProvider provider, IServiceProvider? requestServices)
        where TDbContext : DbContext
    {
        var dataFromRequestServices = requestServices?
            .GetService<TDbContext>();
        if (dataFromRequestServices is not null)
        {
            return dataFromRequestServices;
        }

        var dataFromHttpContext = provider.GetService<HttpContextCapture>()?
            .HttpContextAccessor
            .HttpContext?
            .RequestServices
            .GetService<TDbContext>();
        if (dataFromHttpContext is not null)
        {
            return dataFromHttpContext;
        }

        var dataFromRootProvider = provider.GetService<TDbContext>();
        if (dataFromRootProvider is not null)
        {
            return dataFromRootProvider;
        }

        throw new($"Could not extract {typeof(TDbContext).Name} from the provider. Tried the HttpContext provider and the root provider.");
    }

    static void RegisterScalarsAndArgs(IServiceCollection services, OrderByEnumOptions? orderByEnumOptions)
    {
        services.TryAddSingleton(orderByEnumOptions ?? new OrderByEnumOptions());
        services.AddSingleton<EnumerationGraphType<DayOfWeek>>();
        // The where and orderBy input types are generated per entity type, so they are
        // registered as open generics. An enum comparison takes its values through
        // EnumerationGraphType, which GraphQL.NET registers only through AddGraphQL.
        services.TryAddSingleton(typeof(EnumerationGraphType<>));
        services.TryAddSingleton(typeof(WhereGraph<>));
        services.TryAddSingleton(typeof(CollectionWhereGraph<>));
        services.TryAddSingleton(typeof(ComparisonGraph<>));
        services.TryAddSingleton(typeof(OrderByGraph<>));
        services.TryAddSingleton(typeof(OrderByEnumGraph<>));
        services.TryAddSingleton<SortDirectionGraph>();
    }

    static IModel ResolveModel<TDbContext>(IServiceProvider provider)
        where TDbContext : DbContext
    {
        var model = provider.GetService<IModel>();
        if (model is not null)
        {
            return model;
        }

        var dbContext = provider.GetService<TDbContext>();
        if (dbContext is not null)
        {
            return dbContext.Model;
        }

        throw new($"Could not resolve {nameof(IModel)} from the {nameof(IServiceProvider)}. Tried to extract both {nameof(IModel)} and {typeof(TDbContext)}.");
    }
}