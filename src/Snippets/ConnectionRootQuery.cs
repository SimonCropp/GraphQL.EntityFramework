class ConnectionRootQuery
{
    #region ConnectionRootQuery

    public class Query :
        QueryGraphType<MyDbContext>
    {
        public Query(IEfGraphQLService<MyDbContext> graphQlService) :
            base(graphQlService) =>
            AddQueryConnectionField(
                name: "companies",
                resolve: _ => _.DbContext.Companies);
    }

    #endregion

    public class Company;

    class CompanyGraph(IEfGraphQLService<MyDbContext> efGraphQlService) :
        EfObjectGraphType<MyDbContext, Company>(efGraphQlService);

    public class MyDbContext :
        DbContext
    {
        public IQueryable<Company> Companies { get; set; } = null!;
    }
}