public class MultiContextQuery :
    ObjectGraphType
{
    public MultiContextQuery(
        IEfGraphQLService<DbContext1> efGraphQlService1,
        IEfGraphQLService<DbContext2> efGraphQlService2)
    {
        efGraphQlService1.AddSingleField(
            graph: this,
            name: "entity1",
            resolve: _ => _.DbContext.Entities);
        efGraphQlService1.AddFirstField(
            graph: this,
            name: "entity1First",
            resolve: _ => _.DbContext.Entities);
        efGraphQlService2.AddSingleField(
            graph: this,
            name: "entity2",
            resolve: _ => _.DbContext.Entities);
        efGraphQlService2.AddFirstField(
            graph: this,
            name: "entity2First",
            resolve: _ => _.DbContext.Entities);
    }
}