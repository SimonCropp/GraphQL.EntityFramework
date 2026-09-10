class ParameterReplacer(ParameterExpression original, ParameterExpression replacement) :
    ExpressionVisitor
{
    protected override Expression VisitParameter(ParameterExpression node) =>
        node == original ? replacement : node;
}
