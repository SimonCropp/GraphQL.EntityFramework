namespace GraphQL.EntityFramework;

public class Key(string name, Type type)
{
    public string Name => name;
    public Type Type => type;
}