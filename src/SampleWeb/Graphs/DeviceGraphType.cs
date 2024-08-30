public class DeviceGraphType :
    EfObjectGraphType<SampleDbContext, Device>
{
    public DeviceGraphType(IEfGraphQLService<SampleDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationListField(
            name: "employees",
            resolve: _ => _.Source.Employees);
        AutoMap();
    }
}