namespace LabDocs.Services;

public static class WikiTitleFormatter
{
    public static string FormatSegment(string slugSegment)
    {
        string[] words = slugSegment.Split('-', StringSplitOptions.RemoveEmptyEntries);

        for (int index = 0; index < words.Length; index++)
        {
            string word = words[index];
            string firstLetter = word.Substring(0, 1).ToUpperInvariant();
            string remainingLetters = word.Substring(1);
            words[index] = firstLetter + remainingLetters;
        }

        return string.Join(' ', words);
    }

    public static string FormatSlug(string slug)
    {
        string finalSegment = slug;
        int finalSlashIndex = slug.LastIndexOf('/');

        if (finalSlashIndex >= 0)
        {
            finalSegment = slug.Substring(finalSlashIndex + 1);
        }

        return FormatSegment(finalSegment);
    }
}
