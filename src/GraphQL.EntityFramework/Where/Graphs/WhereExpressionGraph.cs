using GraphQL.Types;

namespace GraphQL.EntityFramework;

public class WhereExpressionGraph :
    InputObjectGraphType<WhereExpression>
{
    public WhereExpressionGraph()
    {
        Name = nameof(WhereExpression);
        Field(x => x.Path, true);
        Field<ComparisonGraph>("comparison", null, null, _ => _.Source.Comparison);
        Field(x => x.Negate, true);
        Field<EnumerationGraphType<StringComparison>>("case", null, null, _ => _.Source.Case);
        Field(x => x.Value, true);
        Field<ConnectorGraph>("connector", null, null, _ => _.Source.Connector);
        Field<ListGraphType<WhereExpressionGraph>>(
            name: "GroupedExpressions");
    }
}