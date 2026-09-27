using LabDocs.Models;

namespace LabDocs.Tests;

public class WikiNavNodeTests
{
    [Fact]
    public void NewNode_StoresNameAndStartsWithEmptyCollections()
    {
        var node = new WikiNavNode("networking");

        Assert.Equal("networking", node.Name);
        Assert.Empty(node.Pages);
        Assert.Empty(node.Children);
    }
}
