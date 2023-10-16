public class SampleDbContext(DbContextOptions options) :
    DbContext(options)
{
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<OrderDetail> OrderDetails { get; set; } = null!;

    public static IModel StaticModel { get; } = BuildStaticModel();

    static IModel BuildStaticModel()
    {
        var builder = new DbContextOptionsBuilder();
        builder.UseSqlServer("Fake");
        using var dbContext = new SampleDbContext(builder.Options);
        return dbContext.Model;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>()
            .HasMany(_ => _.Employees)
            .WithOne(_ => _.Company)
            .IsRequired();
        modelBuilder.Entity<Employee>();

        var order = modelBuilder.Entity<OrderDetail>();
        order.OwnsOne(_ => _.BillingAddress);
        order.OwnsOne(_ => _.ShippingAddress);
    }
}