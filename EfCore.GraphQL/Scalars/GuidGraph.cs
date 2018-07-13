using System;

namespace EfCoreGraphQL
{
    public class GuidGraph : NonNullableScalarGraph<Guid>
    {
        protected override Guid InnerParse(string value)
        {
            return Guid.Parse(value);
        }
    }
}