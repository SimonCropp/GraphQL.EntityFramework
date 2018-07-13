namespace EfCoreGraphQL
{
    public class UshortGraph : NonNullableScalarGraph<ushort>
    {
        protected override ushort InnerParse(string value)
        {
            return ushort.Parse(value);
        }
    }
}