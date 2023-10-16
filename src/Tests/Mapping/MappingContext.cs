using Newtonsoft.Json;

public class MappingContext(DbContextOptions options) :
    DbContext(options)
{
    public DbSet<MappingParent> Parents { get; set; } = null!;
    public DbSet<MappingChild> Children { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var parentBuilder = modelBuilder.Entity<MappingParent>();
        parentBuilder.Property(_ => _.JsonProperty)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<IList<string>>(v)!);

        modelBuilder.Entity<MappingChild>();
    }
}