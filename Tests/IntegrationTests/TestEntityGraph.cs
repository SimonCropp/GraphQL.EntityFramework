using GraphQL.Types;

public class TestEntityGraph : ObjectGraphType<TestEntity>
{
    public TestEntityGraph()
    {
        Field(x => x.Id);
        Field(x => x.Property);
        Field(x => x.Nullable,true);
    }
}