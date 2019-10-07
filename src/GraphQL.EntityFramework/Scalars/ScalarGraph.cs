using GraphQL.Language.AST;
using GraphQL.Types;


namespace GraphQL.EntityFramework
{
    public abstract class ScalarGraph<T> :
        ScalarGraphType
    {
        public ScalarGraph()
        {
            Name = typeof(T).Name;
            Description = Name;
        }

        public override object Serialize(object value)
        {
            return value?.ToString()!;
        }

        public override object ParseValue(object value)
        {
            Guard.AgainstNull(nameof(value), value);
            var trim = value.ToString().Trim('"');
            Guard.AgainstNullWhiteSpace(nameof(value), trim);
            return InnerParse(trim)!;
        }

        protected abstract T InnerParse(string value);

        public override object? ParseLiteral(IValue value)
        {
            if (value is StringValue str)
            {
                return ParseValue(str.Value);
            }

            return null;
        }
    }
}