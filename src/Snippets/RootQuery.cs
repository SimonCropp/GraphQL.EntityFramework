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
                resolve: context => context.DbContext.Companies,
                name: "company");
            AddQueryField(
                name: "companies",
                resolve: context => context.DbContext.Companies);
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