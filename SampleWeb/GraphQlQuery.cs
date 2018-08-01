using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

public class GraphQlQuery
{
    public string OperationName { get; set; }
    [Required]
    public string Query { get; set; }
    public JObject Variables { get; set; }
}