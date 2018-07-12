using GraphQL.Language.AST;
using GraphQL.Types;

namespace EfCoreGraphQL
{
    public class UlongGraphType : ScalarGraphType
    {
        public UlongGraphType()
        {
            Name = "ulong";
            Description = "Unsigned Long.";
        }

        public override object Serialize(object value)
        {
            return value?.ToString();
        }

        public override object ParseValue(object value)
        {
            var trim = value?.ToString().Trim('"');
            if (string.IsNullOrWhiteSpace(trim))
            {
                return null;
            }

            return ulong.Parse(trim);
        }

        public override object ParseLiteral(IValue value)
        {
            if (value is StringValue str)
            {
                return ParseValue(str.Value);
            }

            return null;
        }
    }
}