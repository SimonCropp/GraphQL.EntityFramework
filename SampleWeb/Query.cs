using System;
using System.Linq;
using GraphQL.Types;
using EfCoreGraphQL;
using GraphQL.Types.Relay.DataObjects;
using Microsoft.EntityFrameworkCore;

public class Query : ObjectGraphType
{
    public Query()
    {
        this.AddQueryField<CompanyGraph, Company>("companies",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Companies;
            });
        var companyConnection = Connection<CompanyGraph>();
        companyConnection.Name("companiesConnection");
        companyConnection.ResolveAsync(async context =>
        {
            var dataContext = (MyDataContext) context.UserContext;
            var skip = context.First.GetValueOrDefault(0);
            var take = context.PageSize.GetValueOrDefault(10);
            var list = dataContext.Companies;
            var page = list.Skip(skip).Take(take);
            var totalCount = await list.CountAsync().ConfigureAwait(false);
            return new Connection<Company>
            {
                TotalCount = totalCount,
                PageInfo = new PageInfo
                {
                    HasNextPage = true,
                    HasPreviousPage = false,
                    StartCursor = skip.ToString(),
                    EndCursor = Math.Min(totalCount, skip + take).ToString(),
                },
                Edges = await page
                    .Select((item, index) => new Edge<Company>
                    {
                        Cursor = (index + skip).ToString(),
                        Node = item
                    })
                    .ToListAsync()
                    .ConfigureAwait(false)
            };
        });

        this.AddQueryField<EmployeeGraph, Employee>("employees",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Employees;
            });
        var employeeConnection = Connection<EmployeeGraph>();
        employeeConnection.Name("employeesConnection");
        employeeConnection.ResolveAsync(async context =>
        {
            var dataContext = (MyDataContext) context.UserContext;
            var skip = context.First.GetValueOrDefault(0);
            var take = context.PageSize.GetValueOrDefault(10);
            var list = dataContext.Employees;
            var page = list.Skip(skip).Take(take);
            var totalCount = await list.CountAsync().ConfigureAwait(false);
            return new Connection<Employee>
            {
                TotalCount = totalCount,
                PageInfo = new PageInfo
                {
                    HasNextPage = true,
                    HasPreviousPage = false,
                    StartCursor = skip.ToString(),
                    EndCursor = Math.Min(totalCount, skip + take).ToString(),
                },
                Edges = await page
                    .Select((item, index) => new Edge<Employee>
                    {
                        Cursor = (index + skip).ToString(),
                        Node = item
                    })
                    .ToListAsync()
                    .ConfigureAwait(false)
            };
        });
    }
}