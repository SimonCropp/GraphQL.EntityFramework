// OrderByEnumOptions.NestingDepth is not the default, and the collision it can cause has no
// entity in the integration model, so both are exercised against models built here
public class OrderByEnumGraphTests
{
    [Fact]
    public void Nesting_depth_default_flattens_one_navigation() =>
        Assert.Equal(
            [
                "id",
                "id_desc",
                "level2_id",
                "level2_id_desc"
            ],
            Values<Level1, NestingContext>(new()));

    [Fact]
    public void Nesting_depth_of_one_is_the_entity_only() =>
        Assert.Equal(
            [
                "id",
                "id_desc"
            ],
            Values<Level1, NestingContext>(
                new()
                {
                    NestingDepth = 1
                }));

    [Fact]
    public void Nesting_depth_of_three_flattens_two_navigations() =>
        Assert.Equal(
            [
                "id",
                "id_desc",
                "level2_id",
                "level2_id_desc",
                "level2_level3_id",
                "level2_level3_id_desc",
                "level2_level3_property",
                "level2_level3_property_desc"
            ],
            Values<Level1, NestingContext>(
                new()
                {
                    NestingDepth = 3
                }));

    [Fact]
    public void Collision_between_a_property_and_a_flattened_navigation_throws()
    {
        var exception = Assert.Throws<Exception>(() => Values<Collision, CollisionContext>(new()));
        Assert.Equal(
            "The orderBy enum CollisionOrderBy generated the value 'parent_id' for both 'Parent.Id' and 'Parent_id'. Flattening a navigation uses the same separator as a property name, so the two collide. Rename one of the properties, or drop the nesting depth.",
            exception.Message);
    }

    static string[] Values<TEntity, TContext>(OrderByEnumOptions options)
        where TContext : DbContext
    {
        var options1 = new DbContextOptionsBuilder<TContext>()
            .UseSqlServer("Server=notUsed")
            .Options;
        using var context = (TContext) Activator.CreateInstance(typeof(TContext), options1)!;
        var service = new EfGraphQLService<TContext>(context.Model, (_, _) => context);
        return new OrderByEnumGraph<TEntity>([service], options)
            .Values
            .Select(_ => _.Name)
            .ToArray();
    }

    public class NestingContext(DbContextOptions options) :
        DbContext(options)
    {
        public DbSet<Level1> Level1s { get; set; } = null!;
    }

    public class Level1
    {
        public Guid Id { get; set; }
        public Level2? Level2 { get; set; }
    }

    public class Level2
    {
        public Guid Id { get; set; }
        public Level3? Level3 { get; set; }
    }

    public class Level3
    {
        public Guid Id { get; set; }
        public string? Property { get; set; }
    }

    public class CollisionContext(DbContextOptions options) :
        DbContext(options)
    {
        public DbSet<Collision> Collisions { get; set; } = null!;
    }

    public class Collision
    {
        public Guid Id { get; set; }

        // Flattens to the same enum value as Parent.Id
        public Guid? Parent_id { get; set; }
        public CollisionParent? Parent { get; set; }
    }

    public class CollisionParent
    {
        public Guid Id { get; set; }
    }
}
