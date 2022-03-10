﻿using EfLocalDb;
using GraphQL;
using Microsoft.EntityFrameworkCore;

[UsesVerify]
public class ConnectionConverterTests
{
    static ConnectionConverterTests() =>
        sqlInstance = new(
            buildTemplate: async dbContext =>
            {
                await dbContext.Database.EnsureCreatedAsync();
                dbContext.AddRange(list.Select(x => new Entity {Property = x}));
                await dbContext.SaveChangesAsync();
            },
            constructInstance: builder => new(builder.Options));

    static List<string> list = new()
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
        await using var database = await sqlInstance.Build(databaseSuffix: $"{first.GetValueOrDefault(0)}{after.GetValueOrDefault(0)}{last.GetValueOrDefault(0)}{before.GetValueOrDefault(0)}");
        var entities = database.Context.Entities;
        var connection = await ConnectionConverter.ApplyConnectionContext<string, Entity>(entities.OrderBy(x=>x.Property), first, after, last, before, fieldContext, new());
        await Verify(connection.Items!.OrderBy(x => x!.Property))
            .UseParameters(first, after, last, before);
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

    public Task List(int? first, int? after, int? last, int? before)
    {

        var connection = ConnectionConverter.ApplyConnectionContext(list, first, after, last, before);
        return Verify(connection).UseParameters(first, after, last, before);
    }

    public class Entity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
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

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<Entity>();
    }
}