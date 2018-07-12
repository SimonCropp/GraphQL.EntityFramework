using Newtonsoft.Json.Linq;

public class GraphQlQuery
{
    public string OperationName { get; set; }
    public string NamedQuery { get; set; }
    public string Query { get; set; }
    //https://github.com/graphql-dotnet/graphql-dotnet/issues/389
    public JObject Variables { get; set; }
}