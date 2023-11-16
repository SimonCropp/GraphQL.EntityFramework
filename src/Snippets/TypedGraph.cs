using GraphQL.Types;

public class TypedGraph
{
    #region typedGraph

    public class CompanyGraph :
        EfObjectGraphType<MyDbContext,Company>
    {
        public CompanyGraph(IEfGraphQLService<MyDbContext> graphQlService) :
            base(graphQlService)
        {
            AddNavigationListField(
                name: "employees",
                resolve: _ => _.Source.Employees);
            AddNavigationConnectionField(
                name: "employeesConnection",
                resolve: _ => _.Source.Employees,
                includeNames: ["Employees"]);
            AutoMap();
        }
    }

    #endregion

    public class Company
    {
        public object? Id { get; set; }
        public object? Content { get; set; }
        public List<Employee> Employees { get; set; } = null!;
    }

    public class Employee;

    public class MyDbContext :
        DbContext
    {
        public IQueryable<Company> Companies { get; set; } = null!;
    }

    public class EmployeeGraph :
        ObjectGraphType<Employee>;
}