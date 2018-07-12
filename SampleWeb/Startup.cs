using System;
using System.Collections.Generic;
using System.Linq;
using EfCore.InMemoryHelpers;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var myDataContext = InMemoryContextBuilder.Build<MyDataContext>();
        myDataContext.Add(new Parent());
        services.AddSingleton(myDataContext);
        foreach (var type in GetGraphQlTypes())
        {
            services.AddSingleton(type);
        }

        services.AddScoped<IDocumentExecuter, DocumentExecuter>();
        services.AddScoped<ISchema>(
            provider =>
            {
                var resolver = new FuncDependencyResolver(provider.GetService);
                return new Schema(resolver);
            });
    }

    public static IEnumerable<Type> GetGraphQlTypes()
    {
        return typeof(Startup).Assembly
            .GetTypes()
            .Where(x => !x.IsAbstract &&
                        (typeof(IObjectGraphType).IsAssignableFrom(x) ||
                         typeof(IInputObjectGraphType).IsAssignableFrom(x)));
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseGraphiQl();
    }
}