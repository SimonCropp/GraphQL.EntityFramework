using System.ComponentModel.DataAnnotations;

public class GraphQlQuery
{
    public string OperationName { get; set; }
    [Required]
    public string Query { get; set; }
    public string Variables { get; set; }
}