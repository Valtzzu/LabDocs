using LabDocs.Models;
namespace LabDocs.Services;

public class WikiCatalogService {
private readonly string _contentPath;
public WikiCatalogService(IWebHostEnvironment environment) {
   _contentPath = WikiContentDirectory.GetPath(environment);
}

/// <summary>
/// Discovers the valid Markdown pages beneath the content directory.
/// </summary>
/// <returns>The pages available to the documentation site.</returns>
/// <exception cref="DirectoryNotFoundException">
/// The content directory does not exist.
/// </exception>
/// <exception cref="InvalidDataException">
/// A content path is invalid or contains a symbolic link, junction, or other reparse point.
/// </exception>
/// <exception cref="IOException">
/// The content directory could not be read.
/// </exception>
public IEnumerable<WikiPageEntry> GetPages()
    {
        WikiContentDirectory.EnsureExists(_contentPath);

        var pages = new List<WikiPageEntry>();
        string[] filePaths;

        try
        {
            var discoveredFilePaths = new List<string>();
            AddMarkdownFiles(_contentPath, discoveredFilePaths);
            filePaths = discoveredFilePaths.ToArray();
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new IOException(
                $"The wiki content directory at '{_contentPath}' could not be read.",
                exception);
        }
        catch (IOException exception)
        {
            throw new IOException(
                $"An error occurred while reading the wiki content directory at '{_contentPath}'.",
                exception);
        }

        foreach (var filePath in filePaths)
        {
            var relativeFilePath = Path.GetRelativePath(_contentPath, filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string pageTitle = WikiTitleFormatter.FormatSegment(fileName);
            string? pageSlug = Path.ChangeExtension(relativeFilePath, null);

            if (pageSlug is null)
            {
                throw new InvalidDataException($"Could not create a slug for '{relativeFilePath}'.");
            }

            pageSlug = pageSlug.Replace(Path.DirectorySeparatorChar, '/');

            if (WikiSlugValidator.IsValid(pageSlug) == false)
            {
                throw new InvalidDataException(
                    $"The wiki page '{relativeFilePath}' does not follow the lowercase kebab-case naming convention.");
            }

            pages.Add(new WikiPageEntry(pageTitle, pageSlug));
        }
    return pages;
    }

    private static void AddMarkdownFiles(string directoryPath, List<string> discoveredFilePaths)
    {
        string[] entries = Directory.GetFileSystemEntries(directoryPath);

        foreach (string entryPath in entries)
        {
            FileAttributes attributes = File.GetAttributes(entryPath);
            WikiContentDirectory.EnsureIsNotSymbolicLinkOrJunction(entryPath, attributes);

            if ((attributes & FileAttributes.Directory) != 0)
            {
                AddMarkdownFiles(entryPath, discoveredFilePaths);
                continue;
            }

            StringComparison extensionComparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            if (string.Equals(Path.GetExtension(entryPath), ".md", extensionComparison))
            {
                discoveredFilePaths.Add(entryPath);
            }
        }
    }
}
