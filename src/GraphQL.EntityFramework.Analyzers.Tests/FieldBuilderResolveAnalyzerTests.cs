using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework.Tests;

public class FieldBuilderResolveAnalyzerTests
{
    [Fact]
    public async Task DetectsResolveWithNavigationPropertyAccess()
    {
        var source = """
            using GraphQL.EntityFramework;
            using GraphQL.Types;
            using Microsoft.EntityFrameworkCore;

            public class ParentEntity { public int Id { get; set; } }
            public class ChildEntity
            {
                public int Id { get; set; }
                public int ParentId { get; set; }
                public ParentEntity Parent { get; set; } = null!;
            }

            public class TestDbContext : DbContext { }

            public class ChildGraphType : EfObjectGraphType<TestDbContext, ChildEntity>
            {
                public ChildGraphType(IEfGraphQLService<TestDbContext> graphQlService) : base(graphQlService)
                {
                    Field<int>("ParentProp")
                        .Resolve(context => context.Source.Parent.Id);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Single(diagnostics);
        Assert.Equal("GQLEF002", diagnostics[0].Id);
    }

    [Fact]
    public async Task DetectsResolveAsyncWithNavigationPropertyAccess()
    {
        var source = """
            using GraphQL.EntityFramework;
            using GraphQL.Types;
            using Microsoft.EntityFrameworkCore;
            using System.Threading.Tasks;

            public class ParentEntity { public int Id { get; set; } }
            public class ChildEntity
            {
                public int Id { get; set; }
                public int ParentId { get; set; }
                public ParentEntity Parent { get; set; } = null!;
            }

            public class TestDbContext : DbContext { }

            public class ChildGraphType : EfObjectGraphType<TestDbContext, ChildEntity>
            {
                public ChildGraphType(IEfGraphQLService<TestDbContext> graphQlService) : base(graphQlService)
                {
                    Field<int>("ParentProp")
                        .ResolveAsync(async context => await Task.FromResult(context.Source.Parent.Id));
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Single(diagnostics);
        Assert.Equal("GQLEF002", diagnostics[0].Id);
    }

    [Fact]
    public async Task DetectsResolveWithDirectNavigationAccess()
    {
        var source = """
            using GraphQL.EntityFramework;
            using GraphQL.Types;
            using Microsoft.EntityFrameworkCore;

            public class ParentEntity { public int Id { get; set; } }
            public class ChildEntity
            {
                public int Id { get; set; }
                public ParentEntity Parent { get; set; } = null!;
            }

            public class TestDbContext : DbContext { }

            public class ChildGraphType : EfObjectGraphType<TestDbContext, ChildEntity>
            {
                public ChildGraphType(IEfGraphQLService<TestDbContext> graphQlService) : base(graphQlService)
                {
                    Field<ParentEntity>("Parent")
                        .Resolve(context => context.Source.Parent);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Single(diagnostics);
        Assert.Equal("GQLEF002", diagnostics[0].Id);
    }

    [Fact]
    public async Task AllowsResolveWithIdPropertyAccess()
    {
        var source = """
            using GraphQL.EntityFramework;
            using GraphQL.Types;
            using Microsoft.EntityFrameworkCore;

            public class ChildEntity
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }

            public class TestDbContext : DbContext { }

            public class ChildGraphType : EfObjectGraphType<TestDbContext, ChildEntity>
            {
                public ChildGraphType(IEfGraphQLService<TestDbContext> graphQlService) : base(graphQlService)
                {
                    Field<int>("EntityId")
                        .Resolve(context => context.Source.Id);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AllowsResolveWithForeignKeyAccess()
    {
        var source = """
            using GraphQL.EntityFramework;
            using GraphQL.Types;
            using Microsoft.EntityFrameworkCore;

            public class ParentEntity { public int Id { get; set; } }
            public class ChildEntity
            {
                public int Id { get; set; }
                public int ParentId { get; set; }
                public ParentEntity Parent { get; set; } = null!;
            }

            public class TestDbContext : DbContext { }

            public class ChildGraphType : EfObjectGraphType<TestDbContext, ChildEntity>
            {
                public ChildGraphType(IEfGraphQLService<TestDbContext> graphQlService) : base(graphQlService)
                {
                    Field<int>("FK")
                        .Resolve(context => context.Source.ParentId);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task DetectsResolveWithScalarProperties()
    {
        var source = """
            using GraphQL.EntityFramework;
            using GraphQL.Types;
            using Microsoft.EntityFrameworkCore;
            using System;

            public class ChildEntity
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
                public int Age { get; set; }
                public DateTime CreatedDate { get; set; }
                public bool IsActive { get; set; }
            }

            public class TestDbContext : DbContext { }

            public class ChildGraphType : EfObjectGraphType<TestDbContext, ChildEntity>
            {
                public ChildGraphType(IEfGraphQLService<TestDbContext> graphQlService) : base(graphQlService)
                {
                    Field<string>("FullName")
                        .Resolve(context => $"{context.Source.Name} - {context.Source.Age}");
                    Field<bool>("Active")
                        .Resolve(context => context.Source.IsActive && context.Source.CreatedDate > DateTime.Now);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        // Should warn because Name, Age, IsActive, CreatedDate are scalar properties, not PK/FK
        Assert.Equal(2, diagnostics.Length); // Two Resolve calls
        Assert.All(diagnostics, d => Assert.Equal("GQLEF002", d.Id));
    }

    [Fact]
    public async Task AllowsProjectionBasedResolve()
    {
        var source = """
            using GraphQL.EntityFramework;
            using GraphQL.Types;
            using Microsoft.EntityFrameworkCore;

            public class ParentEntity
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }
            public class ChildEntity
            {
                public int Id { get; set; }
                public int ParentId { get; set; }
                public ParentEntity Parent { get; set; } = null!;
            }

            public class TestDbContext : DbContext { }

            public class ChildGraphType : EfObjectGraphType<TestDbContext, ChildEntity>
            {
                public ChildGraphType(IEfGraphQLService<TestDbContext> graphQlService) : base(graphQlService)
                {
                    Field<string>("ParentName")
                        .Resolve<TestDbContext, ChildEntity, string, ParentEntity>(
                            projection: x => x.Parent,
                            resolve: ctx => ctx.Projection.Name);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AllowsProjectionBasedResolveAsync()
    {
        var source = """
            using GraphQL.EntityFramework;
            using GraphQL.Types;
            using Microsoft.EntityFrameworkCore;
            using System.Threading.Tasks;

            public class ParentEntity
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }
            public class ChildEntity
            {
                public int Id { get; set; }
                public int ParentId { get; set; }
                public ParentEntity Parent { get; set; } = null!;
            }

            public class TestDbContext : DbContext { }

            public class ChildGraphType : EfObjectGraphType<TestDbContext, ChildEntity>
            {
                public ChildGraphType(IEfGraphQLService<TestDbContext> graphQlService) : base(graphQlService)
                {
                    Field<string>("ParentName")
                        .ResolveAsync<TestDbContext, ChildEntity, string, ParentEntity>(
                            projection: x => x.Parent,
                            resolve: async ctx => await Task.FromResult(ctx.Projection.Name));
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task IgnoresNonEfGraphTypeClasses()
    {
        var source = """
            using GraphQL.Types;
            using Microsoft.EntityFrameworkCore;

            public class ParentEntity { public int Id { get; set; } }
            public class ChildEntity
            {
                public int Id { get; set; }
                public ParentEntity Parent { get; set; } = null!;
            }

            public class RegularGraphType : ObjectGraphType<ChildEntity>
            {
                public RegularGraphType()
                {
                    Field<int>("ParentId")
                        .Resolve(context => context.Source.Parent.Id);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AllowsResolveWithOnlyPKAndFK()
    {
        var source = """
            using GraphQL.EntityFramework;
            using GraphQL.Types;
            using Microsoft.EntityFrameworkCore;

            public class ParentEntity { public int Id { get; set; } }
            public class ChildEntity
            {
                public int Id { get; set; }
                public int ParentId { get; set; }
                public int? OptionalParentId { get; set; }
                public ParentEntity Parent { get; set; } = null!;
            }

            public class TestDbContext : DbContext { }

            public class ChildGraphType : EfObjectGraphType<TestDbContext, ChildEntity>
            {
                public ChildGraphType(IEfGraphQLService<TestDbContext> graphQlService) : base(graphQlService)
                {
                    Field<string>("Keys")
                        .Resolve(context => $"{context.Source.Id}-{context.Source.ParentId}-{context.Source.OptionalParentId}");
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task DetectsCollectionNavigationAccess()
    {
        var source = """
            using GraphQL.EntityFramework;
            using GraphQL.Types;
            using Microsoft.EntityFrameworkCore;
            using System.Collections.Generic;
            using System.Linq;

            public class ChildEntity { public int Id { get; set; } }
            public class ParentEntity
            {
                public int Id { get; set; }
                public List<ChildEntity> Children { get; set; } = new();
            }

            public class TestDbContext : DbContext { }

            public class ParentGraphType : EfObjectGraphType<TestDbContext, ParentEntity>
            {
                public ParentGraphType(IEfGraphQLService<TestDbContext> graphQlService) : base(graphQlService)
                {
                    Field<int>("ChildCount")
                        .Resolve(context => context.Source.Children.Count());
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Single(diagnostics);
        Assert.Equal("GQLEF002", diagnostics[0].Id);
    }

    static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = new List<MetadataReference>();

        // Add basic references
        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(Console).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Collections.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Linq.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Linq.Expressions.dll")));

        // Add GraphQL references
        references.Add(MetadataReference.CreateFromFile(typeof(GraphQL.IResolveFieldContext).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(GraphQL.Types.ObjectGraphType).Assembly.Location));

        // Add EF Core references
        references.Add(MetadataReference.CreateFromFile(typeof(DbContext).Assembly.Location));

        // Add GraphQL.EntityFramework reference
        var entityFrameworkAssembly = typeof(GraphQL.EntityFramework.EfObjectGraphType<,>).Assembly;
        references.Add(MetadataReference.CreateFromFile(entityFrameworkAssembly.Location));

        // Add referenced assemblies
        foreach (var referencedAssembly in entityFrameworkAssembly.GetReferencedAssemblies())
        {
            try
            {
                var assembly = Assembly.Load(referencedAssembly);
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
            catch
            {
                // Ignore if assembly can't be loaded
            }
        }

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        var compilationDiagnostics = compilation.GetDiagnostics();
        var errors = compilationDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

        if (errors.Count > 0)
        {
            var errorMessages = string.Join("\n", errors.Select(e => e.ToString()));
            throw new InvalidOperationException($"Compilation errors:\n{errorMessages}");
        }

        // Load analyzer from DLL
        var analyzerAssemblyPath = Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(typeof(FieldBuilderResolveAnalyzerTests).Assembly.Location)!,
            "..", "..", "..", "..", "GraphQL.EntityFramework.Analyzers", "bin", "Release", "netstandard2.0", "GraphQL.EntityFramework.Analyzers.dll"));
        var analyzerAssembly = Assembly.LoadFrom(analyzerAssemblyPath);
        var analyzerType = analyzerAssembly.GetType("GraphQL.EntityFramework.Analyzers.FieldBuilderResolveAnalyzer")!;
        var analyzer = (DiagnosticAnalyzer)Activator.CreateInstance(analyzerType)!;

        var compilationWithAnalyzers = compilation.WithAnalyzers(
            ImmutableArray.Create(analyzer));

        var analyzerDiagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
        return analyzerDiagnostics;
    }
}
