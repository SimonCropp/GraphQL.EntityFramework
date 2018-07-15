class UintGraph : ScalarGraph<uint>
{
    protected override uint InnerParse(string value)
    {
        return uint.Parse(value);
    }
}