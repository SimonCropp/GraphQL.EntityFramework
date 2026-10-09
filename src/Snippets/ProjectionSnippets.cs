using GraphQL;
using GraphQL.Types;
using StringGraphType = GraphQL.Types.StringGraphType;

class ProjectionSnippets
{
    #region ProjectionEntity

    public class Order
    {
        // Primary key
        public int Id { get; set; }

        // Foreign key
        public int CustomerId { get; set; }

        // Navigation property
        public Customer Customer { get; set; } = null!;
        public string OrderNumber { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string InternalNotes { get; set; } = null!;
    }

    #endregion

    #region ProjectionExpression

    static void ProjectionExample(MyDbContext context) =>
        _ = context.Orders.Select(o => new Order
        {
            // Requested (and primary key)
            Id = o.Id,
            // Automatically included (foreign key)
            CustomerId = o.CustomerId,
            // Requested
            OrderNumber = o.OrderNumber
        });

    #endregion

    #region ProjectionCustomResolver

    public class OrderGraph :
        EfObjectGraphType<MyDbContext, Order>
    {
        public OrderGraph(IEfGraphQLService<MyDbContext> graphQlService) :
            base(graphQlService) =>
            // Custom field that uses the foreign key
            Field<StringGraphType>("customerName")
                .ResolveAsync(async context =>
                {
                    var data = ResolveDbContext(context);
                    // CustomerId is available even though it wasn't in the GraphQL query
                    return await data.Customers
                        .Where(c => c.Id == context.Source.CustomerId)
                        .Select(c => c.Name)
                        .SingleAsync();
                });
    }

    #endregion

    #region ProjectionReadOnlyProperty

    public class Invoice
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }

        // Mapped, with no setter: a computed column. Projected like any other column.
        public DateTimeOffset LastModified { get; }

        // Expression bodied: not a column. Selecting it loads the whole entity.
        public string Summary => $"Invoice {Id} for {Amount}";
    }

    #endregion

    #region ProjectionApplyToQuery

    public class Mutation :
        QueryGraphType<MyDbContext>
    {
        public Mutation(IEfGraphQLService<MyDbContext> graphQlService) :
            base(graphQlService) =>
            Field<OrderGraph>("cancelOrder")
                .Argument<NonNullGraphType<IntGraphType>>("id")
                .ResolveAsync(async context =>
                {
                    var id = context.GetArgument<int>("id");
                    var data = ResolveDbContext(context);

                    var order = await data.Orders.SingleAsync(_ => _.Id == id);
                    order.InternalNotes = "Cancelled";
                    await data.SaveChangesAsync();

                    // Read back only what the request selected under cancelOrder
                    var query = data.Orders
                        .AsSplitQuery()
                        .Where(_ => _.Id == id);
                    return await GraphQlService.ApplyProjection(context, query).SingleAsync();
                });
    }

    #endregion

    public class MyDbContext :
        DbContext
    {
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
