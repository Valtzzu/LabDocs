using System.Text.RegularExpressions;

namespace LabDocs.Services;

public static class WikiSlugValidator
{
    // Each path segment contains lowercase letters or numbers, separated by single hyphens.
    private static readonly Regex ValidSlugPattern = new Regex(
        @"^[a-z0-9]+(?:-[a-z0-9]+)*(?:/[a-z0-9]+(?:-[a-z0-9]+)*)*$",
        RegexOptions.CultureInvariant);

    /// <summary>
    /// Determines whether every slug segment follows the lowercase kebab-case convention.
    /// </summary>
    /// <param name="slug">The route slug to validate.</param>
    /// <returns>
    /// <see langword="true"/> when the slug is valid; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsValid(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return false;
        }

        return ValidSlugPattern.IsMatch(slug);
    }
}
