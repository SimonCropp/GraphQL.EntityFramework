using System;
using System.Collections.Generic;
using System.Linq;
using EfCore.InMemoryHelpers;
using GraphQL.EntityFramework;
using GraphQL;
using GraphQL.Types;
using GraphQL.Types.Relay;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        //TODO:
        services.AddTransient(typeof(ConnectionType<>));
        services.AddTransient(typeof(EdgeType<>));
        services.AddTransient<PageInfoType>();



        var myDataContext = InMemoryContextBuilder.Build<MyDataContext>();
        var company1 = new Company
        {
            Content = "Company1"
        };
        var employee1 = new Employee
        {
            CompanyId = company1.Id,
            Content = "Employee1"
        };
        var employee2 = new Employee
        {
            CompanyId = company1.Id,
            Content = "Employee2"
        };
        var company2 = new Company
        {
            Content = "Company2"
        };
        var employee4 = new Employee
        {
            CompanyId = company2.Id,
            Content = "Employee4"
        };
        var company3 = new Company
        {
            Content = "Company3"
        };
        var company4 = new Company
        {
            Content = "Company4"
        };
        myDataContext.AddRange(company1, employee1, employee2, company2,company3,company4, employee4);
        myDataContext.SaveChanges();
        services.AddSingleton(myDataContext);

        EfCoreGraphQLConventions.RegisterInContainer(services);

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