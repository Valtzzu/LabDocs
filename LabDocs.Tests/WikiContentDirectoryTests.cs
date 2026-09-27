using LabDocs.Services;

namespace LabDocs.Tests;

public class WikiContentDirectoryTests
{
    [Theory]
    [InlineData(FileAttributes.ReparsePoint, true)]
    [InlineData(FileAttributes.Directory | FileAttributes.ReparsePoint, true)]
    [InlineData(FileAttributes.Normal, false)]
    [InlineData(FileAttributes.Directory, false)]
    public void IsSymbolicLinkOrJunction_DetectsReparsePoint(
        FileAttributes attributes,
        bool expectedResult)
    {
        bool result = WikiContentDirectory.IsSymbolicLinkOrJunction(attributes);

        Assert.Equal(expectedResult, result);
    }
}
