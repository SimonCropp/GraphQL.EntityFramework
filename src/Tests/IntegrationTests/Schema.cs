using System;

public class Schema :
    GraphQL.Types.Schema
{
    public Schema(IServiceProvider resolver) :
        base(resolver)
    {
        RegisterTypeMapping(typeof(FilterChildEntity), typeof(FilterChildGraph));
        RegisterTypeMapping(typeof(FilterParentEntity), typeof(FilterParentGraph));
        RegisterTypeMapping(typeof(WithManyChildrenEntity), typeof(WithManyChildrenGraph));
        RegisterTypeMapping(typeof(CustomTypeEntity), typeof(CustomTypeGraph));
        RegisterTypeMapping(typeof(Child1Entity), typeof(Child1Graph));
        RegisterTypeMapping(typeof(Child2Entity), typeof(Child2Graph));
        RegisterTypeMapping(typeof(ChildEntity), typeof(ChildGraph));
        RegisterTypeMapping(typeof(ParentEntity), typeof(ParentGraph));
        RegisterTypeMapping(typeof(Level1Entity), typeof(Level1Graph));
        RegisterTypeMapping(typeof(Level2Entity), typeof(Level2Graph));
        RegisterTypeMapping(typeof(Level3Entity), typeof(Level3Graph));
        RegisterTypeMapping(typeof(IncludeNonQueryableB), typeof(IncludeNonQueryableBGraph));
        RegisterTypeMapping(typeof(IncludeNonQueryableA), typeof(IncludeNonQueryableAGraph));
        RegisterTypeMapping(typeof(WithMisNamedQueryParentEntity), typeof(WithMisNamedQueryParentGraph));
        RegisterTypeMapping(typeof(WithNullableEntity), typeof(WithNullableGraph));
        RegisterTypeMapping(typeof(NamedIdEntity), typeof(NamedIdGraph));
        RegisterTypeMapping(typeof(WithMisNamedQueryChildEntity), typeof(WithMisNamedQueryChildGraph));
        RegisterTypeMapping(typeof(DerivedEntity), typeof(DerivedGraph));
        RegisterTypeMapping(typeof(DerivedWithNavigationEntity), typeof(DerivedWithNavigationGraph));
        RegisterTypeMapping(typeof(DerivedChildEntity), typeof(DerivedChildGraph));
        RegisterTypeMapping(typeof(ManyToManyLeftEntity), typeof(ManyToManyLeftGraph));
        RegisterTypeMapping(typeof(ManyToManyRightEntity), typeof(ManyToManyRightGraph));
        RegisterTypeMapping(typeof(ParentEntityView), typeof(ParentEntityViewGraph));
        Query = (Query)resolver.GetService(typeof(Query))!;
        Mutation = (Mutation)resolver.GetService(typeof(Mutation))!;
        RegisterType(typeof(DerivedGraph));
        RegisterType(typeof(DerivedWithNavigationGraph));
    }
}