using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;
using Microsoft.EntityFrameworkCore;

public class QueryableConnection
{
    public class Query : ObjectGraphType
    {
        public Query()
        {
            var connectionBuilder = Connection<EmployeeGraph>();
            connectionBuilder.Name("employees");
            connectionBuilder.ResolveAsync(async context =>
            {
                var dataContext = (DataContext) context.UserContext;
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
                        EndCursor = Math.Min(totalCount, skip + take).ToString()
                    },
                    Edges = await page
                        .Select((item, index) => new Edge<Employee>
                        {
                            Cursor = (index + skip).ToString(),
                            Node = item
                        }).ToListAsync().ConfigureAwait(false)
                };
            });
        }
    }

    public class Company
    {
        public List<Employee> Employees { get; set; }
    }

    public class Employee
    {
    }

    public class EmployeeGraph : ObjectGraphType<Employee>
    {
    }

    public class DataContext : DbContext
    {
        public DbSet<Company> Companies { get; set; }
        public DbSet<Employee> Employees { get; set; }

        public DataContext(DbContextOptions options) :
            base(options)
        {
        }
    }

}