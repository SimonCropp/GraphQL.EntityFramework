using System.Linq;
using GraphQL.EntityFramework;

class RootQuery
{
    #region rootQuery
    public class Query : EfObjectGraphType
    {
        public Query(IEfGraphQLService graphQlService) : base(graphQlService)
        {
            AddQueryField<CompanyGraph, Company>(
                name: "companies",
                resolve: context =>
                {
                    var dataContext = (DataContext) context.UserContext;
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

    class CompanyGraph:EfObjectGraphType<Company>
    {
        public CompanyGraph(IEfGraphQLService efGraphQlService) : base(efGraphQlService)
        {
        }
    }
}