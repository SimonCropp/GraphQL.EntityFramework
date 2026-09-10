class IncludeAppender(
    IReadOnlyDictionary<Type, IReadOnlyDictionary<string, Navigation>> navigations,
    IReadOnlyDictionary<Type, List<string>> keyNames,
    IReadOnlyDictionary<Type, IReadOnlySet<string>> foreignKeys,
    IReadOnlyDictionary<Type, IReadOnlyList<Type>> derivedTypes)
{
    /// <summary>
    /// Narrow <paramref name="query"/> to the fields the request asked for: a select projection
    /// where the entity can be projected, otherwise includes. A projected navigation whose type
    /// cannot be projected is bound whole, and the navigations under it are then loaded through
    /// includes alongside the select, since EF applies includes to the entities in a projection.
    /// </summary>
    public IQueryable<TItem> ApplyProjection<TDbContext, TItem>(
        IResolveFieldContext context,
        Filters<TDbContext>? filters,
        IQueryable<TItem> query)
        where TDbContext : DbContext
        where TItem : class
    {
        if (context.SubFields is null)
        {
            return query;
        }

        var type = typeof(TItem);
        navigations.TryGetValue(type, out var navigationProperties);
        keyNames.TryGetValue(type, out var keys);
        foreignKeys.TryGetValue(type, out var fks);

        var projection = GetProjectionInfo(context, type, navigationProperties, keys, fks);

        if (filters is { HasFilters: true })
        {
            projection = MergeFilterFieldsIntoProjection(projection, filters, type);
        }

        if (!SelectExpressionBuilder.TryBuild<TItem>(projection, keyNames, derivedTypes, out var expression, out var includePaths, out var argumentFields))
        {
            return AddIncludesFromProjection(query, projection);
        }

        foreach (var includePath in includePaths)
        {
            query = query.Include(includePath);
        }

        PushDown.Mark(context, argumentFields);

        return query.Select(expression);
    }

    IQueryable<TItem> AddIncludesFromProjection<TItem>(
        IQueryable<TItem> query,
        FieldProjectionInfo projection)
        where TItem : class
    {
        if (projection.Navigations is { Count: > 0 })
        {
            foreach (var (navName, navProjection) in projection.Navigations)
            {
                query = query.Include(navName);
                query = AddNestedIncludes(query, navName, typeof(TItem), navName, navProjection);
            }
        }

        // Add derived-type navigation includes for TPH inline fragments
        // e.g. query.Include(e => ((GroupAccessRule)e).Group)
        if (projection.DerivedNavigations is { Count: > 0 })
        {
            query = AddDerivedTypeIncludes(query, projection.DerivedNavigations);
        }

        return query;
    }

    static IQueryable<TItem> AddDerivedTypeIncludes<TItem>(
        IQueryable<TItem> query,
        Dictionary<Type, Dictionary<string, NavigationProjectionInfo>> derivedNavigations)
        where TItem : class
    {
        var itemType = typeof(TItem);
        var parameter = Expression.Parameter(itemType, "e");

        foreach (var (derivedType, navDict) in derivedNavigations)
        {
            // Cast: (DerivedType)e
            var cast = Expression.Convert(parameter, derivedType);

            foreach (var (navName, _) in navDict)
            {
                // Property access: ((DerivedType)e).Navigation
                var property = derivedType.GetProperty(navName);
                if (property == null)
                {
                    continue;
                }

                var propertyAccess = Expression.Property(cast, property);

                // Build lambda: e => ((DerivedType)e).Navigation
                var lambda = Expression.Lambda(propertyAccess, parameter);

                // Call EntityFrameworkQueryableExtensions.Include(query, lambda)
                var includeMethod = GetIncludeMethod(itemType, property.PropertyType);
                query = (IQueryable<TItem>)includeMethod.Invoke(null, [query, lambda])!;
            }
        }

        return query;
    }

    static MethodInfo includeMethodDefinition = typeof(EntityFrameworkQueryableExtensions)
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .First(_ => _.Name == "Include" &&
                    _.GetGenericArguments().Length == 2 &&
                    _.GetParameters().Length == 2 &&
                    _.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>));

    static ConcurrentDictionary<(Type entity, Type property), MethodInfo> includeMethods = new();

    /// <summary>
    /// Both the scan over every public static method on EntityFrameworkQueryableExtensions and the
    /// MakeGenericMethod were being repeated per request. The pairs are bounded by the model, so
    /// they are safe to hold on to.
    /// </summary>
    static MethodInfo GetIncludeMethod(Type entityType, Type propertyType) =>
        includeMethods.GetOrAdd(
            (entityType, propertyType),
            _ => includeMethodDefinition.MakeGenericMethod(_.entity, _.property));

    /// <summary>
    /// The one nested include EF rejects, in a no tracking query, is the inverse of the navigation
    /// just traversed: Attachments then Request, where Request is the other end of Attachments.
    /// EF fixes that inverse up while materializing the include anyway, so skipping it loses
    /// nothing. This used to skip every navigation to a type already on the path, or to a base of
    /// one, which dropped legitimate includes such as a second, unrelated navigation to the root's
    /// base type.
    /// </summary>
    IQueryable<TItem> AddNestedIncludes<TItem>(
        IQueryable<TItem> query,
        string includePath,
        Type parentType,
        string navName,
        NavigationProjectionInfo navProjection)
        where TItem : class
    {
        var projection = navProjection.Projection;
        if (projection.Navigations is not { Count: > 0 })
        {
            return query;
        }

        var inverseName = InverseName(parentType, navName);
        foreach (var (nestedName, nestedProjection) in projection.Navigations)
        {
            if (string.Equals(nestedName, inverseName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var nestedPath = $"{includePath}.{nestedName}";
            query = query.Include(nestedPath);
            query = AddNestedIncludes(query, nestedPath, navProjection.EntityType, nestedName, nestedProjection);
        }

        return query;
    }

    string? InverseName(Type entityType, string navName)
    {
        if (navigations.TryGetValue(entityType, out var properties) &&
            properties.TryGetValue(navName, out var navigation))
        {
            return navigation.InverseName;
        }

        return null;
    }

    FieldProjectionInfo MergeFilterFieldsIntoProjection<TDbContext>(
        FieldProjectionInfo projection,
        Filters<TDbContext> filters,
        Type entityType)
        where TDbContext : DbContext
    {
        navigations.TryGetValue(entityType, out var navigationProperties);

        foreach (var filter in filters.GetFiltersForHierarchy(entityType))
        {
            projection = filter.AddRequirements(projection, navigationProperties);
        }

        // Recursively process existing navigations
        if (projection.Navigations is not { Count: > 0 })
        {
            return projection;
        }

        // Copied only when a nested projection changed, since most do not
        Dictionary<string, NavigationProjectionInfo>? updatedNavigations = null;
        foreach (var (navName, navProjection) in projection.Navigations)
        {
            var updatedProjection = MergeFilterFieldsIntoProjection(navProjection.Projection, filters, navProjection.EntityType);
            if (!ReferenceEquals(updatedProjection, navProjection.Projection))
            {
                updatedNavigations ??= new(projection.Navigations);
                updatedNavigations[navName] = navProjection with { Projection = updatedProjection };
            }
        }

        if (updatedNavigations is null)
        {
            return projection;
        }

        return projection with { Navigations = updatedNavigations };
    }

    FieldProjectionInfo GetProjectionInfo(
        IResolveFieldContext context,
        Type entityType,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        List<string>? keys,
        IReadOnlySet<string>? foreignKeyNames)
    {
        var scalarFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var navProjections = new Dictionary<string, NavigationProjectionInfo>();

        if (context.SubFields is not null)
        {
            var parentGraphType = GetComplexGraphType(context.FieldDefinition);
            foreach (var (field, fieldType) in context.SubFields.Values)
            {
                ProcessField(field, fieldType, parentGraphType, navigationProperties, scalarFields, navProjections, context);
            }
        }

        // Scan for derived-type navigations from inline fragments (TPH support)
        var derivedNavigations = GetDerivedNavigationsFromFragments(
            context.FieldAst.SelectionSet,
            GetComplexGraphType(context.FieldDefinition),
            entityType,
            scalarFields,
            context);

        return new(scalarFields, keys, foreignKeyNames, navProjections, derivedNavigations);
    }

    /// <summary>
    /// The field type carries the projection metadata, and its resolved graph type is what the
    /// selection set below it selects from. GraphQL.NET supplies both for the root sub fields
    /// only, so for nested selection sets they are recovered by walking the schema alongside
    /// the ast. Without that, a projection based field or a connection below the root was
    /// treated as a scalar named after the field, and nothing under it was projected.
    /// </summary>
    void ProcessField(
        GraphQLField field,
        FieldType? fieldType,
        IComplexGraphType? parentGraphType,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        HashSet<string> scalarFields,
        Dictionary<string, NavigationProjectionInfo> navProjections,
        IResolveFieldContext context)
    {
        // SubFields is keyed by response key, which is the alias when one is used.
        // The name of the property to project has to come from the ast node.
        var fieldName = field.Name.StringValue;

        if (IsConnectionWrapper(parentGraphType, fieldName))
        {
            // edges, items and node are wrappers with no property of their own.
            // The entity fields are in the selection set below them.
            ProcessSelectionSet(field.SelectionSet, GetComplexGraphType(fieldType), navigationProperties, scalarFields, navProjections, context);
            return;
        }

        if (fieldType is not null &&
            TryGetProjectionMetadata(fieldType, out var projection))
        {
            ProcessProjectionExpression(field, fieldType, parentGraphType, projection, navigationProperties, scalarFields, navProjections, context);
            return;
        }

        ProcessNavigationOrScalar(fieldName, field, fieldType, navigationProperties, scalarFields, navProjections, context);
    }

    void ProcessSelectionSet(
        GraphQLSelectionSet? selectionSet,
        IComplexGraphType? graphType,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        HashSet<string> scalarFields,
        Dictionary<string, NavigationProjectionInfo> navProjections,
        IResolveFieldContext context)
    {
        if (selectionSet?.Selections is null)
        {
            return;
        }

        foreach (var (field, fieldGraphType) in EnumerateFields(selectionSet, graphType, context))
        {
            var fieldType = fieldGraphType?.GetField(field.Name.Value);
            ProcessField(field, fieldType, fieldGraphType, navigationProperties, scalarFields, navProjections, context);
        }
    }

    /// <summary>
    /// The graph type a field's selection set selects from. Null for a scalar or a field that could
    /// not be resolved, in which case the selection set is matched against the entity by name alone.
    /// </summary>
    static IComplexGraphType? GetComplexGraphType(FieldType? fieldType) =>
        fieldType?.ResolvedType?.GetNamedType() as IComplexGraphType;

    /// <summary>
    /// Navigations selected through a fragment on a type derived from the entity type. They do
    /// not exist on the entity type itself, so they are collected per derived type and included
    /// with a cast. Fragments on the entity type itself, or on one of its base types, select
    /// navigations the main projection already covers, so they are skipped. Runs for the root
    /// selection and for every nested one; it used to run for the root only, so a derived
    /// navigation under a fragment below a navigation fell through to the scalar path and was
    /// dropped.
    /// </summary>
    Dictionary<Type, Dictionary<string, NavigationProjectionInfo>>? GetDerivedNavigationsFromFragments(
        GraphQLSelectionSet? selectionSet,
        IComplexGraphType? graphType,
        Type entityType,
        HashSet<string> scalarFields,
        IResolveFieldContext context)
    {
        IComplexGraphType? leafGraphType;
        (selectionSet, leafGraphType) = GetLeafSelection(selectionSet, graphType);
        if (selectionSet?.Selections is null)
        {
            return null;
        }

        Dictionary<Type, Dictionary<string, NavigationProjectionInfo>>? result = null;

        foreach (var (field, fieldGraphType) in EnumerateFields(selectionSet, leafGraphType, context))
        {
            if (fieldGraphType is null ||
                ReferenceEquals(fieldGraphType, leafGraphType) ||
                !TryFindDerivedClrType(fieldGraphType, out var derivedType) ||
                derivedType == entityType ||
                !entityType.IsAssignableFrom(derivedType))
            {
                continue;
            }

            navigations.TryGetValue(derivedType, out var derivedNavProps);
            if (derivedNavProps is null ||
                !derivedNavProps.TryGetValue(field.Name.StringValue, out var navigation))
            {
                // A scalar of the derived type. The root sub fields exclude fields conditional on
                // another type, so it is recorded here; it resolves against the derived type's
                // properties when that type's member init is built, and is skipped for the base.
                scalarFields.Add(field.Name.StringValue);
                continue;
            }

            // Below the root, the selection set walk sees the field before this does, and with
            // only the base type's navigations to match against records it as a scalar. It is a
            // navigation of the derived type, so it is projected as one and not as a member.
            scalarFields.Remove(field.Name.StringValue);

            result ??= [];
            if (!result.TryGetValue(derivedType, out var derivedNavs))
            {
                derivedNavs = [];
                result[derivedType] = derivedNavs;
            }

            var navType = navigation.Type;
            navigations.TryGetValue(navType, out var nestedNavProps);
            keyNames.TryGetValue(navType, out var nestedKeys);
            foreignKeys.TryGetValue(navType, out var nestedFks);

            var navGraphType = GetComplexGraphType(fieldGraphType.GetField(field.Name.Value));
            AddNavigation(
                derivedNavs,
                navigation.Name,
                new(
                    navType,
                    navigation.IsCollection,
                    GetNestedProjection(field.SelectionSet, navGraphType, navType, nestedNavProps, nestedKeys, nestedFks, context)));
        }

        return result;
    }

    /// <summary>
    /// Navigate through connection wrapper fields (edges/items/node) to find the leaf selection set
    /// that contains the actual entity fields and inline fragments, and the graph type it selects from.
    /// </summary>
    static (GraphQLSelectionSet? SelectionSet, IComplexGraphType? GraphType) GetLeafSelection(
        GraphQLSelectionSet? selectionSet,
        IComplexGraphType? graphType)
    {
        if (selectionSet?.Selections is null)
        {
            return (null, null);
        }

        // Drill through connection wrapper fields
        while (true)
        {
            var found = false;
            foreach (var selection in selectionSet.Selections)
            {
                if (selection is GraphQLField { SelectionSet: not null } field &&
                    IsConnectionWrapper(graphType, field.Name.StringValue))
                {
                    graphType = GetComplexGraphType(graphType?.GetField(field.Name.Value));
                    selectionSet = field.SelectionSet;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                break;
            }
        }

        return (selectionSet, graphType);
    }

    bool TryFindDerivedClrType(IGraphType graphType, [NotNullWhen(true)] out Type? clrType)
    {
        clrType = null;

        // Walk the type hierarchy to find the CLR type from the generic arguments
        var graphClrType = GetSourceType(graphType.GetType());
        if (graphClrType is not null && navigations.ContainsKey(graphClrType))
        {
            clrType = graphClrType;
            return true;
        }

        // Fallback: match CLR type name directly
        foreach (var type in navigations.Keys)
        {
            if (string.Equals(type.Name, graphType.Name, StringComparison.OrdinalIgnoreCase))
            {
                clrType = type;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The clr type of what a field returns, through a list or connection. Null when the graph
    /// type is not one of the library's typed graph types.
    /// </summary>
    static Type? FieldItemType(FieldType fieldType)
    {
        var namedType = fieldType.ResolvedType?.GetNamedType();
        if (namedType is null)
        {
            return null;
        }

        var graphType = namedType.GetType();
        for (var type = graphType; type is not null; type = type.BaseType)
        {
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(ConnectionType<,>))
            {
                return GetSourceType(type.GenericTypeArguments[0]);
            }
        }

        return GetSourceType(graphType);
    }

    static Type? GetSourceType(Type graphType)
    {
        var type = graphType;
        while (type is not null)
        {
            if (type.IsGenericType)
            {
                var genericDef = type.GetGenericTypeDefinition();
                if (genericDef == typeof(ObjectGraphType<>) ||
                    genericDef == typeof(InterfaceGraphType<>))
                {
                    return type.GenericTypeArguments[0];
                }
            }

            type = type.BaseType;
        }

        return null;
    }

    /// <summary>
    /// Whether a field is a connection wrapper, edges, items or node, with the entity fields in
    /// the selection set below it. Decided by the graph type the field is selected from, since an
    /// entity can have a navigation with one of those names, and matching on the name alone left
    /// such a navigation never projected. The name is only relied on when the graph type could
    /// not be resolved.
    /// </summary>
    static bool IsConnectionWrapper(IComplexGraphType? parentGraphType, string fieldName)
    {
        if (!IsConnectionNodeName(fieldName))
        {
            return false;
        }

        if (parentGraphType is null)
        {
            return true;
        }

        return IsConnectionOrEdgeType(parentGraphType.GetType());
    }

    static ConcurrentDictionary<Type, bool> connectionTypes = new();

    static bool IsConnectionOrEdgeType(Type graphType) =>
        connectionTypes.GetOrAdd(
            graphType,
            _ =>
            {
                for (var type = _; type is not null; type = type.BaseType)
                {
                    if (type.IsGenericType)
                    {
                        var definition = type.GetGenericTypeDefinition();
                        if (definition == typeof(ConnectionType<,>) ||
                            definition == typeof(ConnectionType<>) ||
                            definition == typeof(EdgeType<>))
                        {
                            return true;
                        }
                    }
                }

                return false;
            });

    static bool IsConnectionNodeName(string fieldName) =>
        fieldName.Equals("edges", StringComparison.OrdinalIgnoreCase) ||
        fieldName.Equals("items", StringComparison.OrdinalIgnoreCase) ||
        fieldName.Equals("node", StringComparison.OrdinalIgnoreCase);

    void ProcessProjectionExpression(
        GraphQLField field,
        FieldType fieldType,
        IComplexGraphType? parentGraphType,
        LambdaExpression projection,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        HashSet<string> scalarFields,
        Dictionary<string, NavigationProjectionInfo> navProjections,
        IResolveFieldContext context)
    {
        var accessedPaths = ProjectionAnalyzer.ExtractPropertyPaths(projection);

        // Group paths by their root navigation property
        var pathsByNavigation = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        string? primaryNavigation = null;

        foreach (var path in accessedPaths)
        {
            var dotIndex = path.IndexOf('.');
            var rootProperty = dotIndex >= 0 ? path[..dotIndex] : path;

            if (!pathsByNavigation.TryGetValue(rootProperty, out var paths))
            {
                paths = [];
                pathsByNavigation[rootProperty] = paths;
            }

            if (dotIndex >= 0)
            {
                paths.Add(path[(dotIndex + 1)..]);
            }

            primaryNavigation ??= rootProperty;
        }

        // The field's selection set applies to the navigations of the type the field returns.
        // It used to go to whichever path the analyzer visited first, so with `new { _.Child2,
        // _.Child1 }` resolving Child1, Child2 got the selection and Child1 got nothing under it.
        var itemType = FieldItemType(fieldType);

        foreach (var (navName, nestedPaths) in pathsByNavigation)
        {
            if (!TryFindNavigation(navigationProperties, navName, out var navigation))
            {
                // Scalar field path (no navigation) — add root property to scalarFields
                scalarFields.Add(navName);
                continue;
            }

            var receivesSelection = itemType is null
                ? navName == primaryNavigation
                : itemType.IsAssignableFrom(navigation.Type) || navigation.Type.IsAssignableFrom(itemType);

            var navType = navigation.Type;
            navigations.TryGetValue(navType, out var nestedNavProps);
            keyNames.TryGetValue(navType, out var nestedKeys);
            foreignKeys.TryGetValue(navType, out var nestedFks);

            FieldProjectionInfo nestedProjection;
            NavigationArguments? arguments = null;

            // A navigation accessed as a whole, with nothing read from it in the expression and no
            // selection set to say which fields are wanted, needs the whole entity. It was
            // projected with only its keys, so the resolver saw every other property as null.
            var isWhole = nestedPaths.Count == 0;

            if (receivesSelection && field.SelectionSet is not null)
            {
                // A navigation list or connection field projecting the collection itself. Its ids,
                // where and orderBy are applied inside the collection subquery.
                if (navigation.IsCollection &&
                    pathsByNavigation.Count == 1 &&
                    nestedPaths.Count == 0)
                {
                    arguments = ReadArguments(field, fieldType, parentGraphType, context);
                }

                // Primary navigation: merge GraphQL fields with projection-required fields
                nestedProjection = GetNestedProjection(field.SelectionSet, GetComplexGraphType(fieldType), navType, nestedNavProps, nestedKeys, nestedFks, context);
                foreach (var nestedPath in nestedPaths)
                {
                    if (!nestedPath.Contains('.'))
                    {
                        nestedProjection.ScalarFields.Add(nestedPath);
                    }
                }

                isWhole = false;
            }
            else
            {
                // Secondary navigation: include only projection-required fields
                var nestedScalarFields = nestedPaths
                    .Where(_ => !_.Contains('.'))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                nestedProjection = new(nestedScalarFields, nestedKeys ?? [], nestedFks ?? new HashSet<string>(), []);
            }

            AddNavigation(navProjections, navigation.Name, new(navType, navigation.IsCollection, nestedProjection, isWhole, arguments));
        }
    }

    /// <summary>
    /// The ids, where and orderBy of a navigation list or connection field, read through a field
    /// context of the field's own, since the values can come from variables and the readers
    /// convert them the same way the resolver would. Only fields carrying the library's where
    /// argument qualify, so a user defined argument of the same name is left alone.
    /// </summary>
    static NavigationArguments? ReadArguments(
        GraphQLField field,
        FieldType fieldType,
        IComplexGraphType? parentGraphType,
        IResolveFieldContext context)
    {
        if (field.Arguments is not { Count: > 0 } ||
            fieldType.Arguments?.Find("where")?.ResolvedType?.GetNamedType() is not IWhereGraph)
        {
            return null;
        }

        var values = ExecutionHelper.GetArguments(fieldType.Arguments, field.Arguments, context.Variables, context.Document, field, null);
        if (values is null)
        {
            return null;
        }

        var fieldContext = new ResolveFieldContext
        {
            Arguments = values,
            FieldAst = field,
            FieldDefinition = fieldType,
            ParentType = (parentGraphType as IObjectGraphType)!,
            Schema = context.Schema,
            Document = context.Document,
            Operation = context.Operation,
            Variables = context.Variables,
            Errors = context.Errors,
            UserContext = context.UserContext,
            RequestServices = context.RequestServices,
            CancellationToken = context.CancellationToken
        };

        ArgumentReader.TryReadWhere(fieldContext, out var wheres);
        var orderBys = ArgumentReader.ReadOrderBy(fieldContext);
        ArgumentReader.TryReadIds(fieldContext, out var ids);

        if (wheres.Count == 0 &&
            orderBys.Count == 0 &&
            ids is null)
        {
            return null;
        }

        return new(field, wheres, orderBys, ids);
    }

    static bool TryFindNavigation(
        IReadOnlyDictionary<string, Navigation>? properties,
        string name,
        [NotNullWhen(true)] out Navigation? navigation)
    {
        navigation = null;
        return properties is not null &&
               properties.TryGetValue(name, out navigation);
    }

    FieldProjectionInfo GetNestedProjection(
        GraphQLSelectionSet? selectionSet,
        IComplexGraphType? graphType,
        Type entityType,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        List<string>? keys,
        IReadOnlySet<string>? foreignKeyNames,
        IResolveFieldContext context)
    {
        var scalarFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var navProjections = new Dictionary<string, NavigationProjectionInfo>();

        ProcessSelectionSet(selectionSet, graphType, navigationProperties, scalarFields, navProjections, context);

        var derivedNavigations = GetDerivedNavigationsFromFragments(selectionSet, graphType, entityType, scalarFields, context);

        return new(scalarFields, keys, foreignKeyNames, navProjections, derivedNavigations);
    }

    /// <summary>
    /// The fields in a selection set, including those inside fragments at any depth. A fragment
    /// can name a type other than the parent's, so each field is paired with the graph type it
    /// selects from. Validation rejects fragment cycles, but they are guarded against anyway,
    /// since the recursion would otherwise never end.
    /// </summary>
    static IEnumerable<(GraphQLField Field, IComplexGraphType? GraphType)> EnumerateFields(
        GraphQLSelectionSet selectionSet,
        IComplexGraphType? graphType,
        IResolveFieldContext context,
        HashSet<string>? spreadsInProgress = null)
    {
        foreach (var selection in selectionSet.Selections)
        {
            switch (selection)
            {
                case GraphQLField field:
                    yield return (field, graphType);
                    break;
                case GraphQLInlineFragment inlineFragment:
                {
                    var fragmentGraphType = ResolveTypeCondition(inlineFragment.TypeCondition, graphType, context);
                    foreach (var item in EnumerateFields(inlineFragment.SelectionSet, fragmentGraphType, context, spreadsInProgress))
                    {
                        yield return item;
                    }

                    break;
                }
                case GraphQLFragmentSpread fragmentSpread:
                {
                    var name = fragmentSpread.FragmentName.Name;
                    var fragmentDefinition = FindFragment(context.Document, name);

                    if (fragmentDefinition?.SelectionSet.Selections is null)
                    {
                        break;
                    }

                    spreadsInProgress ??= [];
                    if (!spreadsInProgress.Add(name.StringValue))
                    {
                        break;
                    }

                    var fragmentGraphType = ResolveTypeCondition(fragmentDefinition.TypeCondition, graphType, context);
                    foreach (var item in EnumerateFields(fragmentDefinition.SelectionSet, fragmentGraphType, context, spreadsInProgress))
                    {
                        yield return item;
                    }

                    spreadsInProgress.Remove(name.StringValue);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Validation already rejects duplicate fragment names, so the first match is the definition.
    /// SingleOrDefault kept scanning the rest of the document after finding it.
    /// </summary>
    static GraphQLFragmentDefinition? FindFragment(GraphQLDocument document, GraphQLName name)
    {
        foreach (var definition in document.Definitions)
        {
            if (definition is GraphQLFragmentDefinition fragment &&
                fragment.FragmentName.Name == name)
            {
                return fragment;
            }
        }

        return null;
    }

    static IComplexGraphType? ResolveTypeCondition(
        GraphQLTypeCondition? typeCondition,
        IComplexGraphType? parentGraphType,
        IResolveFieldContext context)
    {
        if (typeCondition is null)
        {
            return parentGraphType;
        }

        return context.Schema.AllTypes[typeCondition.Type.Name.StringValue] as IComplexGraphType ?? parentGraphType;
    }

    void ProcessNavigationOrScalar(
        string fieldName,
        GraphQLField field,
        FieldType? fieldType,
        IReadOnlyDictionary<string, Navigation>? navigationProperties,
        HashSet<string> scalarFields,
        Dictionary<string, NavigationProjectionInfo> navProjections,
        IResolveFieldContext context)
    {
        Navigation? navigation = null;
        navigationProperties?.TryGetValue(fieldName, out navigation);

        if (navigation == null)
        {
            scalarFields.Add(fieldName);
            return;
        }

        var navType = navigation.Type;
        navigations.TryGetValue(navType, out var nestedNavProps);
        keyNames.TryGetValue(navType, out var nestedKeys);
        foreignKeys.TryGetValue(navType, out var nestedFks);

        AddNavigation(
            navProjections,
            navigation.Name,
            new(
                navType,
                navigation.IsCollection,
                GetNestedProjection(field.SelectionSet, GetComplexGraphType(fieldType), navType, nestedNavProps, nestedKeys, nestedFks, context)));
    }

    /// <summary>
    /// The same navigation can be selected more than once: under two aliases, through a fragment
    /// as well as directly, or by a projection based field alongside the navigation field itself.
    /// Each selection can ask for different fields, so they are merged rather than the first or
    /// the last one winning.
    /// </summary>
    static void AddNavigation(
        Dictionary<string, NavigationProjectionInfo> navProjections,
        string name,
        NavigationProjectionInfo navProjection) =>
        navProjections[name] = navProjections.TryGetValue(name, out var existing)
            ? existing.Merge(navProjection)
            : navProjection;

    public static void SetProjectionMetadata(FieldType fieldType, LambdaExpression projection) =>
        fieldType.Metadata["_EF_Projection"] = projection;

    static bool TryGetProjectionMetadata(FieldType fieldType, [NotNullWhen(true)] out LambdaExpression? projection)
    {
        if (fieldType.Metadata.TryGetValue("_EF_Projection", out var projectionObj))
        {
            projection = (LambdaExpression)projectionObj!;
            return true;
        }

        projection = null;
        return false;
    }
}
