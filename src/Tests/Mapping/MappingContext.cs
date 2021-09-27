using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

public class MappingContext :
    DbContext
{
    public DbSet<MappingParent> Parents { get; set; } = null!;
    public DbSet<MappingChild> Children { get; set; } = null!;

    public MappingContext(DbContextOptions options) :
        base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var parentBuilder = modelBuilder.Entity<MappingParent>();
        parentBuilder.Property(e => e.JsonProperty)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<IList<string>>(v)!);

        modelBuilder.Entity<MappingChild>();
    }
}