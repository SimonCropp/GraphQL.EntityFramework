public class FilterIdentityProjectionCodeFixProviderTests
{
    [Fact]
    public async Task RemovesIdentityProjectionParameter()
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
                public void ConfigureFilters(Filters<TestDbContext> filters, Guid targetId)
                {
                    filters.For<TestEntity>().Add(
                        projection: _ => _,
                        filter: (_, _, _, e) => e.Id == targetId);
                }
            }
            """;

        var expected = """
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
                public void ConfigureFilters(Filters<TestDbContext> filters, Guid targetId)
                {
                    filters.For<TestEntity>().Add(
                        filter: (_, _, _, e) => e.Id == targetId);
                }
            }
            """;

        var actual = await ApplyCodeFixAsync(source);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task RemovesIdentityProjectionParameter_WithMultilineFormatting()
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
                public void ConfigureFilters(Filters<TestDbContext> filters)
                {
                    filters.For<TestEntity>().Add(
                        projection: _ => _,
                        filter: (_, _, _, e) =>
                        {
                            return e.ParentId != null;
                        });
                }
            }
            """;

        var expected = """
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
                public void ConfigureFilters(Filters<TestDbContext> filters)
                {
                    filters.For<TestEntity>().Add(
                        filter: (_, _, _, e) =>
                        {
                            return e.ParentId != null;
                        });
                }
            }
            """;

        var actual = await ApplyCodeFixAsync(source);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task RemovesIdentityProjectionParameter_PreservesFilterFormatting()
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
                public void ConfigureFilters(Filters<TestDbContext> filters, int categoryId)
                {
                    filters.For<TestEntity>().Add(
                        projection: _ => _,
                        filter: (_, _, _, e) => e.CategoryId == categoryId);
                }
            }
            """;

        var expected = """
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
                public void ConfigureFilters(Filters<TestDbContext> filters, int categoryId)
                {
                    filters.For<TestEntity>().Add(
                        filter: (_, _, _, e) => e.CategoryId == categoryId);
                }
            }
            """;

        var actual = await ApplyCodeFixAsync(source);
        Assert.Equal(expected, actual);
    }

    static async Task<string> ApplyCodeFixAsync(string source)
    {
        var document = CreateDocument(source);
        var compilation = await document.Project.GetCompilationAsync();
        if (compilation == null)
        {
            throw new("Failed to get compilation");
        }

        var analyzer = new FilterIdentityProjectionAnalyzer();
        var compilationWithAnalyzers = compilation.WithAnalyzers([analyzer]);

        var allDiagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync();
        var diagnostic = allDiagnostics.FirstOrDefault(_ => _.Id == "GQLEF004");

        if (diagnostic == null)
        {
            throw new("No GQLEF004 diagnostic found");
        }

        var codeFixProvider = new FilterIdentityProjectionCodeFixProvider();
        var actions = new List<CodeAction>();
        var context = new CodeFixContext(
            document,
            diagnostic,
            (action, _) => actions.Add(action),
            default);

        await codeFixProvider.RegisterCodeFixesAsync(context);

        if (actions.Count == 0)
        {
            throw new($"No code fixes were registered. Diagnostic: {diagnostic}");
        }

        // Get operations and apply changes
        var operations = await actions[0].GetOperationsAsync(default);
        if (operations.IsEmpty)
        {
            throw new("Code action returned no operations");
        }

        var operation = operations.OfType<ApplyChangesOperation>().FirstOrDefault();
        if (operation == null)
        {
            throw new($"No ApplyChangesOperation found. Operations: {string.Join(", ", operations.Select(o => o.GetType().Name))}");
        }

        var changedSolution = operation.ChangedSolution;
        var changedDocument = changedSolution.GetDocument(document.Id);
        if (changedDocument == null)
        {
            throw new("Failed to get changed document");
        }

        var root = await changedDocument.GetSyntaxRootAsync();
        return root?.ToFullString() ?? "";
    }

    static Document CreateDocument(string source)
    {
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(DbContext).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Filters<>).Assembly.Location),
        };

        var requiredAssemblies = new[]
        {
            typeof(Guid).Assembly,
            Assembly.Load("System.Runtime"),
            typeof(IQueryable<>).Assembly,
            Assembly.Load("System.Security.Claims"),
        };

        foreach (var assembly in requiredAssemblies)
        {
            if (!string.IsNullOrEmpty(assembly.Location))
            {
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

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

        var solution = new AdhocWorkspace()
            .CurrentSolution
            .AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp)
            .WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddMetadataReferences(projectId, references)
            .AddDocument(documentId, "TestDocument.cs", source);

        return solution.GetDocument(documentId)!;
    }
}
