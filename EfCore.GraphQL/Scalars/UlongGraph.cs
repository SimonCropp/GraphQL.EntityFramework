namespace EfCoreGraphQL
{
    public class UlongGraph : NonNullableScalarGraph<ulong>
    {
        protected override ulong InnerParse(string value)
        {
            return ulong.Parse(value);
        }
    }
}