using GraphiQl;
using GraphQL.Types;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        EfGraphQLConventions.RegisterInContainer<SampleDbContext>(
            services,
            model: SampleDbContext.StaticModel);

        foreach (var type in GetGraphQlTypes())
        {
            services.AddSingleton(type);
        }

        var dbContextBuilder = new DbContextBuilder();
        services.AddSingleton<IHostedService>(dbContextBuilder);
        services.AddSingleton<Func<SampleDbContext>>(_ => dbContextBuilder.BuildDbContext);
        services.AddScoped(_ => dbContextBuilder.BuildDbContext());
        services.AddSingleton<IDocumentExecuter, EfDocumentExecuter>();
        services.AddSingleton<ISchema, Schema>();
        services.AddMvc(option => option.EnableEndpointRouting = false);
        services.AddGraphQL(null);
    }

    static IEnumerable<Type> GetGraphQlTypes() =>
        typeof(Startup).Assembly
            .GetTypes()
            .Where(_ => !_.IsAbstract &&
                        (_.IsAssignableTo(typeof(IObjectGraphType)) ||
                         _.IsAssignableTo(typeof(IInputObjectGraphType))));

    public void Configure(IApplicationBuilder builder)
    {
        builder.UseWebSockets();
        //builder.UseGraphQLWebSockets<ISchema>();
        builder.UseGraphiQl("/graphiql", "/graphql");
        builder.UseMvc();
    }
}