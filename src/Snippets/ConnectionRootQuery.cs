using System.Linq;
using GraphQL.EntityFramework;

class ConnectionRootQuery
{
    #region ConnectionRootQuery

    public class Query :
        EfObjectGraphType
    {
        public Query(IEfGraphQLService graphQlService) :
            base(graphQlService)
        {
            AddQueryConnectionField(
                name: "companies",
                resolve: context =>
                {
                    var dataContext = (MyDataContext) context.UserContext;
                    return dataContext.Companies;
                },
                graphType: typeof(CompanyGraph));
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

    class MyDataContext
    {
        public IQueryable<Company> Companies { get; set; }
    }
}