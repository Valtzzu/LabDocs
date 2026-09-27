using LabDocs.Models;
using LabDocs.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace LabDocs.Tests;

public sealed class WikiCatalogServiceTests : IDisposable
{
    private readonly string _tempRoot;

    public WikiCatalogServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), $"LabDocsTests-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempRoot);
    }

    [Fact]
    public void GetPages_ThrowsClearErrorWhenContentDirectoryIsMissing()
    {
        var service = CreateService();

        var exception = Assert.Throws<DirectoryNotFoundException>(() => service.GetPages());

        Assert.Contains("wiki content directory was not found", exception.Message);
        Assert.Contains(Path.Combine(_tempRoot, "content"), exception.Message);
    }

    [Fact]
    public void GetPages_ReturnsEmptyCollectionWhenContentDirectoryIsEmpty()
    {
        Directory.CreateDirectory(Path.Combine(_tempRoot, "content"));
        var service = CreateService();

        IEnumerable<WikiPageEntry> pages = service.GetPages();

        Assert.Empty(pages);
    }

    [Fact]
    public async Task GetPages_RejectsSymbolicDirectory_WhenSupported()
    {
        string contentPath = Path.Combine(_tempRoot, "content");
        string outsidePath = Path.Combine(_tempRoot, "outside");
        string linkPath = Path.Combine(contentPath, "linked-folder");
        Directory.CreateDirectory(contentPath);
        Directory.CreateDirectory(outsidePath);
        await File.WriteAllTextAsync(Path.Combine(outsidePath, "outside-page.md"), "Secret");

        if (TryCreateDirectorySymbolicLink(linkPath, outsidePath) == false)
        {
            return;
        }

        var service = CreateService();

        var exception = Assert.Throws<InvalidDataException>(() => service.GetPages());

        Assert.Contains("Symbolic links", exception.Message);
    }

    public void Dispose()
    {
        Directory.Delete(_tempRoot, recursive: true);
    }

    private WikiCatalogService CreateService()
    {
        var environment = new TestWebHostEnvironment
        {
            ContentRootPath = _tempRoot
        };

        return new WikiCatalogService(environment);
    }

    private static bool TryCreateDirectorySymbolicLink(string linkPath, string targetPath)
    {
        try
        {
            Directory.CreateSymbolicLink(linkPath, targetPath);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (PlatformNotSupportedException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "LabDocs.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
