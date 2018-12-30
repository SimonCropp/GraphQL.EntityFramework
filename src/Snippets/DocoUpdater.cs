using System.IO;
using System.Linq;
using CaptureSnippets;
using Xunit;

public class DocoUpdater
{
    [Fact]
    public void Run()
    {
        var root = GitRepoDirectoryFinder.Find();

        var files = Directory.EnumerateFiles(Path.Combine(root, "src/SampleWeb.Tests"), "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(Path.Combine(root, "src/Snippets"), "*.cs", SearchOption.AllDirectories))
            .Concat(Directory.EnumerateFiles(Path.Combine(root, "src/SampleWeb"), "*.cs", SearchOption.AllDirectories));
        DirectorySourceMarkdownProcessor.Run(root, files);
    }
}