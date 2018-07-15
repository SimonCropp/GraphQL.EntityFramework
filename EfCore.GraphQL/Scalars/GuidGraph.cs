using System;

class GuidGraph : ScalarGraph<Guid>
{
    protected override Guid InnerParse(string value)
    {
        return Guid.Parse(value);
    }
}