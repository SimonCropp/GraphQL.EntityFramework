using System;
using System.Collections.Generic;
using System.Linq;
using EfCore.InMemoryHelpers;
using EfCoreGraphQL;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var myDataContext = InMemoryContextBuilder.Build<MyDataContext>();
        var company = new Company
        {
            Content = "Company1"
        };
        var employee1 = new Employee
        {
            CompanyId = company.Id,
            Content = "Employee1"
        };
        var employee2 = new Employee
        {
            CompanyId = company.Id,
            Content = "Employee2"
        };
        myDataContext.AddRange(company, employee1, employee2);
        myDataContext.SaveChanges();
        services.AddSingleton(myDataContext);
        Scalar.Inject((type, instance) => { services.AddSingleton(type, instance); });
        ArgumentGraphTypes.Inject((type, instance) => { services.AddSingleton(type, instance); });
        foreach (var type in GetGraphQlTypes())
        {
            services.AddSingleton(type);
        }

        services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
        services.AddSingleton<IDependencyResolver>(provider => new FuncDependencyResolver(provider.GetRequiredService));
        services.AddSingleton<ISchema,Schema>();
        services.AddMvc();
    }

    public static IEnumerable<Type> GetGraphQlTypes()
    {
        return typeof(Startup).Assembly
            .GetTypes()
            .Where(x => !x.IsAbstract &&
                        (typeof(IObjectGraphType).IsAssignableFrom(x) ||
                         typeof(IInputObjectGraphType).IsAssignableFrom(x)));
    }

    public void Configure(IApplicationBuilder builder)
    {
        builder.UseGraphiQl();
        builder.UseMvc();
    }
}