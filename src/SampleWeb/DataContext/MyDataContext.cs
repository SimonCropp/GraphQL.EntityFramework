using Microsoft.EntityFrameworkCore;

public class MyDbContext :
    DbContext
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Company> Companies { get; set; }

    public MyDbContext()
    {
    }
    public MyDbContext(DbContextOptions options) :
        base(options)
    {
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