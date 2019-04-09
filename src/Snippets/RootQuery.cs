using System.Linq;
using GraphQL.EntityFramework;

class RootQuery
{
    #region rootQuery

    public class Query :
        EfObjectGraphType
    {
        public Query(IEfGraphQLService graphQlService) :
            base(graphQlService)
        {
            AddSingleField(
                resolve: context =>
                {
                    var dbContext = (DbContext) context.UserContext;
                    return dbContext.Companies;
                },
                name: "company");
            AddQueryField(
                name: "companies",
                resolve: context =>
                {
                    var dbContext = (DbContext) context.UserContext;
                    return dbContext.Companies;
                });
        }
    }

    #endregion

    class DbContext
    {
        public IQueryable<Company> Companies { get; set; }
    }

    class Company
    {
    }

    class CompanyGraph:
        EfObjectGraphType<Company>
    {
        public CompanyGraph(IEfGraphQLService efGraphQlService) :
            base(efGraphQlService)
        {
        }
    }
}