# Rendering and Security

LabDocs converts Markdown into HTML when a documentation page is requested.

The renderer supports common Markdown features, including:

- headings and text emphasis;
- ordered, unordered, and task lists;
- links and block quotes;
- tables;
- inline code and fenced code blocks.

## Safe output

Raw HTML inside Markdown files is intentionally disabled. The generated HTML is also sanitized before it reaches the page.

This means HTML-looking examples should be placed inside fenced code blocks:

```html
<section class="example">
    <p>This is displayed as code instead of page markup.</p>
</section>
```

These safeguards allow LabDocs to render useful formatting while filtering executable or unsafe markup.

## Filesystem boundary

Page routes are restricted to lowercase kebab-case slugs, and resolved paths must stay inside the `content` directory. Symbolic links and directory junctions are rejected so they cannot redirect content discovery or page reads outside that directory.

The content directory should still only be writable by trusted administrators.
