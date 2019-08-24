using GraphQL;

public class DependencySchema :
    GraphQL.Types.Schema
{
    public DependencySchema(IDependencyResolver resolver) :
        base(resolver)
    {
        Query = resolver.Resolve<DependencyQuery>();
    }
}