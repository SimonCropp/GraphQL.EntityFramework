namespace GraphQL.EntityFramework
{
    public class UshortGraph :
        ScalarGraph<ushort>
    {
        protected override ushort InnerParse(string value)
        {
            return ushort.Parse(value);
        }
    }
}