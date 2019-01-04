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
            AddQueryConnectionField<CompanyGraph, Company>(
                name: "companies",
                resolve: context =>
                {
                    var dataContext = (MyDataContext) context.UserContext;
                    return dataContext.Companies;
                });
        }
    }

    #endregion

    class DataContext
    {
        public IQueryable<Company> Companies { get; set; }
    }

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