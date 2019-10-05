using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfLocalDb;
using GraphQL.EntityFramework;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

public class ConnectionConverterTests :
    XunitApprovalBase
{
    static ConnectionConverterTests()
    {
        sqlInstance = new SqlInstance<MyContext>(
            buildTemplate: async dbContext =>
            {
                await dbContext.Database.EnsureCreatedAsync();
                dbContext.AddRange(list.Select(x => new Entity {Property = x}));
                await dbContext.SaveChangesAsync();
            },
            constructInstance: builder => new MyContext(builder.Options));
    }

    static List<string> list = new List<string>
    {
        "a", "b", "c", "d", "e", "f", "g", "h", "i", "j"
    };

    static SqlInstance<MyContext> sqlInstance;

    [Theory]
    //first after
    [InlineData(1, 0, null, null)]
    [InlineData(2, null, null, null)]
    [InlineData(2, 1, null, null)]
    [InlineData(3, null, null, null)]
    [InlineData(3, 1, null, null)]
    [InlineData(10, null, null, null)]
    [InlineData(10, 1, null, null)]
    [InlineData(11, null, null, null)]
    [InlineData(11, 1, null, null)]

    //first after
    [InlineData(2, null, null, 2)]
    [InlineData(3, null, null, 2)]
    [InlineData(2, null, null, 3)]

    //last before
    [InlineData(null, null, 2, null)]
    [InlineData(null, null, 2, 8)]

    //last after
    [InlineData(null, 7, 2, null)]
    public async Task Queryable(int? first, int? after, int? last, int? before)
    {
        var fieldContext = new ResolveFieldContext<string>();
        using var database = await sqlInstance.BuildWithRollback();
        var entities = database.Context.Entities;
        var connection = await ConnectionConverter.ApplyConnectionContext(entities, first, after, last, before, fieldContext, new Filters());
        ObjectApprover.Verify(connection);
    }

    [Theory]
    //first after
    [InlineData(1, 0, null, null)]
    [InlineData(2, null, null, null)]
    [InlineData(2, 1, null, null)]
    [InlineData(3, null, null, null)]
    [InlineData(3, 1, null, null)]
    [InlineData(10, null, null, null)]
    [InlineData(10, 1, null, null)]
    [InlineData(11, null, null, null)]
    [InlineData(11, 1, null, null)]

    //first after
    [InlineData(2, null, null, 2)]
    [InlineData(3, null, null, 2)]
    [InlineData(2, null, null, 3)]

    //last before
    [InlineData(null, null, 2, null)]
    [InlineData(null, null, 2, 8)]

    //last after
    [InlineData(null, 7, 2, null)]

    public void List(int? first, int? after, int? last, int? before)
    {
        var connection = ConnectionConverter.ApplyConnectionContext(list, first, after, last, before);
        ObjectApprover.Verify(connection);
    }

    public ConnectionConverterTests(ITestOutputHelper output) :
        base(output)
    {
    }

    public class Entity
    {
        public Guid Id { get; set; } = XunitLogging.Context.NextGuid();
        public string? Property { get; set; }
    }

    public class MyContext :
        DbContext
    {
        public DbSet<Entity> Entities { get; set; } = null!;

        public MyContext(DbContextOptions options) :
            base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity>();
        }
    }
}