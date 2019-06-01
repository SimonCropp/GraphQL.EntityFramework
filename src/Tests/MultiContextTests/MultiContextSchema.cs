using GraphQL;

public class MultiContextSchema :
    GraphQL.Types.Schema
{
    public MultiContextSchema(IDependencyResolver resolver) :
        base(resolver)
    {
        Query = resolver.Resolve<MultiContextQuery>();
    }
}