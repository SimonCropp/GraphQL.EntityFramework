public class Schema :
    GraphQL.Types.Schema
{
    public Schema(IServiceProvider resolver) :
        base(resolver)
    {
        RegisterTypeMapping(typeof(FilterChildEntity), typeof(FilterChildGraphType));
        RegisterTypeMapping(typeof(FilterParentEntity), typeof(FilterParentGraphType));
        RegisterTypeMapping(typeof(WithManyChildrenEntity), typeof(WithManyChildrenGraphType));
        RegisterTypeMapping(typeof(CustomTypeEntity), typeof(CustomTypeGraphType));
        RegisterTypeMapping(typeof(Child1Entity), typeof(Child1GraphType));
        RegisterTypeMapping(typeof(Child2Entity), typeof(Child2GraphType));
        RegisterTypeMapping(typeof(ChildEntity), typeof(ChildGraphType));
        RegisterTypeMapping(typeof(ParentEntity), typeof(ParentGraphType));
        RegisterTypeMapping(typeof(DateEntity), typeof(DateEntityGraphType));
        RegisterTypeMapping(typeof(EnumEntity), typeof(EnumEntityGraphType));
        RegisterTypeMapping(typeof(StringEntity), typeof(StringEntityGraphType));
        RegisterTypeMapping(typeof(TimeEntity), typeof(TimeEntityGraphType));
        RegisterTypeMapping(typeof(TimeEntity), typeof(TimeEntityGraphType));
        RegisterTypeMapping(typeof(Level1Entity), typeof(Level1GraphType));
        RegisterTypeMapping(typeof(Level2Entity), typeof(Level2GraphType));
        RegisterTypeMapping(typeof(Level3Entity), typeof(Level3GraphType));
        RegisterTypeMapping(typeof(IncludeNonQueryableB), typeof(IncludeNonQueryableBGraphType));
        RegisterTypeMapping(typeof(IncludeNonQueryableA), typeof(IncludeNonQueryableAGraphType));
        RegisterTypeMapping(typeof(WithMisNamedQueryParentEntity), typeof(WithMisNamedQueryParentGraphType));
        RegisterTypeMapping(typeof(WithMisNamedQueryChildEntity), typeof(WithMisNamedQueryChildGraphType));
        RegisterTypeMapping(typeof(WithNullableEntity), typeof(WithNullableGraphType));
        RegisterTypeMapping(typeof(NamedIdEntity), typeof(NamedIdGraphType));
        RegisterTypeMapping(typeof(DerivedEntity), typeof(DerivedGraphType));
        RegisterTypeMapping(typeof(DerivedWithNavigationEntity), typeof(DerivedWithNavigationGraphType));
        RegisterTypeMapping(typeof(DerivedChildEntity), typeof(DerivedChildGraphType));
        RegisterTypeMapping(typeof(ManyToManyLeftEntity), typeof(ManyToManyLeftGraphType));
        RegisterTypeMapping(typeof(ManyToManyRightEntity), typeof(ManyToManyRightGraphType));
        RegisterTypeMapping(typeof(ParentEntityView), typeof(ParentEntityViewGraphType));
        RegisterTypeMapping(typeof(OwnedParent), typeof(OwnedParentGraphType));
        RegisterTypeMapping(typeof(OwnedChild), typeof(OwnedChildGraphType));
        Query = (Query)resolver.GetService(typeof(Query))!;
        Mutation = (Mutation)resolver.GetService(typeof(Mutation))!;
        RegisterType(typeof(DerivedGraphType));
        RegisterType(typeof(DerivedWithNavigationGraphType));
    }
}