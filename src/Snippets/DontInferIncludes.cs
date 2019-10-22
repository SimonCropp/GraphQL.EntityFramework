using System.Collections.Generic;
using System.Linq;
using GraphQL.EntityFramework;
using Microsoft.EntityFrameworkCore;

class DontInferIncludes
{
    #region inferIncludes

    public class Query :
        QueryGraphType<MyDbContext>
    {
        public Query(IEfGraphQLService<MyDbContext> graphQlService) :
            base(graphQlService)
        {
            AddQueryField(
                name: "newCompanies",
                inferIncludes: false,
                resolve: context =>
                {
                    return context.DbContext.Companies
                        .Include(x => x.Employees)
                        .Where(x => x.Age > 10);
                });
        }
    }

    #endregion

    public class MyDbContext :
        DbContext
    {
        public IQueryable<Company> Companies { get; set; } = null!;
    }

    public class Company
    {
        public List<Employee> Employees = null!;
        public int Age { get; set; }
    }

    public class Employee
    {
    }
}