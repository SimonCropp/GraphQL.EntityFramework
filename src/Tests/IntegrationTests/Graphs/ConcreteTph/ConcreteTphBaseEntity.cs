// A TPH hierarchy whose base type is concrete, so rows of both types come back from the base set
public class ConcreteTphBaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
}

public class ConcreteTphDerivedEntity :
    ConcreteTphBaseEntity
{
    public string? DerivedProperty { get; set; }
}

public class ConcreteTphInterfaceGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
    EfInterfaceGraphType<IntegrationDbContext, ConcreteTphBaseEntity>(graphQlService);

public class ConcreteTphBaseGraphType :
    EfObjectGraphType<IntegrationDbContext, ConcreteTphBaseEntity>
{
    public ConcreteTphBaseGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        Name = "ConcreteTphBase";
        AutoMap();
        Interface<ConcreteTphInterfaceGraphType>();
        IsTypeOf = _ => _.GetType() == typeof(ConcreteTphBaseEntity);
    }
}

public class ConcreteTphDerivedGraphType :
    EfObjectGraphType<IntegrationDbContext, ConcreteTphDerivedEntity>
{
    public ConcreteTphDerivedGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
        Interface<ConcreteTphInterfaceGraphType>();
        IsTypeOf = _ => _ is ConcreteTphDerivedEntity;
    }
}
