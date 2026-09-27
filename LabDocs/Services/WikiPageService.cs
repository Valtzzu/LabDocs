using Ganss.Xss;
using Markdig;

namespace LabDocs.Services;
  
public class WikiPageService {
private readonly string _contentPath;
private readonly MarkdownPipeline _markdownPipeline;
private readonly HtmlSanitizer _htmlSanitizer;


public WikiPageService(IWebHostEnvironment environment, HtmlSanitizer htmlSanitizer) {
    _contentPath = WikiContentDirectory.GetPath(environment);
    _htmlSanitizer = htmlSanitizer;
    _markdownPipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .Build();
}

    /// <summary>
    /// Loads a Markdown page and converts it to sanitized HTML.
    /// </summary>
    /// <param name="slug">The lowercase kebab-case route beneath the content directory.</param>
    /// <returns>
    /// The sanitized HTML, or <see langword="null"/> when the slug is invalid or the page does not exist.
    /// </returns>
    /// <exception cref="DirectoryNotFoundException">
    /// The content directory does not exist.
    /// </exception>
    /// <exception cref="InvalidDataException">
    /// The content directory or page path contains a symbolic link, junction, or other reparse point.
    /// </exception>
    /// <exception cref="IOException">
    /// The page could not be read.
    /// </exception>
    public async Task<string?> LoadPageAsync(string slug) 
    {
            WikiContentDirectory.EnsureExists(_contentPath);

            if (WikiSlugValidator.IsValid(slug) == false)
            {
                return null;
            }

            var candidatePath = Path.GetFullPath($"{slug}.md", _contentPath);

            if (!IsPathWithinContentRoot(candidatePath)) {
                return null;
            }
 
            if (!System.IO.File.Exists(candidatePath)) {
                return null;
            }
            
            try
            {
                WikiContentDirectory.EnsurePathContainsNoSymbolicLinks(
                    _contentPath,
                    candidatePath);

                var markdownContent = await System.IO.File.ReadAllTextAsync(candidatePath);
                var renderedHtml = Markdown.ToHtml(markdownContent, _markdownPipeline);
                return _htmlSanitizer.Sanitize(renderedHtml);
            }
            catch (FileNotFoundException)
            {
                // The file may have been removed after the existence check.
                return null;
            }
            catch (UnauthorizedAccessException exception)
            {
                throw new IOException(
                    $"The wiki page at '{candidatePath}' could not be read.",
                    exception);
            }
            catch (IOException exception)
            {
                throw new IOException(
                    $"An error occurred while reading the wiki page at '{candidatePath}'.",
                    exception);
            }
        }

private bool IsPathWithinContentRoot(string candidatePath) {
        if (System.OperatingSystem.IsWindows())
        {
            return candidatePath.StartsWith(_contentPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        } else
        {
            return candidatePath.StartsWith(_contentPath + Path.DirectorySeparatorChar, StringComparison.Ordinal);
        }
    }

}
