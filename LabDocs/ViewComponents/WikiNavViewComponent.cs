using LabDocs.Services;
using LabDocs.Models;
using Microsoft.AspNetCore.Mvc;
namespace LabDocs.ViewComponents;

public class WikiNavViewComponent : ViewComponent {

    private readonly WikiCatalogService _catalogService;
    public WikiNavViewComponent(WikiCatalogService catalogService) {
        _catalogService = catalogService;
    }
public IViewComponentResult Invoke()
    {
       var pages = _catalogService.GetPages();
       var navigationTree = BuildNavigationTree(pages);
       SortNavigationTree(navigationTree);
       return View(navigationTree);
    }

   private WikiNavNode BuildNavigationTree(IEnumerable<WikiPageEntry> pages)
    {
        var root = new WikiNavNode("root");

        foreach (var page in pages)
        {
            var segments = page.Slug.Split('/');
            var currentNode = root;

            foreach (var folderSegment in segments[..^1])
            {
                var childNode = currentNode.Children
                    .FirstOrDefault(child => child.Name == folderSegment);

                if (childNode is null)
                {
                    childNode = new WikiNavNode(folderSegment);
                    currentNode.Children.Add(childNode);
                }

                currentNode = childNode;
            }

            currentNode.Pages.Add(page);
        }

        return root;
    }

    private void SortNavigationTree(WikiNavNode node)
    {
        node.Pages.Sort(ComparePagesByTitle);
        node.Children.Sort(CompareFoldersByName);

        foreach (WikiNavNode child in node.Children)
        {
            SortNavigationTree(child);
        }
    }

    private static int ComparePagesByTitle(WikiPageEntry firstPage, WikiPageEntry secondPage)
    {
        return StringComparer.OrdinalIgnoreCase.Compare(firstPage.Title, secondPage.Title);
    }

    private static int CompareFoldersByName(WikiNavNode firstFolder, WikiNavNode secondFolder)
    {
        return StringComparer.OrdinalIgnoreCase.Compare(firstFolder.Name, secondFolder.Name);
    }

}
