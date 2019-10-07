using System.Collections.Generic;
using GraphQL.EntityFramework;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

class ConnectionTypedGraph
{
    #region ConnectionTypedGraph

    public class CompanyGraph :
        EfObjectGraphType<MyDbContext, Company>
    {
        public CompanyGraph(IEfGraphQLService<MyDbContext> graphQlService) :
            base(graphQlService)
        {
            AddNavigationConnectionField(
                name: "employees",
                resolve: context => context.Source.Employees);
        }
    }

    #endregion

    internal class MyDbContext :
        DbContext
    {
    }

    public class Company
    {
        public List<Employee> Employees { get; set; } = null!;
    }

    public class Employee
    {
    }

    public class EmployeeGraph :
        ObjectGraphType<Employee>
    {
    }
}