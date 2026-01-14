using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace GraphQL.EntityFramework.Analyzers.Tests;

public class ContextSourceAccessAnalyzerTests
{
    [Fact]
    public async Task DetectsNavigationPropertyAccess()
    {
        var source = """
            #nullable enable
            using GraphQL.EntityFramework;

            namespace Test
            {
                public class TestEntity
                {
                    public int Id { get; set; }
                    public ParentEntity? Parent { get; set; }
                    public int ParentId { get; set; }
                }

                public class ParentEntity
                {
                    public int Id { get; set; }
                    public string Name { get; set; } = "";
                }

                public class TestDbContext : Microsoft.EntityFrameworkCore.DbContext { }

                public class TestGraph : EfObjectGraphType<TestDbContext, TestEntity>
                {
                    public TestGraph(IEfGraphQLService<TestDbContext> service) : base(service)
                    {
                        AddNavigationField(
                            name: "parent",
                            resolve: context => context.Source.Parent);
                    }
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        var warning = Assert.Single(diagnostics, d => d.Id == "GQLEF001");
        Assert.Contains("Parent", warning.GetMessage());
        Assert.Contains("ParentId", warning.GetMessage());
    }

    [Fact]
    public async Task DetectsScalarPropertyAccess()
    {
        var source = """
            using GraphQL.EntityFramework;

            namespace Test
            {
                public class TestEntity
                {
                    public int Id { get; set; }
                    public string? Status { get; set; }
                }

                public class TestDbContext : Microsoft.EntityFrameworkCore.DbContext { }

                public class TestGraph : EfObjectGraphType<TestDbContext, TestEntity>
                {
                    public TestGraph(IEfGraphQLService<TestDbContext> service) : base(service)
                    {
                        AddNavigationField(
                            name: "customField",
                            resolve: context => {
                                var status = context.Source.Status;
                                return status ?? "none";
                            });
                    }
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        var warning = Assert.Single(diagnostics, d => d.Id == "GQLEF001");
        Assert.Contains("Status", warning.GetMessage());
    }

    [Fact]
    public async Task AllowsIdPropertyAccess()
    {
        var source = """
            using GraphQL.EntityFramework;

            namespace Test
            {
                public class TestEntity
                {
                    public int Id { get; set; }
                    public string Name { get; set; } = "";
                }

                public class TestDbContext : Microsoft.EntityFrameworkCore.DbContext { }

                public class TestGraph : EfObjectGraphType<TestDbContext, TestEntity>
                {
                    public TestGraph(IEfGraphQLService<TestDbContext> service) : base(service)
                    {
                        AddNavigationField(
                            name: "customField",
                            resolve: context => {
                                var id = context.Source.Id;
                                return id.ToString();
                            });
                    }
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        Assert.DoesNotContain(diagnostics, d => d.Id == "GQLEF001");
    }

    [Fact]
    public async Task AllowsForeignKeyPropertyAccess()
    {
        var source = """
            using GraphQL.EntityFramework;

            namespace Test
            {
                public class TestEntity
                {
                    public int Id { get; set; }
                    public int ParentId { get; set; }
                    public string Name { get; set; } = "";
                }

                public class TestDbContext : Microsoft.EntityFrameworkCore.DbContext { }

                public class TestGraph : EfObjectGraphType<TestDbContext, TestEntity>
                {
                    public TestGraph(IEfGraphQLService<TestDbContext> service) : base(service)
                    {
                        AddNavigationField(
                            name: "customField",
                            resolve: context => {
                                var parentId = context.Source.ParentId;
                                return parentId.ToString();
                            });
                    }
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        Assert.DoesNotContain(diagnostics, d => d.Id == "GQLEF001");
    }

    [Fact]
    public async Task IgnoresNonEfGraphTypeClasses()
    {
        var source = """
            namespace Test
            {
                public class TestEntity
                {
                    public int Id { get; set; }
                    public string Name { get; set; } = "";
                }

                public class RegularClass
                {
                    public void Method()
                    {
                        var entity = new TestEntity();
                        var name = entity.Name;
                    }
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        Assert.DoesNotContain(diagnostics, d => d.Id == "GQLEF001");
    }

    [Fact]
    public async Task DetectsMultiplePropertyAccesses()
    {
        var source = """
            using GraphQL.EntityFramework;

            namespace Test
            {
                public class TestEntity
                {
                    public int Id { get; set; }
                    public string? Name { get; set; }
                    public string? Status { get; set; }
                }

                public class TestDbContext : Microsoft.EntityFrameworkCore.DbContext { }

                public class TestGraph : EfObjectGraphType<TestDbContext, TestEntity>
                {
                    public TestGraph(IEfGraphQLService<TestDbContext> service) : base(service)
                    {
                        AddNavigationField(
                            name: "customField",
                            resolve: context => {
                                var name = context.Source.Name;
                                var status = context.Source.Status;
                                return (name ?? "") + (status ?? "");
                            });
                    }
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        var warnings = diagnostics.Where(d => d.Id == "GQLEF001").ToArray();
        Assert.Equal(2, warnings.Length);
        Assert.Contains(warnings, w => w.GetMessage().Contains("Name"));
        Assert.Contains(warnings, w => w.GetMessage().Contains("Status"));
    }

    [Fact(Skip = "Fluent API pattern (Field().Resolve()) not yet supported by analyzer")]
    public async Task WorksWithEfInterfaceGraphType()
    {
        var source = """
            using GraphQL.EntityFramework;
            using GraphQL.Types;

            namespace Test
            {
                public interface ITestEntity
                {
                    int Id { get; set; }
                    string Name { get; set; }
                }

                public class TestDbContext : Microsoft.EntityFrameworkCore.DbContext { }

                public class TestGraph : EfInterfaceGraphType<TestDbContext, ITestEntity>
                {
                    public TestGraph(IEfGraphQLService<TestDbContext> service) : base(service)
                    {
                        Field<StringGraphType>("customField")
                            .Resolve(context => {
                                var name = context.Source.Name;
                                return name;
                            });
                    }
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        var warning = Assert.Single(diagnostics, d => d.Id == "GQLEF001");
        Assert.Contains("Name", warning.GetMessage());
    }

    [Fact(Skip = "Fluent API pattern (Field().Resolve()) not yet supported by analyzer")]
    public async Task WorksWithQueryGraphType()
    {
        var source = """
            using GraphQL.EntityFramework;
            using GraphQL.Types;

            namespace Test
            {
                public class TestEntity
                {
                    public int Id { get; set; }
                    public string Name { get; set; } = "";
                }

                public class TestDbContext : Microsoft.EntityFrameworkCore.DbContext { }

                public class TestQuery : QueryGraphType<TestDbContext>
                {
                    public TestQuery(IEfGraphQLService<TestDbContext> service) : base(service)
                    {
                        Field<StringGraphType>("customField")
                            .Resolve(context => {
                                var entity = new TestEntity { Name = "test" };
                                var name = entity.Name;
                                return name;
                            });
                    }
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        var warning = Assert.Single(diagnostics, d => d.Id == "GQLEF001");
        Assert.Contains("Name", warning.GetMessage());
    }

    [Fact]
    public async Task AllowsNestedIdAccess()
    {
        var source = """
            using GraphQL.EntityFramework;

            namespace Test
            {
                public class TestEntity
                {
                    public int Id { get; set; }
                    public ParentEntity? Parent { get; set; }
                }

                public class ParentEntity
                {
                    public int Id { get; set; }
                }

                public class TestDbContext : Microsoft.EntityFrameworkCore.DbContext { }

                public class TestGraph : EfObjectGraphType<TestDbContext, TestEntity>
                {
                    public TestGraph(IEfGraphQLService<TestDbContext> service) : base(service)
                    {
                        AddNavigationField(
                            name: "customField",
                            resolve: context => {
                                var parentId = context.Source.Parent?.Id;
                                return parentId?.ToString() ?? "none";
                            });
                    }
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        // Should warn on Parent access, but not on Id
        var warning = Assert.Single(diagnostics, d => d.Id == "GQLEF001");
        Assert.Contains("Parent", warning.GetMessage());
    }

    [Fact]
    public async Task AllowsCaseSensitiveId()
    {
        var source = """
            using GraphQL.EntityFramework;

            namespace Test
            {
                public class TestEntity
                {
                    public int ID { get; set; }
                    public string Name { get; set; } = "";
                }

                public class TestDbContext : Microsoft.EntityFrameworkCore.DbContext { }

                public class TestGraph : EfObjectGraphType<TestDbContext, TestEntity>
                {
                    public TestGraph(IEfGraphQLService<TestDbContext> service) : base(service)
                    {
                        AddNavigationField(
                            name: "customField",
                            resolve: context => {
                                var id = context.Source.ID;
                                return id.ToString();
                            });
                    }
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        Assert.DoesNotContain(diagnostics, d => d.Id == "GQLEF001");
    }

    private static async Task<IEnumerable<Diagnostic>> GetDiagnosticsAsync(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = new List<MetadataReference>();

        // Add specific assemblies we need, avoiding conflicts
        var requiredAssemblies = new[]
        {
            typeof(object).Assembly, // System.Private.CoreLib
            typeof(Console).Assembly, // System.Console
            typeof(IEfGraphQLService<>).Assembly, // GraphQL.EntityFramework
            typeof(Microsoft.EntityFrameworkCore.DbContext).Assembly, // EF Core
            typeof(Types.ObjectGraphType).Assembly, // GraphQL
            typeof(IQueryable<>).Assembly, // System.Linq.Expressions
        };

        foreach (var assembly in requiredAssemblies)
        {
            if (!string.IsNullOrEmpty(assembly.Location))
            {
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

        // Add all System.* and Microsoft.* assemblies except those that conflict
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!assembly.IsDynamic &&
                !string.IsNullOrEmpty(assembly.Location) &&
                !references.Any(r => r.Display == assembly.Location))
            {
                var name = assembly.GetName().Name ?? "";
                if ((name.StartsWith("System.") || name.StartsWith("Microsoft.")) &&
                    !name.Contains("xunit", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        references.Add(MetadataReference.CreateFromFile(assembly.Location));
                    }
                    catch
                    {
                        // Ignore assemblies that can't be referenced
                    }
                }
            }
        }

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        var analyzer = new ContextSourceAccessAnalyzer();
        var compilationWithAnalyzers = compilation.WithAnalyzers(
            [analyzer]);

        var allDiagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync();

        // Debug: Check for compilation errors
        var compilationErrors = allDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (compilationErrors.Length > 0)
        {
            var errorMessages = string.Join("\n", compilationErrors.Select(e => $"{e.Id}: {e.GetMessage()}"));
            throw new($"Compilation errors:\n{errorMessages}");
        }

        // Filter to only GQLEF001 diagnostics (ignore debug diagnostics and compilation warnings)
        var gqlef001Diagnostics = allDiagnostics
            .Where(d => d.Id == "GQLEF001")
            .ToArray();

        return gqlef001Diagnostics;
    }
}
