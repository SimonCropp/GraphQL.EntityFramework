using GraphQL.Types;

public class EmployeeGraph : ObjectGraphType<Employee>
{
    public EmployeeGraph()
    {
        Field(x => x.Id);
        Field(x => x.Content);

    }
}