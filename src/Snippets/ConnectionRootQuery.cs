class ConnectionRootQuery
{
    #region ConnectionRootQuery

    public class Query :
        QueryGraphType<MyDbContext>
    {
        public Query(IEfGraphQLService<MyDbContext> graphQlService)
            :
            base(graphQlService) =>
            AddQueryConnectionField<Company>(
                name: "companies",
                resolve: _ => _.DbContext.Companies.OrderBy(_ => _.Name));
    }

    #endregion

    public class Company
    {
        public string Name { get; set; } = null!;
    }

    class CompanyGraph(IEfGraphQLService<MyDbContext> efGraphQlService) :
        EfObjectGraphType<MyDbContext, Company>(efGraphQlService);

    public class MyDbContext :
        DbContext
    {
        public DbSet<Company> Companies { get; set; } = null!;
    }
}