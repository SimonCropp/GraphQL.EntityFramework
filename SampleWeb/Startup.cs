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

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped(provider => DataContextBuilder.BuildDataContext());

        EfGraphQLConventions.RegisterConnectionTypesInContainer(services);
        using (var myDataContext = DataContextBuilder.BuildDataContext())
        {
            EfGraphQLConventions.RegisterInContainer(services, myDataContext);
        }

        foreach (var type in GetGraphQlTypes())
        {
            services.AddSingleton(type);
        }

        services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
        services.AddSingleton<IDependencyResolver>(
            provider => new FuncDependencyResolver(provider.GetRequiredService));
        services.AddSingleton<ISchema, Schema>();
        var mvc = services.AddMvc();
        mvc.SetCompatibilityVersion(CompatibilityVersion.Latest);
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
        builder.UseGraphiQl("graphiql");
        builder.UseMvc();
    }
}