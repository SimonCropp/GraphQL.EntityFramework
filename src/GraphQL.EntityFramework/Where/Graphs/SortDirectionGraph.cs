namespace GraphQL.EntityFramework;

public class SortDirectionGraph :
    EnumerationGraphType
{
    public SortDirectionGraph()
    {
        Name = "SortDirection";
        Add("ascending", SortDirection.Ascending);
        Add("descending", SortDirection.Descending);
    }
}
