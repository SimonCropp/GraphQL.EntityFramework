namespace GraphQL.EntityFramework;

public class FirstEntityNotFoundException(string? query = null) :
    Exception
{
    public override string Message
    {
        get
        {
            if (Query == null)
            {
                return "Not found";
            }

            return $"""
                    Not found.
                    Query:
                    {Query}
                    """;
        }
    }

    public string? Query { get; } = query;
}