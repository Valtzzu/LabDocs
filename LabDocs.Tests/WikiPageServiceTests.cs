using Ganss.Xss;
using LabDocs.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace LabDocs.Tests;

public sealed class WikiPageServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly string _contentRoot;

    public WikiPageServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), $"LabDocsTests-{Guid.NewGuid()}");
        _contentRoot = Path.Combine(_tempRoot, "content");
        Directory.CreateDirectory(_contentRoot);
    }

    [Fact]
    public async Task LoadPageAsync_LoadsNestedMarkdownPage()
    {
        var folder = Path.Combine(_contentRoot, "guides");
        Directory.CreateDirectory(folder);
        await File.WriteAllTextAsync(Path.Combine(folder, "deployment-guide.md"), "# Deployment guide");

        var service = CreateService();

        var htmlContent = await service.LoadPageAsync("guides/deployment-guide");

        Assert.Contains("<h1", htmlContent);
        Assert.Contains(">Deployment guide</h1>", htmlContent);
    }

    [Fact]
    public async Task LoadPageAsync_ReturnsNullWhenPageDoesNotExist()
    {
        var service = CreateService();

        var html = await service.LoadPageAsync("missing-page");

        Assert.Null(html);
    }

    [Fact]
    public async Task LoadPageAsync_ThrowsClearErrorWhenContentDirectoryIsMissing()
    {
        Directory.Delete(_contentRoot);
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<DirectoryNotFoundException>(
            () => service.LoadPageAsync("missing-page"));

        Assert.Contains("wiki content directory was not found", exception.Message);
        Assert.Contains(_contentRoot, exception.Message);
    }

    [Fact]
    public async Task LoadPageAsync_RejectsPathOutsideContentDirectory()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempRoot, "outside.md"), "Secret");
        var service = CreateService();

        var html = await service.LoadPageAsync("../outside");

        Assert.Null(html);
    }

    [Fact]
    public async Task LoadPageAsync_RejectsSymbolicLinkOutsideContentDirectory_WhenSupported()
    {
        string outsidePath = Path.Combine(_tempRoot, "outside.md");
        string linkPath = Path.Combine(_contentRoot, "linked-page.md");
        await File.WriteAllTextAsync(outsidePath, "Secret");

        if (TryCreateFileSymbolicLink(linkPath, outsidePath) == false)
        {
            return;
        }

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<InvalidDataException>(
            () => service.LoadPageAsync("linked-page"));

        Assert.Contains("Symbolic links", exception.Message);
    }

    [Fact]
    public async Task LoadPageAsync_RejectsSlugContainingUppercaseLetters()
    {
        await File.WriteAllTextAsync(Path.Combine(_contentRoot, "mixed-case.md"), "# Mixed case");
        var service = CreateService();

        var html = await service.LoadPageAsync("MixedCase");

        Assert.Null(html);
    }

    [Fact]
    public async Task LoadPageAsync_RendersSafeMarkdownFeatures()
    {
        string markdown =
            "# Safe heading\n\n" +
            "| Name | Value |\n" +
            "| --- | --- |\n" +
            "| Security | Enabled |\n\n" +
            "[Relative link](/docs/home)\n\n" +
            "[HTTPS link](https://example.com)\n\n" +
            "```xml\n<key>value</key>\n```";
        await CreatePageAsync("safe-markdown", markdown);
        var service = CreateService();

        var html = await service.LoadPageAsync("safe-markdown");

        Assert.Contains("<h1", html);
        Assert.Contains("<table>", html);
        Assert.Contains("href=\"/docs/home\"", html);
        Assert.Contains("href=\"https://example.com\"", html);
        Assert.Contains("&lt;key&gt;value&lt;/key&gt;", html);
    }

    [Fact]
    public async Task LoadPageAsync_DoesNotRenderRawHtml()
    {
        string markdown =
            "<script>alert('xss')</script>\n\n" +
            "<strong>Raw HTML</strong>";
        await CreatePageAsync("raw-html", markdown);
        var service = CreateService();

        var html = await service.LoadPageAsync("raw-html");

        Assert.DoesNotContain("<script", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<strong>", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadPageAsync_RemovesUnsafeAttributes()
    {
        string markdown = "# Unsafe heading {onclick=\"alert('xss')\"}";
        await CreatePageAsync("unsafe-attribute", markdown);
        var service = CreateService();

        var html = await service.LoadPageAsync("unsafe-attribute");

        Assert.Contains("Unsafe heading", html);
        Assert.DoesNotContain("onclick", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadPageAsync_RemovesUnsafeUrlSchemes()
    {
        string markdown =
            "[JavaScript](javascript:alert)\n\n" +
            "[VBScript](vbscript:msgbox)\n\n" +
            "![Data image](data:text/html;base64,PHNjcmlwdD4=)\n\n" +
            "[Relative](/docs/home)\n\n" +
            "[HTTPS](https://example.com)";
        await CreatePageAsync("unsafe-links", markdown);
        var service = CreateService();

        var html = await service.LoadPageAsync("unsafe-links");

        Assert.DoesNotContain("javascript:", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("vbscript:", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data:", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("href=\"/docs/home\"", html);
        Assert.Contains("href=\"https://example.com\"", html);
    }

    public void Dispose()
    {
        Directory.Delete(_tempRoot, recursive: true);
    }

    private WikiPageService CreateService()
    {
        var environment = new TestWebHostEnvironment
        {
            ContentRootPath = _tempRoot
        };

        return new WikiPageService(environment, new HtmlSanitizer());
    }

    private async Task CreatePageAsync(string slug, string markdown)
    {
        string filePath = Path.Combine(_contentRoot, $"{slug}.md");
        await File.WriteAllTextAsync(filePath, markdown);
    }

    private static bool TryCreateFileSymbolicLink(string linkPath, string targetPath)
    {
        try
        {
            File.CreateSymbolicLink(linkPath, targetPath);
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
