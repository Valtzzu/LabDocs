using LabDocs.Models;
using LabDocs.Services;
using LabDocs.ViewComponents;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.FileProviders;

namespace LabDocs.Tests;

public sealed class WikiNavViewComponentTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly string _contentRoot;

    public WikiNavViewComponentTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), $"LabDocsTests-{Guid.NewGuid()}");
        _contentRoot = Path.Combine(_tempRoot, "content");
        Directory.CreateDirectory(_contentRoot);
    }

    [Fact]
    public async Task Invoke_SortsPagesAndFoldersAlphabetically()
    {
        await CreatePageAsync("z-page.md");
        await CreatePageAsync("a-page.md");
        await CreatePageAsync("z-folder/z-child.md");
        await CreatePageAsync("a-folder/z-child.md");
        await CreatePageAsync("a-folder/a-child.md");

        var environment = new TestWebHostEnvironment
        {
            ContentRootPath = _tempRoot
        };
        var catalogService = new WikiCatalogService(environment);
        var component = new WikiNavViewComponent(catalogService);

        var result = Assert.IsType<ViewViewComponentResult>(component.Invoke());
        Assert.NotNull(result.ViewData);
        var root = Assert.IsType<WikiNavNode>(result.ViewData.Model);

        Assert.Collection(
            root.Pages,
            firstPage => Assert.Equal("A Page", firstPage.Title),
            secondPage => Assert.Equal("Z Page", secondPage.Title));

        Assert.Collection(
            root.Children,
            firstFolder => Assert.Equal("a-folder", firstFolder.Name),
            secondFolder => Assert.Equal("z-folder", secondFolder.Name));

        WikiNavNode firstFolder = root.Children[0];

        Assert.Collection(
            firstFolder.Pages,
            firstPage => Assert.Equal("A Child", firstPage.Title),
            secondPage => Assert.Equal("Z Child", secondPage.Title));
    }

    public void Dispose()
    {
        Directory.Delete(_tempRoot, recursive: true);
    }

    private async Task CreatePageAsync(string relativePath)
    {
        string filePath = Path.Combine(_contentRoot, relativePath);
        string? directoryPath = Path.GetDirectoryName(filePath);

        if (directoryPath is not null)
        {
            Directory.CreateDirectory(directoryPath);
        }

        await File.WriteAllTextAsync(filePath, "# Test page");
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
