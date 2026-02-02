public class FilterIdentityProjectionAnalyzerTests
{
    // GQLEF004 Tests - Suggest simplified API

    [Fact]
    public async Task GQLEF004_SuggestsSimplifiedAPI_ForIdentityProjectionWithPKAccess()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;

            public class TestEntity
            {
                public Guid Id { get; set; }
                public string Name { get; set; } = "";
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public void ConfigureFilters(Filters<TestDbContext> filters, Guid targetId)
                {
                    filters.For<TestEntity>().Add(
                        projection: _ => _,
                        filter: (_, _, _, e) => e.Id == targetId);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Single(diagnostics);
        Assert.Equal("GQLEF004", diagnostics[0].Id);
    }

    [Fact]
    public async Task GQLEF004_SuggestsSimplifiedAPI_ForIdentityProjectionWithFKAccess()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;

            public class TestEntity
            {
                public Guid Id { get; set; }
                public Guid? ParentId { get; set; }
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public void ConfigureFilters(Filters<TestDbContext> filters, Guid targetId)
                {
                    filters.For<TestEntity>().Add(
                        projection: _ => _,
                        filter: (_, _, _, e) => e.ParentId == targetId);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Single(diagnostics);
        Assert.Equal("GQLEF004", diagnostics[0].Id);
    }

    [Fact]
    public async Task GQLEF004_SuggestsSimplifiedAPI_ForIdentityProjectionWithMultipleFKAccess()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;

            public class TestEntity
            {
                public Guid Id { get; set; }
                public Guid? ParentId { get; set; }
                public int? CategoryId { get; set; }
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public void ConfigureFilters(Filters<TestDbContext> filters, Guid parentId, int categoryId)
                {
                    filters.For<TestEntity>().Add(
                        projection: _ => _,
                        filter: (_, _, _, e) => e.ParentId == parentId || e.CategoryId == categoryId);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Single(diagnostics);
        Assert.Equal("GQLEF004", diagnostics[0].Id);
    }

    [Fact]
    public async Task GQLEF004_NoWarning_ForNonIdentityProjection()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;

            public class TestEntity
            {
                public Guid Id { get; set; }
                public string Name { get; set; } = "";
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public void ConfigureFilters(Filters<TestDbContext> filters)
                {
                    filters.For<TestEntity>().Add(
                        projection: e => e.Name,
                        filter: (_, _, _, name) => name != "");
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Empty(diagnostics);
    }

    // GQLEF005 Tests - Invalid simplified API usage

    [Fact]
    public async Task GQLEF005_Error_ForSimplifiedAPIAccessingScalarProperty()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;

            public class TestEntity
            {
                public Guid Id { get; set; }
                public string Name { get; set; } = "";
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public void ConfigureFilters(Filters<TestDbContext> filters)
                {
                    filters.For<TestEntity>().Add(
                        filter: (_, _, _, e) => e.Name == "Test");
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Single(diagnostics);
        Assert.Equal("GQLEF005", diagnostics[0].Id);
        Assert.Contains("Name", diagnostics[0].GetMessage());
    }

    [Fact]
    public async Task GQLEF005_Error_ForSimplifiedAPIAccessingNavigationProperty()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;

            public class ParentEntity { public Guid Id { get; set; } }
            public class TestEntity
            {
                public Guid Id { get; set; }
                public Guid? ParentId { get; set; }
                public ParentEntity? Parent { get; set; }
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public void ConfigureFilters(Filters<TestDbContext> filters)
                {
                    filters.For<TestEntity>().Add(
                        filter: (_, _, _, e) => e.Parent != null);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Single(diagnostics);
        Assert.Equal("GQLEF005", diagnostics[0].Id);
        Assert.Contains("Parent", diagnostics[0].GetMessage());
    }

    [Fact]
    public async Task GQLEF005_NoError_ForSimplifiedAPIAccessingPK()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;

            public class TestEntity
            {
                public Guid Id { get; set; }
                public string Name { get; set; } = "";
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public void ConfigureFilters(Filters<TestDbContext> filters, Guid targetId)
                {
                    filters.For<TestEntity>().Add(
                        filter: (_, _, _, e) => e.Id == targetId);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task GQLEF005_NoError_ForSimplifiedAPIAccessingFK()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;

            public class TestEntity
            {
                public Guid Id { get; set; }
                public Guid? ParentId { get; set; }
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public void ConfigureFilters(Filters<TestDbContext> filters, Guid parentId)
                {
                    filters.For<TestEntity>().Add(
                        filter: (_, _, _, e) => e.ParentId == parentId);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task GQLEF005_NoError_ForSimplifiedAPIAccessingNullableFK()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;

            public class TestEntity
            {
                public Guid Id { get; set; }
                public int? CategoryId { get; set; }
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public void ConfigureFilters(Filters<TestDbContext> filters)
                {
                    filters.For<TestEntity>().Add(
                        filter: (_, _, _, e) => e.CategoryId != null);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Empty(diagnostics);
    }

    // GQLEF006 Tests - Invalid identity projection with existing API

    [Fact]
    public async Task GQLEF006_Error_ForIdentityProjectionAccessingScalarProperty()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;

            public class TestEntity
            {
                public Guid Id { get; set; }
                public string City { get; set; } = "";
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public void ConfigureFilters(Filters<TestDbContext> filters)
                {
                    filters.For<TestEntity>().Add(
                        projection: _ => _,
                        filter: (_, _, _, e) => e.City == "London");
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Single(diagnostics);
        Assert.Equal("GQLEF006", diagnostics[0].Id);
        Assert.Contains("City", diagnostics[0].GetMessage());
    }

    [Fact]
    public async Task GQLEF006_Error_ForIdentityProjectionAccessingNavigationProperty()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;

            public class ParentEntity { public Guid Id { get; set; } }
            public class TestEntity
            {
                public Guid Id { get; set; }
                public ParentEntity? Parent { get; set; }
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public void ConfigureFilters(Filters<TestDbContext> filters)
                {
                    filters.For<TestEntity>().Add(
                        projection: _ => _,
                        filter: (_, _, _, e) => e.Parent != null);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Single(diagnostics);
        Assert.Equal("GQLEF006", diagnostics[0].Id);
        Assert.Contains("Parent", diagnostics[0].GetMessage());
    }

    // Edge Cases

    [Fact]
    public async Task EdgeCase_DiscardParameter()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;

            public class TestEntity
            {
                public Guid Id { get; set; }
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public void ConfigureFilters(Filters<TestDbContext> filters)
                {
                    filters.For<TestEntity>().Add(
                        filter: (_, _, _, _) => true);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task EdgeCase_TypeNameWithEntitySuffix()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;

            public class CompanyEntity
            {
                public Guid CompanyId { get; set; }
                public string Name { get; set; } = "";
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public void ConfigureFilters(Filters<TestDbContext> filters, Guid companyId)
                {
                    filters.For<CompanyEntity>().Add(
                        filter: (_, _, _, e) => e.CompanyId == companyId);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task EdgeCase_ComplexExpression()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;

            public class TestEntity
            {
                public Guid Id { get; set; }
                public Guid? ParentId { get; set; }
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public bool IsValidId(Guid? id) => id.HasValue;

                public void ConfigureFilters(Filters<TestDbContext> filters)
                {
                    filters.For<TestEntity>().Add(
                        filter: (_, _, _, e) => IsValidId(e.ParentId));
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task EdgeCase_MultiplePropertyAccess()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;

            public class TestEntity
            {
                public Guid Id { get; set; }
                public Guid? ParentId { get; set; }
                public int? CategoryId { get; set; }
                public string Name { get; set; } = "";
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public void ConfigureFilters(Filters<TestDbContext> filters, Guid parentId)
                {
                    filters.For<TestEntity>().Add(
                        projection: _ => _,
                        filter: (_, _, _, e) => e.ParentId == parentId && e.Name == "Test");
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        // Should report error for Name access (first non-key property encountered)
        Assert.Single(diagnostics);
        Assert.Equal("GQLEF006", diagnostics[0].Id);
        Assert.Contains("Name", diagnostics[0].GetMessage());
    }

    [Fact]
    public async Task EdgeCase_AsyncFilter()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;
            using System.Threading.Tasks;

            public class TestEntity
            {
                public Guid Id { get; set; }
                public Guid? ParentId { get; set; }
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public void ConfigureFilters(Filters<TestDbContext> filters, Guid parentId)
                {
                    filters.For<TestEntity>().Add(
                        filter: async (_, _, _, e) =>
                        {
                            await Task.Delay(1);
                            return e.ParentId == parentId;
                        });
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Empty(diagnostics);
    }

    static async Task<Diagnostic[]> GetDiagnosticsAsync(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(DbContext).Assembly.Location)
        };

        // Add GraphQL.EntityFramework assembly
        var efAssembly = typeof(Filters<>).Assembly;
        references.Add(MetadataReference.CreateFromFile(efAssembly.Location));

        // Add required assemblies for compilation
        var requiredAssemblies = new[]
        {
            typeof(Guid).Assembly,
            // System.Runtime
            Assembly.Load("System.Runtime"),
            // System.Linq.Expressions
            typeof(IQueryable<>).Assembly,
            // System.Security.Claims
            Assembly.Load("System.Security.Claims"),
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
                references.All(_ => _.Display != assembly.Location))
            {
                var name = assembly.GetName().Name ?? "";
                if ((!name.StartsWith("System.") &&
                     !name.StartsWith("Microsoft.")) ||
                    name.Contains("xunit", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

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

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        var analyzer = new FilterIdentityProjectionAnalyzer();
        var compilationWithAnalyzers = compilation.WithAnalyzers([analyzer]);

        var allDiagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync();

        // Check for compilation errors
        var compilationErrors = allDiagnostics.Where(_ => _.Severity == DiagnosticSeverity.Error && !_.Id.StartsWith("GQLEF")).ToArray();
        if (compilationErrors.Length > 0)
        {
            var errorMessages = string.Join("\n", compilationErrors.Select(_ => $"{_.Id}: {_.GetMessage()}"));
            throw new($"Compilation errors:\n{errorMessages}");
        }

        // Filter to only GQLEF004, GQLEF005, GQLEF006, and GQLEF007 diagnostics
        return allDiagnostics
            .Where(_ => _.Id == "GQLEF004" || _.Id == "GQLEF005" || _.Id == "GQLEF006" || _.Id == "GQLEF007")
            .ToArray();
    }

    // GQLEF007 Tests - Identity projection with abstract navigation access

    [Fact]
    public async Task GQLEF007_Reports_IdentityProjection_WithAbstractNavigationAccess()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;

            public abstract class BaseEntity
            {
                public Guid Id { get; set; }
                public string Property { get; set; } = "";
            }

            public class DerivedEntity : BaseEntity { }

            public class ChildEntity
            {
                public Guid Id { get; set; }
                public Guid? ParentId { get; set; }
                public BaseEntity? Parent { get; set; }
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public void ConfigureFilters(Filters<TestDbContext> filters)
                {
                    filters.For<ChildEntity>().Add(
                        projection: _ => _,
                        filter: (_, _, _, c) => c.Parent!.Property == "test");
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Single(diagnostics);
        Assert.Equal("GQLEF007", diagnostics[0].Id);
        Assert.Contains("Parent", diagnostics[0].GetMessage());
    }

    [Fact]
    public async Task GQLEF007_Reports_FourParamFilter_WithAbstractNavigationAccess()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;

            public abstract class BaseEntity
            {
                public Guid Id { get; set; }
                public string Status { get; set; } = "";
            }

            public class ChildEntity
            {
                public Guid Id { get; set; }
                public BaseEntity? Parent { get; set; }
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public void ConfigureFilters(Filters<TestDbContext> filters)
                {
                    filters.For<ChildEntity>().Add(
                        filter: (_, _, _, c) => c.Parent!.Status == "Active");
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Single(diagnostics);
        Assert.Equal("GQLEF007", diagnostics[0].Id);
    }

    [Fact]
    public async Task GQLEF007_NoReport_ConcreteNavigationAccess()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;

            public class ConcreteParent
            {
                public Guid Id { get; set; }
                public string Property { get; set; } = "";
            }

            public class ChildEntity
            {
                public Guid Id { get; set; }
                public ConcreteParent? Parent { get; set; }
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public void ConfigureFilters(Filters<TestDbContext> filters)
                {
                    filters.For<ChildEntity>().Add(
                        projection: _ => _,
                        filter: (_, _, _, c) => c.Parent!.Property == "test");
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        // Should report GQLEF006 (non-key property access) but not GQLEF007
        Assert.DoesNotContain(diagnostics, d => d.Id == "GQLEF007");
    }

    [Fact]
    public async Task GQLEF007_NoReport_ExplicitProjection_WithAbstractNavigation()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;

            public abstract class BaseEntity
            {
                public Guid Id { get; set; }
                public string Property { get; set; } = "";
            }

            public class ChildEntity
            {
                public Guid Id { get; set; }
                public BaseEntity? Parent { get; set; }
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public void ConfigureFilters(Filters<TestDbContext> filters)
                {
                    filters.For<ChildEntity>().Add(
                        projection: c => new { c.Id, ParentProp = c.Parent!.Property },
                        filter: (_, _, _, proj) => proj.ParentProp == "test");
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        // Explicit projection - analyzer doesn't catch this (runtime does)
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task GQLEF007_Reports_NestedAbstractNavigationAccess()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;
            using System.Collections.Generic;

            public abstract class BaseEntity
            {
                public Guid Id { get; set; }
                public List<ChildEntity> Children { get; set; } = new();
            }

            public class ChildEntity
            {
                public Guid Id { get; set; }
                public BaseEntity? Parent { get; set; }
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public void ConfigureFilters(Filters<TestDbContext> filters)
                {
                    filters.For<ChildEntity>().Add(
                        projection: _ => _,
                        filter: (_, _, _, c) => c.Parent!.Children.Count > 0);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Single(diagnostics);
        Assert.Equal("GQLEF007", diagnostics[0].Id);
        Assert.Contains("Parent", diagnostics[0].GetMessage());
    }
}
