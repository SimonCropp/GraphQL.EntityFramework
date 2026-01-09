class ProjectionSnippets
{
    #region ProjectionEntity

    public class Order
    {
        public int Id { get; set; }                    // Primary key
        public int CustomerId { get; set; }            // Foreign key
        public Customer Customer { get; set; } = null!;         // Navigation property
        public string OrderNumber { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string InternalNotes { get; set; } = null!;
    }

    #endregion

    #region ProjectionExpression

    static void ProjectionExample(MyDbContext context)
    {
        _ = context.Orders.Select(o => new Order
        {
            Id = o.Id,                  // Requested (and primary key)
            CustomerId = o.CustomerId,  // Automatically included (foreign key)
            OrderNumber = o.OrderNumber // Requested
        });
    }

    #endregion

    #region ProjectionCustomResolver

    public class OrderGraph : EfObjectGraphType<MyDbContext, Order>
    {
        public OrderGraph(IEfGraphQLService<MyDbContext> graphQlService) : base(graphQlService)
        {
            // Custom field that uses the foreign key
            Field<StringGraphType>("customerName")
                .ResolveAsync(async context =>
                {
                    var data = context.DataContext();
                    // CustomerId is available even though it wasn't in the GraphQL query
                    var customer = await data.Customers
                        .Where(c => c.Id == context.Source.CustomerId)
                        .Select(c => c.Name)
                        .SingleAsync();
                    return customer;
                });
        }
    }

    #endregion

    public class MyDbContext :
        DbContext
    {
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
