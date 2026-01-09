using GraphQL.Types;

class ResolveDbContextQuery
{
    #region QueryResolveDbContext

    public class Query :
        QueryGraphType<MyDbContext>
    {
        public Query(IEfGraphQLService<MyDbContext> graphQlService) :
            base(graphQlService) =>
            Field<ListGraphType<CompanyGraph>>("oldCompanies")
                .Resolve(context =>
                {
                    // uses the base QueryGraphType to resolve the db context
                    var data = ResolveDbContext(context);
                    return data.Companies.Where(_ => _.Age > 10);
                });
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

    class CompanyGraph(IEfGraphQLService<MyDbContext> efGraphQlService) :
        EfObjectGraphType<MyDbContext, Company>(efGraphQlService);
}
