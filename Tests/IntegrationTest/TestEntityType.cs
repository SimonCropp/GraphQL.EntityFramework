using GraphQL.Types;

public class TestEntityType : ObjectGraphType<TestEntity>
{
    public TestEntityType()
    {
        Field(x => x.Id);
        Field(x => x.Property);
    }
}