public class EmployeeGraphType :
    EfObjectGraphType<IntegrationDbContext, EmployeeEntity>
{
    public EmployeeGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        // Custom field that uses the foreign key to query related data
        // This simulates the isGovernmentMember/isCabinetMinister scenario
        Field<NonNullGraphType<BooleanGraphType>>("isInActiveDepartment")
            .ResolveAsync(async context =>
            {
                var dbContext = ResolveDbContext(context);
                var department = await dbContext.Departments
                    .SingleAsync(_ => _.Id == context.Source.DepartmentId);
                return department.IsActive;
            });

        AutoMap();
    }
}
