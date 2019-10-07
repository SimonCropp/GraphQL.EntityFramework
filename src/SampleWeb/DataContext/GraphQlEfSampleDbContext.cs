using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

public class GraphQlEfSampleDbContext :
    DbContext
{
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Company> Companies { get; set; } = null!;

    public GraphQlEfSampleDbContext(DbContextOptions options) :
        base(options)
    {
    }

    public static IModel GetModel()
    {
        var builder = new DbContextOptionsBuilder();
        builder.UseSqlServer("Fake");
        using var dbContext = new GraphQlEfSampleDbContext(builder.Options);
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