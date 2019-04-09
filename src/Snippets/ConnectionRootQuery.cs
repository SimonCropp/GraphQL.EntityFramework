using System.Linq;
using GraphQL.EntityFramework;

class ConnectionRootQuery
{
    #region ConnectionRootQuery

    public class Query :
        QueryGraphType
    {
        public Query(IEfGraphQLService graphQlService) :
            base(graphQlService)
        {
            AddQueryConnectionField(
                name: "companies",
                resolve: context =>
                {
                    var dbContext = (MyDbContext) context.UserContext;
                    return dbContext.Companies;
                });
        }
    }

    #endregion

    class Company
    {
    }

    class CompanyGraph :
        EfObjectGraphType<Company>
    {
        public CompanyGraph(IEfGraphQLService efGraphQlService) :
            base(efGraphQlService)
        {
        }
    }

    class MyDbContext
    {
        public IQueryable<Company> Companies { get; set; }
    }
}