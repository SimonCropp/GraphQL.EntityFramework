namespace EfCoreGraphQL
{
    public class ShortGraph : NonNullableScalarGraph<short>
    {
        protected override short InnerParse(string value)
        {
            return short.Parse(value);
        }
    }
}