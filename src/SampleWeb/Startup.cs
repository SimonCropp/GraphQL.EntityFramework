using System;
using System.Collections.Generic;
using System.Linq;
using GraphiQl;
using GraphQL.EntityFramework;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        EfGraphQLConventions.RegisterInContainer<SampleDbContext>(
            services,
            model: SampleDbContext.StaticModel);
        EfGraphQLConventions.RegisterConnectionTypesInContainer(services);

        foreach (var type in GetGraphQlTypes())
        {
            services.AddSingleton(type);
        }
        //TODO: re add for subscriptions
        //var graphQl = services.AddGraphQL(
        //    options => options.ExposeExceptions = true);
        //graphQl.AddWebSockets();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            DbContextBuilder dbContextBuilder = new();
            services.AddSingleton<IHostedService>(dbContextBuilder);

            services.AddSingleton<Func<SampleDbContext>>(_ => dbContextBuilder.BuildDbContext);
            services.AddScoped(_ => dbContextBuilder.BuildDbContext());
        }
        else
        {
            services.AddScoped(_ =>
            {
                var _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                var _options = new DbContextOptionsBuilder<SampleDbContext>()
                    .UseSqlite(_connection)
                    .Options;
                var _context = new SampleDbContext(_options);

                DbContextBuilder.CreateDb(_context).Wait();

                return _context;
            });
        }

        services.AddSingleton<IDocumentExecuter, EfDocumentExecuter>();
        services.AddSingleton<ISchema, Schema>();
        var mvc = services.AddMvc(option => option.EnableEndpointRouting = false);
        mvc.SetCompatibilityVersion(CompatibilityVersion.Latest);
        mvc.AddNewtonsoftJson();
    }

    static IEnumerable<Type> GetGraphQlTypes()
    {
        return typeof(Startup).Assembly
            .GetTypes()
            .Where(x => !x.IsAbstract &&
                        (typeof(IObjectGraphType).IsAssignableFrom(x) ||
                         typeof(IInputObjectGraphType).IsAssignableFrom(x)));
    }

    public void Configure(IApplicationBuilder builder)
    {
        builder.UseWebSockets();
        //builder.UseGraphQLWebSockets<ISchema>();
        builder.UseGraphiQl("/graphiql", "/graphql");
        builder.UseMvc();
    }
}