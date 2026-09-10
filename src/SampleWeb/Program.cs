using GraphQL.Types;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

EfGraphQLConventions.RegisterInContainer<SampleDbContext>(
    services,
    model: SampleDbContext.StaticModel);

foreach (var type in typeof(Program).Assembly
             .GetTypes()
             .Where(_ => !_.IsAbstract &&
                         (_.IsAssignableTo(typeof(IObjectGraphType)) ||
                          _.IsAssignableTo(typeof(IInputObjectGraphType)))))
{
    services.AddSingleton(type);
}

var dbContextBuilder = new DbContextBuilder();
services.AddSingleton<IHostedService>(dbContextBuilder);
services.AddSingleton<Func<SampleDbContext>>(_ => dbContextBuilder.BuildDbContext);
services.AddScoped(_ => dbContextBuilder.BuildDbContext());
services.AddSingleton<IDocumentExecuter, EfDocumentExecuter>();
services.AddSingleton<ISchema, Schema>();
services.AddControllers();
services.AddGraphQL(null);

var app = builder.Build();

app.UseWebSockets();
// Serves the Blazor client: its framework files and wwwroot, with every unmatched path falling
// back to index.html so the router owns "/" and "/explorer". Mapped after the controller, so
// /graphql stays the api rather than becoming another route into the client.
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapFallbackToFile("index.html");

await app.RunAsync();

public partial class Program;
