using LabDocs.Services;

namespace LabDocs.Tests;

public class WikiTitleFormatterTests
{
    [Theory]
    [InlineData("documentation", "Documentation")]
    [InlineData("release-notes", "Release Notes")]
    [InlineData("api-reference-notes-en", "Api Reference Notes En")]
    public void FormatSegment_ReturnsReadableTitle(string segment, string expectedTitle)
    {
        string title = WikiTitleFormatter.FormatSegment(segment);

        Assert.Equal(expectedTitle, title);
    }

    [Fact]
    public void FormatSlug_UsesFinalPathSegment()
    {
        string title = WikiTitleFormatter.FormatSlug("guides/installation-basics");

        Assert.Equal("Installation Basics", title);
    }
}
