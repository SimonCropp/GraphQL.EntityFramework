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
            resolve: context =>
            {
                var data = context.RequestServices!.GetRequiredService<DbContext1>();
                return data.Entities;
            });
        efGraphQlService1.AddFirstField(
            graph: this,
            name: "entity1First",
            resolve: context =>
            {
                var data = context.RequestServices!.GetRequiredService<DbContext1>();
                return data.Entities;
            });
        efGraphQlService2.AddSingleField(
            graph: this,
            name: "entity2",
            resolve: context =>
            {
                var data = context.RequestServices!.GetRequiredService<DbContext2>();
                return data.Entities;
            });
        efGraphQlService2.AddFirstField(
            graph: this,
            name: "entity2First",
            resolve: context =>
            {
                var data = context.RequestServices!.GetRequiredService<DbContext2>();
                return data.Entities;
            });
    }
}