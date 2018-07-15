using System;
using System.Linq;
using EfCoreGraphQL;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;

public class CompanyGraph : ObjectGraphType<Company>
{
    public CompanyGraph()
    {
        Field(x => x.Id);
        Field(x => x.Content);
        this.AddEnumerableField<Company, EmployeeGraph, Employee>(
            "employees",
            resolve: context =>
            {
                return context
                    .Source
                    .Employees;
            });
        var employeeConnection = Connection<EmployeeGraph>();
        employeeConnection.FieldType.Metadata["IncludeName"] = "Employees";
        employeeConnection.Name("employeesConnection");
        employeeConnection.Resolve(context =>
        {
            var skip = context.First.GetValueOrDefault(0);
            var take = context.PageSize.GetValueOrDefault(10);
            var list = context.Source.Employees;
            var page = list.Skip(skip).Take(take);
            return new Connection<Employee>
            {
                TotalCount = list.Count,
                PageInfo = new PageInfo
                {
                    HasNextPage = true,
                    HasPreviousPage = false,
                    StartCursor = skip.ToString(),
                    EndCursor = Math.Min(list.Count, skip+ take).ToString(),
                },
                Edges = page
                    .Select((item, index) => new Edge<Employee>
                    {
                        Cursor = (index + skip).ToString(),
                        Node = item
                    })
                    .ToList()
            };
        });
    }
}