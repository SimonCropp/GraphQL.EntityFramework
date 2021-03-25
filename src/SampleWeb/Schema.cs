using System;

public class Schema :
    GraphQL.Types.Schema
{
    public Schema(IServiceProvider provider, Query query) :
        base(provider)
    {
        RegisterTypeMapping(typeof(Employee), typeof(EmployeeGraph));
        RegisterTypeMapping(typeof(EmployeeSummary), typeof(EmployeeSummaryGraph));
        RegisterTypeMapping(typeof(Company), typeof(CompanyGraph));
        Query = query;
        //   Subscription = (Subscription)provider.GetService(typeof(Subscription));
    }
}