using System.Linq;
using GraphQL.EntityFramework;
using Microsoft.EntityFrameworkCore;

class ConnectionRootQuery
{
    #region ConnectionRootQuery

    public class Query :
        QueryGraphType<MyDbContext>
    {
        public Query(IEfGraphQLService<MyDbContext> graphQlService) :
            base(graphQlService)
        {
            AddQueryConnectionField(
                name: "companies",
                resolve: context => context.DbContext.Companies);
        }
    }

    #endregion

    public class Company
    {
    }

    class CompanyGraph :
        EfObjectGraphType<MyDbContext, Company>
    {
        public CompanyGraph(IEfGraphQLService<MyDbContext> efGraphQlService) :
            base(efGraphQlService)
        {
        }
    }

    public class MyDbContext :
        DbContext
    {
        public IQueryable<Company> Companies { get; set; } = null!;
    }
}