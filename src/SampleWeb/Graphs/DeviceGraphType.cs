public class DeviceGraphType :
    EfObjectGraphType<SampleDbContext, Device>
{
    public DeviceGraphType(IEfGraphQLService<SampleDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationListField(
            name: "employees",
            projection: _ => _.Employees,
            resolve: ctx => ctx.Projection);
        AutoMap();
    }
}