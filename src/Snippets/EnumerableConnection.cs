using GraphQL.Types;

public class EnumerableConnection
{
    public class CompanyGraph :
        ObjectGraphType<Company>
    {
        public CompanyGraph()
        {
            var builder = Connection<EmployeeGraph>("employees");
            builder.Resolve(context =>
            {
                var skip = context.First.GetValueOrDefault(0);
                var take = context.PageSize.GetValueOrDefault(10);
                var list = context.Source.Employees;
                var page = list
                    .Skip(skip)
                    .Take(take);
                return new Connection<Employee>
                {
                    TotalCount = list.Count,
                    PageInfo = new()
                    {
                        HasNextPage = true,
                        HasPreviousPage = false,
                        StartCursor = skip.ToString(),
                        EndCursor = Math
                            .Min(list.Count, skip + take)
                            .ToString()
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

    public class Company
    {
        public List<Employee> Employees { get; set; } = null!;
    }

    public class Employee;

    public class EmployeeGraph :
        ObjectGraphType<Employee>;
}