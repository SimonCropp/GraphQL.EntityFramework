using System.Linq;
using GraphQL.EntityFramework;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

class ResolveDbContextQuery
{
    #region QueryResolveDbContext

    public class Query :
        QueryGraphType<MyDbContext>
    {
        public Query(IEfGraphQLService<MyDbContext> graphQlService) :
            base(graphQlService)
        {
            Field<ListGraphType<CompanyGraph>>(
                name: "oldCompanies",
                resolve: context =>
                {
                    // uses the base QueryGraphType to resolve the db context
                    var dbContext = ResolveDbContext(context);
                    return dbContext.Companies.Where(x => x.Age > 10);
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
        public int Age { get; set; }
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