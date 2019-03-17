using System.Collections.Generic;
using GraphQL.EntityFramework;
using GraphQL.Types;

public class TypedGraph
{
    #region typedGraph

    public class CompanyGraph :
        EfObjectGraphType<Company>
    {
        public CompanyGraph(IEfGraphQLService graphQlService) :
            base(graphQlService)
        {
            Field(x => x.Id);
            Field(x => x.Content);
            AddNavigationField<Employee>(
                typeof(EmployeeGraph),
                name: "employees",
                resolve: context => context.Source.Employees);
            AddNavigationConnectionField(
                name: "employeesConnection",
                resolve: context => context.Source.Employees,
                typeof(EmployeeGraph),
                includeNames: new[] {"Employees"});
        }
    }

    #endregion

    public class Company
    {
        public object Id { get; set; }
        public object Content { get; set; }
        public List<Employee> Employees { get; set; }
    }

    public class Employee
    {
    }

    public class EmployeeGraph :
        ObjectGraphType<Employee>
    {
    }
}