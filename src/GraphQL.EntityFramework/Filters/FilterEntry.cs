class FilterEntry<TDbContext, TEntity, TProjection> : IFilterEntry<TDbContext>
    where TDbContext : DbContext
    where TEntity : class
    where TProjection : class
{
    readonly Func<object, TDbContext, ClaimsPrincipal?, TProjection, Task<bool>> filter;
    readonly Expression<Func<TEntity, TProjection>> projection;

    public FilterEntry(
        Func<object, TDbContext, ClaimsPrincipal?, TProjection, Task<bool>> filter,
        Expression<Func<TEntity, TProjection>> projection)
    {
        this.filter = filter;
        this.projection = projection;
    }

    public Type EntityType => typeof(TEntity);

    static PropertyInfo GetIdProperty() => typeof(TEntity).GetProperty("Id")!;

    static (Type tupleType, LambdaExpression tupleProjection) BuildTupleProjection(
        Expression<Func<TEntity, TProjection>> projection)
    {
        var idProperty = GetIdProperty();
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var propertyAccess = Expression.Property(parameter, idProperty);

        // Build projection that includes both Id and user's projection: e => (e.Id, projection(e))
        var tupleType = typeof(ValueTuple<,>).MakeGenericType(idProperty.PropertyType, typeof(TProjection));
        var tupleConstructor = tupleType.GetConstructor([idProperty.PropertyType, typeof(TProjection)])!;

        var userProjectionInvoke = Expression.Invoke(projection, parameter);
        var tupleNew = Expression.New(
            tupleConstructor,
            propertyAccess,
            userProjectionInvoke);
        var tupleProjection = Expression.Lambda(tupleNew, parameter);

        return (tupleType, tupleProjection);
    }

    static async Task<Dictionary<object, object>> ExecuteTupleQuery(
        IQueryable<TEntity> queryable,
        Type tupleType,
        LambdaExpression tupleProjection)
    {
        var selectMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == "Select" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TEntity), tupleType);

        var query = selectMethod.Invoke(null, [queryable, tupleProjection])!;

        var toListAsyncMethod = typeof(EntityFrameworkQueryableExtensions).GetMethod("ToListAsync")!
            .MakeGenericMethod(tupleType);
        var resultsTask = (Task) toListAsyncMethod.Invoke(null, [query, Cancel.None])!;
        await resultsTask;

        // Extract result from completed task
        var resultProperty = resultsTask.GetType().GetProperty("Result")!;
        var resultList = (IEnumerable) resultProperty.GetValue(resultsTask)!;

        // Map results by ID - extract from tuple
        var map = new Dictionary<object, object>();
        var item1Property = tupleType.GetField("Item1")!;
        var item2Property = tupleType.GetField("Item2")!;

        foreach (var result in resultList)
        {
            var id = item1Property.GetValue(result)!;
            var projectedData = item2Property.GetValue(result)!;
            map[id] = projectedData;
        }

        return map;
    }

    public async Task<Dictionary<object, object>> QueryProjectedData(IEnumerable<object> entities, TDbContext data)
    {
        var typedEntities = entities.Cast<TEntity>().ToList();
        if (typedEntities.Count == 0)
        {
            return [];
        }

        var idProperty = GetIdProperty();

        // Extract IDs using reflection to maintain the correct type
        var idsListType = typeof(List<>).MakeGenericType(idProperty.PropertyType);
        var idsList = (IList) Activator.CreateInstance(idsListType)!;

        foreach (var entity in typedEntities)
        {
            var id = idProperty.GetValue(entity);
            idsList.Add(id);
        }

        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var propertyAccess = Expression.Property(parameter, idProperty);

        // Build: e => ids.Contains(e.Id)
        var idsConstant = Expression.Constant(idsList, idsListType);
        var containsMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
            .MakeGenericMethod(idProperty.PropertyType);
        var containsCall = Expression.Call(containsMethod, idsConstant, propertyAccess);
        var wherePredicate = Expression.Lambda(containsCall, parameter);

        var (tupleType, tupleProjection) = BuildTupleProjection(projection);

        // Query: data.Set<TEntity>().Where(e => ids.Contains(e.Id)).Select(e => (e.Id, projection(e)))
        var queryable = data.Set<TEntity>()
            .Where((Expression<Func<TEntity, bool>>) wherePredicate);

        return await ExecuteTupleQuery(queryable, tupleType, tupleProjection);
    }

    public async Task<object?> QueryProjectedDataForSingle(object entity, TDbContext data)
    {
        var typedEntity = (TEntity) entity;
        var idProperty = GetIdProperty();
        var id = idProperty.GetValue(typedEntity)!;

        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var propertyAccess = Expression.Property(parameter, idProperty);

        // Build: e => e.Id == id
        var idConstant = Expression.Constant(id, idProperty.PropertyType);
        var equalExpression = Expression.Equal(propertyAccess, idConstant);
        var wherePredicate = Expression.Lambda<Func<TEntity, bool>>(equalExpression, parameter);

        var (tupleType, tupleProjection) = BuildTupleProjection(projection);

        // Query: data.Set<TEntity>().Where(e => e.Id == id).Select(e => (e.Id, projection(e)))
        var queryable = data.Set<TEntity>().Where(wherePredicate);

        var map = await ExecuteTupleQuery(queryable, tupleType, tupleProjection);

        return map.TryGetValue(id, out var projectedData) ? projectedData : null;
    }

    public async Task<bool> ShouldInclude(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        object item,
        Dictionary<(Type, object), object> projectedDataMap)
    {
        var idProperty = typeof(TEntity).GetProperty("Id")!;
        var id = idProperty.GetValue(item)!;

        if (!projectedDataMap.TryGetValue((typeof(TEntity), id), out var projectedItem))
        {
            throw new($"Projected data not found for {typeof(TEntity).Name} with Id {id}");
        }

        return await filter(userContext, data, userPrincipal, (TProjection) projectedItem);
    }
}
