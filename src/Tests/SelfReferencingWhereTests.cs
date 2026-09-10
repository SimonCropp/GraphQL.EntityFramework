// A where path into a collection of the enclosing type built the inner lambda on the same
// parameter instance as the outer one, and EF's parameter replacement then rewrote both.
public class SelfReferencingWhereTests
{
    static SqlInstance<SelfReferencingContext> sqlInstance = new(
        buildTemplate: async data =>
        {
            await data.Database.EnsureCreatedAsync();
            var root = new Node
            {
                Name = "root"
            };
            root.Children.Add(new()
            {
                Name = "a"
            });
            root.Children.Add(new()
            {
                Name = "b"
            });
            var other = new Node
            {
                Name = "other"
            };
            other.Children.Add(new()
            {
                Name = "c"
            });
            data.AddRange(root, other);
            await data.SaveChangesAsync();
        },
        constructInstance: builder => new(builder.Options));

    [Fact]
    public async Task List_path_into_same_type()
    {
        await using var database = await sqlInstance.Build();
        var predicate = ExpressionBuilder<Node>.BuildPredicate("children[name]", Comparison.Equal, ["a"]);
        var names = await database.Context.Nodes
            .Where(predicate)
            .Select(_ => _.Name)
            .ToListAsync();
        Assert.Equal(["root"], names);
    }

    [Fact]
    public async Task List_path_into_same_type_negated()
    {
        await using var database = await sqlInstance.Build();
        var predicate = ExpressionBuilder<Node>.BuildPredicate("children[name]", Comparison.Equal, ["a"], negate: true);
        var names = await database.Context.Nodes
            .Where(predicate)
            .OrderBy(_ => _.Name)
            .Select(_ => _.Name)
            .ToListAsync();
        Assert.Equal(["a", "b", "c", "other"], names);
    }

    public class Node
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public Guid? ParentId { get; set; }
        public Node? Parent { get; set; }
        public List<Node> Children { get; set; } = [];
    }

    public class SelfReferencingContext(DbContextOptions options) :
        DbContext(options)
    {
        public DbSet<Node> Nodes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<Node>()
                .HasMany(_ => _.Children)
                .WithOne(_ => _.Parent)
                .HasForeignKey(_ => _.ParentId);
    }
}
