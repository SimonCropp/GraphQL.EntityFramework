public class TphDerivedNavBaseGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
    EfInterfaceGraphType<IntegrationDbContext, TphDerivedNavBaseEntity>(graphQlService);
