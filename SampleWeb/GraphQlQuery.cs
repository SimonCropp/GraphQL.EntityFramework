using Newtonsoft.Json.Linq;

public class GraphQlQuery
{
    public string Query { get; set; }
    public JObject Variables { get; set; }
}