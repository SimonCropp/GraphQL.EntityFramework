using System.Collections.Generic;
using GraphQL.EntityFramework;
using GraphQL.Types;

class ConnectionTypedGraph
{
    #region ConnectionTypedGraph

    public class CompanyGraph :
        EfObjectGraphType<Company>
    {
        public CompanyGraph(IEfGraphQLService graphQlService) :
            base(graphQlService)
        {
            AddNavigationConnectionField<EmployeeGraph, Employee>(
                name: "employees",
                resolve: context => context.Source.Employees);
        }
    }

    #endregion

    public class Company
    {
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