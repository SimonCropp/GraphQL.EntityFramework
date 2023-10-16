class RootQuery
{
    #region rootQuery

    public class Query :
        QueryGraphType<MyDbContext>
    {
        public Query(IEfGraphQLService<MyDbContext> graphQlService) :
            base(graphQlService)
        {
            AddSingleField(
                resolve: _ => _.DbContext.Companies,
                name: "company");
            AddQueryField(
                name: "companies",
                resolve: _ => _.DbContext.Companies);
        }
    }

    #endregion

    public class MyDbContext :
        DbContext
    {
        public IQueryable<Company> Companies { get; set; } = null!;
    }

    public class Company;

    class CompanyGraph(IEfGraphQLService<MyDbContext> efGraphQlService) :
        EfObjectGraphType<MyDbContext, Company>(efGraphQlService);
}