namespace EfCoreGraphQL
{
    public class ShortGraph : ScalarGraph<short>
    {
        protected override short InnerParse(string value)
        {
            return short.Parse(value);
        }
    }
}