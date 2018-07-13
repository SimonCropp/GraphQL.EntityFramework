namespace EfCoreGraphQL
{
    public class UintGraph : NonNullableScalarGraph<uint>
    {
        protected override uint InnerParse(string value)
        {
            return uint.Parse(value);
        }
    }
}