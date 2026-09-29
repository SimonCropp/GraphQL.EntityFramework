class ParameterReplacer(ParameterExpression original, ParameterExpression replacement) :
    ExpressionVisitor
{
    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (node == original)
        {
            return replacement;
        }

        return node;
    }
}
