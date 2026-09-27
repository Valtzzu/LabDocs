namespace LabDocs.Services;

public static class WikiContentDirectory
{
    public static string GetPath(IWebHostEnvironment environment)
    {
        return Path.GetFullPath("content", environment.ContentRootPath);
    }

    /// <summary>
    /// Ensures that the content directory exists and is not a symbolic link or junction.
    /// </summary>
    /// <param name="contentPath">The absolute content-directory path.</param>
    /// <exception cref="DirectoryNotFoundException">
    /// The content directory does not exist.
    /// </exception>
    /// <exception cref="InvalidDataException">
    /// The content directory is a symbolic link, junction, or other reparse point.
    /// </exception>
    public static void EnsureExists(string contentPath)
    {
        if (Directory.Exists(contentPath) == false)
        {
            throw new DirectoryNotFoundException(
                $"The wiki content directory was not found at '{contentPath}'. " +
                "Ensure that the content directory is included in the application deployment.");
        }

        EnsureIsNotSymbolicLinkOrJunction(contentPath);
    }

    /// <summary>
    /// Ensures that a target remains inside the content directory and that none of its path entries are links.
    /// </summary>
    /// <param name="contentPath">The absolute content-directory path.</param>
    /// <param name="targetPath">The absolute path to validate.</param>
    /// <exception cref="InvalidDataException">
    /// The target is outside the content directory or contains a symbolic link, junction, or other reparse point.
    /// </exception>
    public static void EnsurePathContainsNoSymbolicLinks(string contentPath, string targetPath)
    {
        string relativePath = Path.GetRelativePath(contentPath, targetPath);

        if (Path.IsPathRooted(relativePath) ||
            relativePath == ".." ||
            relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
            relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"The path '{targetPath}' is outside the wiki content directory.");
        }

        string currentPath = contentPath;
        char[] directorySeparators = new char[]
        {
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar
        };

        string[] pathSegments = relativePath.Split(
            directorySeparators,
            StringSplitOptions.RemoveEmptyEntries);

        foreach (string pathSegment in pathSegments)
        {
            currentPath = Path.Combine(currentPath, pathSegment);
            EnsureIsNotSymbolicLinkOrJunction(currentPath);
        }
    }

    /// <summary>
    /// Ensures that a filesystem entry is not a symbolic link, junction, or other reparse point.
    /// </summary>
    /// <param name="path">The filesystem entry to inspect.</param>
    /// <exception cref="InvalidDataException">
    /// The entry is a symbolic link, junction, or other reparse point.
    /// </exception>
    public static void EnsureIsNotSymbolicLinkOrJunction(string path)
    {
        FileAttributes attributes = File.GetAttributes(path);
        EnsureIsNotSymbolicLinkOrJunction(path, attributes);
    }

    internal static void EnsureIsNotSymbolicLinkOrJunction(
        string path,
        FileAttributes attributes)
    {
        if (IsSymbolicLinkOrJunction(attributes))
        {
            throw new InvalidDataException(
                $"Symbolic links and directory junctions are not supported in the wiki content directory: '{path}'.");
        }
    }

    /// <summary>
    /// Determines whether file attributes identify an entry that LabDocs must treat as a link.
    /// </summary>
    /// <param name="attributes">The attributes of the filesystem entry.</param>
    /// <returns>
    /// <see langword="true"/> when the reparse-point attribute is present; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool IsSymbolicLinkOrJunction(FileAttributes attributes)
    {
        return (attributes & FileAttributes.ReparsePoint) != 0;
    }
}
