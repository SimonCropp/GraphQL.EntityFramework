public class Schema :
    GraphQL.Types.Schema
{
    public Schema(IServiceProvider provider, Query query) :
        base(provider)
    {
        RegisterTypeMapping(typeof(Employee), typeof(EmployeeGraphType));
        RegisterTypeMapping(typeof(EmployeeSummary), typeof(EmployeeSummaryGraphType));
        RegisterTypeMapping(typeof(Company), typeof(CompanyGraphType));
        Query = query;
        //   Subscription = (Subscription)provider.GetService(typeof(Subscription));
    }
}