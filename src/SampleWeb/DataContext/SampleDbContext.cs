using Microsoft.EntityFrameworkCore.Diagnostics;

public class SampleDbContext(DbContextOptions options) :
    DbContext(options)
{
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Device> Devices { get; set; } = null!;
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

    protected override void OnConfiguring(DbContextOptionsBuilder builder) =>
        builder.ConfigureWarnings(_ => _.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning));

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Company>()
            .HasMany(_ => _.Employees)
            .WithOne(_ => _.Company)
            .IsRequired();
        builder.Entity<Device>();
        builder.Entity<Employee>()
            .HasMany(x => x.Devices)
            .WithMany(x => x.Employees)
            .UsingEntity("EmployeeDevice");
        var order = builder.Entity<OrderDetail>();
        order.OwnsOne(_ => _.BillingAddress);
        order.OwnsOne(_ => _.ShippingAddress);
    }
}