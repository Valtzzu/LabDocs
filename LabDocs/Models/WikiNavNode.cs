namespace LabDocs.Models;

/// <summary>
/// Represents a folder-like node in the wiki navigation tree.
/// </summary>
public class WikiNavNode
{
    public string Name {get;} 
    public List<WikiPageEntry> Pages { get; } = new();
    public List<WikiNavNode> Children { get; } = new();

    public WikiNavNode(string name) {
        Name = name;
    }

}
