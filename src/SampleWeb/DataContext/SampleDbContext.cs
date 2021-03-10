using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

public class SampleDbContext :
    DbContext
{
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<OrderDetail> OrderDetails { get; set; } = null!;

    public static IModel StaticModel { get; } = BuildStaticModel();

    public SampleDbContext(DbContextOptions options) :
        base(options)
    {
    }

    static IModel BuildStaticModel()
    {
        DbContextOptionsBuilder builder = new();
        builder.UseSqlServer("Fake");
        using SampleDbContext dbContext = new(builder.Options);
        return dbContext.Model;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>()
            .HasMany(c => c.Employees)
            .WithOne(e => e.Company)
            .IsRequired();
        modelBuilder.Entity<Employee>();

        modelBuilder.Entity<OrderDetail>().OwnsOne(p => p.BillingAddress);
        modelBuilder.Entity<OrderDetail>().OwnsOne(p => p.ShippingAddress);

    }
}