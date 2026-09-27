using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LabDocs.Services;

namespace LabDocs.Pages;

public class WikiModel : PageModel 
{

    private readonly WikiPageService _wikiPageService;
    public WikiModel(WikiPageService wikiPageService)
    {
        _wikiPageService = wikiPageService;
    }

    public string HtmlContent { get; private set; } = "";

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var htmlContent = await _wikiPageService.LoadPageAsync(slug);

        if (htmlContent is null)
        {
            return NotFound(); 
        }

        HtmlContent = htmlContent;
        ViewData["Title"] = WikiTitleFormatter.FormatSlug(slug);
        return Page();
    }

}
