using System.Linq;
using GraphQL.EntityFramework;

class RootQuery
{
    #region rootQuery

    public class Query :
        QueryGraphType
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

    public class DbContext
    {
        public IQueryable<Company> Companies { get; set; }
    }

    public class Company
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