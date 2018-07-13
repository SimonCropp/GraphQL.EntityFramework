namespace EfCoreGraphQL
{
    public class WhereExpression
    {
        public string Path { get; set; }
        public Comparison Comparison { get; set; }
        public string Value { get; set; }
    }
}