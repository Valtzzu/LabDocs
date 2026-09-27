using LabDocs.Services;

namespace LabDocs.Tests;

public class WikiSlugValidatorTests
{
    [Theory]
    [InlineData("documentation")]
    [InlineData("release-notes")]
    [InlineData("guides/getting-started")]
    [InlineData("reference/api-version-2")]
    public void IsValid_ReturnsTrueForValidSlug(string slug)
    {
        bool result = WikiSlugValidator.IsValid(slug);

        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Documentation")]
    [InlineData("nested folder/page")]
    [InlineData("release_notes")]
    [InlineData("release--notes")]
    [InlineData("-release-notes")]
    [InlineData("release-notes-")]
    [InlineData("guides/getting-started.md")]
    [InlineData("../outside")]
    public void IsValid_ReturnsFalseForInvalidSlug(string slug)
    {
        bool result = WikiSlugValidator.IsValid(slug);

        Assert.False(result);
    }
}
