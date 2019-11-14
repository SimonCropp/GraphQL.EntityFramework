using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

public class SampleDbContext :
    DbContext
{
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Company> Companies { get; set; } = null!;
    public static IModel StaticModel { get; } = BuildStaticModel();

    public SampleDbContext(DbContextOptions options) :
        base(options)
    {
    }

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
            .HasMany(c => c.Employees)
            .WithOne(e => e.Company)
            .IsRequired();
        modelBuilder.Entity<Employee>();
    }
}