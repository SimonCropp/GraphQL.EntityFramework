using System.Linq;
using GraphQL.EntityFramework;
using Microsoft.EntityFrameworkCore;

class RootQuery
{
    #region rootQuery

    public class Query :
        QueryGraphType<MyDbContext>
    {
        public Query(IEfGraphQLService<MyDbContext> graphQlService) :
            base(graphQlService)
        {
            AddSingleField(
                resolve: context =>
                {
                    var dbContext = (MyDbContext) context.UserContext;
                    return dbContext.Companies;
                },
                name: "company");
            AddQueryField(
                name: "companies",
                resolve: context =>
                {
                    var dbContext = (MyDbContext) context.UserContext;
                    return dbContext.Companies;
                });
        }
    }

    #endregion

    public class MyDbContext:DbContext
    {
        public IQueryable<Company> Companies { get; set; }
    }

    public class Company
    {
    }

    class CompanyGraph:
        EfObjectGraphType<MyDbContext,Company>
    {
        public CompanyGraph(IEfGraphQLService<MyDbContext> efGraphQlService) :
            base(efGraphQlService)
        {
        }
    }
}