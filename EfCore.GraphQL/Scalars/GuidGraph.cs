using System;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace EfCoreGraphQL
{
    public class GuidGraph : ScalarGraphType
    {
        public GuidGraph()
        {
            Name = "Guid";
            Description = "Globally Unique Identifier.";
        }

        public override object Serialize(object value)
        {
            return value?.ToString();
        }

        public override object ParseValue(object value)
        {
            var guid = value?.ToString().Trim('"');
            if (string.IsNullOrWhiteSpace(guid))
            {
                return null;
            }

            return Guid.Parse(guid);
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