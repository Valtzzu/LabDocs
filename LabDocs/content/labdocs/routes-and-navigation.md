# Routes and Navigation

LabDocs creates a page route from each Markdown file's path relative to the `content` directory.

For example:

```text
content/guides/getting-started.md
```

becomes:

```text
/docs/guides/getting-started
```

## Navigation behavior

- Folders become navigation groups.
- Pages and folders are ordered alphabetically at each level.
- Hyphens are replaced with spaces in navigation labels and browser titles.
- The current page is highlighted in the navigation.
- A missing or invalid page route returns the internal 404 page.

The application home page remains available at `/`. Markdown documentation pages use the `/docs/` route prefix.
