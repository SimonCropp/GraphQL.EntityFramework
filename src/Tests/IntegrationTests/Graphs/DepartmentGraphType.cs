public class DepartmentGraphType :
    EfObjectGraphType<IntegrationDbContext, DepartmentEntity>
{
    public DepartmentGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}
