namespace GraphQL.EntityFramework;

public class SingleEntityNotFoundException :
    Exception
{
    public override string Message => "Not found";
}
public class FirstEntityNotFoundException :
    Exception
{
    public override string Message => "Not found";
}