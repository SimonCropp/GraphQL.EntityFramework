using System;

namespace GraphQL.EntityFramework
{
    public class GuidGraph :
        ScalarGraph<Guid>
    {
        protected override Guid InnerParse(string value)
        {
            return Guid.Parse(value);
        }
    }
}