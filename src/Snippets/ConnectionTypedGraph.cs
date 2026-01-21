using GraphQL.Types;

class ConnectionTypedGraph
{
    #region ConnectionTypedGraph

    public class CompanyGraph :
        EfObjectGraphType<MyDbContext, Company>
    {
        public CompanyGraph(IEfGraphQLService<MyDbContext> graphQlService) :
            base(graphQlService) =>
            AddNavigationConnectionField(
                name: "employees",
                projection: _ => _.Employees,
                resolve: _ => _.Projection);
    }

    #endregion

    internal class MyDbContext :
        DbContext;

    public class Company
    {
        public List<Employee> Employees { get; set; } = null!;
    }

    public class Employee;

    public class EmployeeGraph :
        ObjectGraphType<Employee>;
}
